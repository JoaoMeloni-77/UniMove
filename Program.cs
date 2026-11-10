using UniMove.Repositories.Dapper;
using UniMove.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IConexaoFactory, ConexaoFactory>();

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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
