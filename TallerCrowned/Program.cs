using FamilyApp.Data;
using FamilyApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========= 1) SERVICES (ANTES del Build) =========
var env = builder.Environment; // <- úsalo en AddJwtBearer

builder.Services.AddControllers();

// DB
builder.Services.AddDbContext<dbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Servicios propios
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRepository, Repository<dbContext>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProdCors", p => p
        .WithOrigins(
            "https://invoice.familyapp.store",
            "https://familyapp.store",
            "https://www.familyapp.store",
            "https://www.invoice.familyapp.store"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());

    options.AddPolicy("DevCors", p => p
        .WithOrigins(
            "http://localhost:5173", "http://localhost:5174",
            "https://localhost:5173", "https://localhost:5174"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());
});

// Auth (JWT)
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = false;

        // Relaja issuer/audience SOLO en dev
        var validateIssuer = !env.IsDevelopment();
        var validateAudience = !env.IsDevelopment();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = validateIssuer,
            ValidateAudience = validateAudience,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ClaimTypes.Role
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token))
                {
                    var cookieToken = ctx.HttpContext.Request.Cookies[".familyapp.auth"];
                    if (!string.IsNullOrEmpty(cookieToken)) ctx.Token = cookieToken;
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                // BYPASS de validación JTI en DEV
                var webEnv = ctx.HttpContext.RequestServices
                                   .GetRequiredService<IWebHostEnvironment>();
                if (webEnv.IsDevelopment()) return;

                // (tu validación real contra DB aquí si quieres sesión única)
                try
                {
                    var db = ctx.HttpContext.RequestServices.GetRequiredService<dbContext>();
                    var principal = ctx.Principal!;
                    var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
                    var jti = principal.FindFirstValue("jti");

                    if (string.IsNullOrEmpty(sub) || string.IsNullOrEmpty(jti) || !int.TryParse(sub, out var userId))
                    { ctx.Fail("Token inválido."); return; }

                    var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    if (user == null || user.ActiveSessionJti == null
                        || user.ActiveSessionJti.ToString() != jti
                        || (user.ActiveSessionExpiresAt.HasValue && user.ActiveSessionExpiresAt < DateTime.UtcNow))
                    { ctx.Fail("Sesión no válida o caducada."); return; }
                }
                catch { ctx.Fail("Error validando la sesión."); }
            }
        };
    });

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Taller Crowned API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega SOLO el token (sin 'Bearer ')"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ========= 2) BUILD =========
var app = builder.Build();

// ========= 3) MIDDLEWARE (DESPUÉS del Build) =========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(app.Environment.IsDevelopment() ? "DevCors" : "ProdCors");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
