using MedCitas.Core.Configuration;
using MedCitas.Core.Interfaces;
using MedCitas.Core.Services;
using MedCitas.Infrastructure.Repositories;
using MedCitas.Infrastructure.Services;
using MedCitas.Infrastructure.DataDb;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// CONFIGURACIÓN DE SERVICIOS
// ---------------------------------------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // ? SEGURIDAD: Solo HTTPS
});

// ? CONFIGURACIÓN DE EMAIL CON OPTIONS PATTERN
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("Email"));

// Leer la contraseña desde User Secrets
var dbPassword = builder.Configuration["ConnectionStrings:DbPassword"];
var baseConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Construir la cadena de conexión completa
var connectionString = string.IsNullOrEmpty(dbPassword)
    ? baseConnectionString
    : $"{baseConnectionString};Password={dbPassword}";

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", false);

// Configurar DbContext con la cadena de conexión completa
builder.Services.AddDbContext<MedCitasDbContext>(options =>
    options.UseNpgsql(connectionString));

// ---------------------------------------------------------
// INYECCIÓN DE DEPENDENCIAS
// ---------------------------------------------------------
builder.Services.AddScoped<IPacienteRepository, EfPacienteRepositorio>();
builder.Services.AddScoped<IAppointmentRepository, EfAppointmentRepository>();
builder.Services.AddScoped<IDoctorRepository, EfDoctorRepository>();
builder.Services.AddScoped<ISpecialtyRepository, EfSpecialtyRepository>();
builder.Services.AddScoped<IAdminRepository, EfAdminRepository>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<PacienteService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<AdminService>();

// ? SEGURIDAD: Agregar AntiForgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// ---------------------------------------------------------
// CONSTRUCCIÓN DE LA APP
// ---------------------------------------------------------
var app = builder.Build();

// ---------------------------------------------------------
// CONFIGURACIÓN DEL PIPELINE HTTP
// ---------------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// ? SEGURIDAD: Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.MapControllerRoute(
  name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

await app.RunAsync();
