using FamilyApp.Data;
using Microsoft.EntityFrameworkCore;

public interface ICurrentWorkshopService
{
    Task<int?> GetCurrentWorkshopIdAsync(CancellationToken ct = default);
    Task<bool> UserHasAccessAsync(int workshopId, CancellationToken ct = default);
}

public class CurrentWorkshopService : ICurrentWorkshopService
{
    private readonly dbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _http;
    private readonly IWebHostEnvironment _env;

    public CurrentWorkshopService(
        dbContext db,
        ICurrentUserService currentUser,
        IHttpContextAccessor http,
        IWebHostEnvironment env)
    {
        _db = db;
        _currentUser = currentUser;
        _http = http;
        _env = env;
    }

    public async Task<int?> GetCurrentWorkshopIdAsync(CancellationToken ct = default)
    {
        var uid = _currentUser.UserIdInt;
        if (!uid.HasValue)
            return null;

        var requestedId = GetRequestedWorkshopId();
        if (requestedId.HasValue && await UserHasAccessAsync(requestedId.Value, ct))
            return requestedId.Value;

        if (IsSuperAdmin())
        {
            return await _db.Workshops
                .AsNoTracking()
                .Where(x => x.Activo)
                .OrderBy(x => x.Id)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync(ct);
        }

        var assignedWorkshopId = await _db.WorkshopUsers
            .AsNoTracking()
            .Where(x => x.UserId == uid.Value && x.Activo && x.Workshop.Activo)
            .OrderBy(x => x.WorkshopId)
            .Select(x => (int?)x.WorkshopId)
            .FirstOrDefaultAsync(ct);

        if (assignedWorkshopId.HasValue)
            return assignedWorkshopId.Value;

        if (_env.IsDevelopment())
            return await GetFirstActiveWorkshopAsync(ct);

        return null;
    }

    public async Task<bool> UserHasAccessAsync(int workshopId, CancellationToken ct = default)
    {
        var uid = _currentUser.UserIdInt;
        if (!uid.HasValue)
            return false;

        if (IsSuperAdmin())
        {
            return await _db.Workshops
                .AsNoTracking()
                .AnyAsync(x => x.Id == workshopId && x.Activo, ct);
        }

        return await _db.WorkshopUsers
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId == uid.Value &&
                x.WorkshopId == workshopId &&
                x.Activo &&
                x.Workshop.Activo,
                ct
            );
    }

    private int? GetRequestedWorkshopId()
    {
        var header = _http.HttpContext?.Request.Headers["X-Workshop-Id"].FirstOrDefault();
        return int.TryParse(header, out var id) && id > 0 ? id : null;
    }

    private bool IsSuperAdmin()
    {
        return _http.HttpContext?.User?.IsInRole("superadmin") == true;
    }

    private Task<int?> GetFirstActiveWorkshopAsync(CancellationToken ct)
    {
        return _db.Workshops
            .AsNoTracking()
            .Where(x => x.Activo)
            .OrderBy(x => x.Id)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(ct);
    }
}
