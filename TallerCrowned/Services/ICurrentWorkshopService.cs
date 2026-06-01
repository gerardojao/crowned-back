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

    public CurrentWorkshopService(
        dbContext db,
        ICurrentUserService currentUser,
        IHttpContextAccessor http)
    {
        _db = db;
        _currentUser = currentUser;
        _http = http;
    }

    public async Task<int?> GetCurrentWorkshopIdAsync(CancellationToken ct = default)
    {
        var uid = _currentUser.UserIdInt;
        if (!uid.HasValue)
            return null;

        var requestedId = GetRequestedWorkshopId();
        if (requestedId.HasValue && await UserHasAccessAsync(requestedId.Value, ct))
            return requestedId.Value;

        return await _db.WorkshopUsers
            .AsNoTracking()
            .Where(x => x.UserId == uid.Value && x.Activo && x.Workshop.Activo)
            .OrderBy(x => x.WorkshopId)
            .Select(x => (int?)x.WorkshopId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> UserHasAccessAsync(int workshopId, CancellationToken ct = default)
    {
        var uid = _currentUser.UserIdInt;
        if (!uid.HasValue)
            return false;

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
}
