using UniMove.Repositories.Dapper;
using UniMove.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using UniMove.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(opcoes =>
{
    // Toda página exige login, exceto as marcadas com [AllowAnonymous]
    var politica = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    opcoes.Filters.Add(new AuthorizeFilter(politica));
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Account/Login";
        opcoes.AccessDeniedPath = "/Account/AccessDenied";
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddScoped<IConexaoFactory, ConexaoFactory>();
builder.Services.AddScoped<IInstituicaoRepository, InstituicaoRepository>();
builder.Services.AddScoped<IMotoristaRepository, MotoristaRepository>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IRotaRepository, RotaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<AutenticacaoService>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<ReservaService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var fabrica = escopo.ServiceProvider.GetRequiredService<IConexaoFactory>();
    var caminhoScript = Path.Combine(app.Environment.ContentRootPath, "Database", "script.sql");
    InicializadorBanco.Inicializar(fabrica, caminhoScript);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();