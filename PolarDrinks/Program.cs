using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PolarDrinks.Compartilhado.Repositories;
using PolarDrinks.Compartilhado.Mvc;
using PolarDrinks.Data;
using PolarDrinks.Modules.Compras.Repositories;
using PolarDrinks.Modules.Compras.Services;
using PolarDrinks.Modules.Dashboard.Services;
using PolarDrinks.Modules.Dashboard.Repositories;
using PolarDrinks.Modules.Estoque.Repositories;
using PolarDrinks.Modules.Estoque.Services;
using PolarDrinks.Modules.Loja.Jobs;
using PolarDrinks.Modules.Loja.Repositories;
using PolarDrinks.Modules.Loja.Services;
using PolarDrinks.Modules.Usuarios.Models;
using PolarDrinks.Modules.Usuarios.Repositories;
using PolarDrinks.Modules.Usuarios.Services;
using PolarDrinks.Modules.Vendas.Repositories;
using PolarDrinks.Modules.Vendas.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Procura as views dentro da pasta do módulo do controller (Modules/<Modulo>/Views/...)
        options.ViewLocationExpanders.Add(new ModuloViewLocationExpander());
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IMovimentacaoEstoqueRepository, MovimentacaoEstoqueRepository>();
builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IVendaRepository, VendaRepository>();
builder.Services.AddScoped<ICompraEstoqueRepository, CompraEstoqueRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();

builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IVendaService, VendaService>();
builder.Services.AddScoped<ICompraEstoqueService, CompraEstoqueService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMovimentacaoEstoqueService, MovimentacaoEstoqueService>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IArmazenamentoService, ArmazenamentoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRelatorioRepository, RelatorioRepository>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

builder.Services.AddHostedService<ExpiracaoPedidosJob>();


builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IClienteAuthService, ClienteAuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICarrinhoRepository, CarrinhoRepository>();
builder.Services.AddScoped<ICarrinhoService, CarrinhoService>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<IPedidoService, PedidoService>();

// Autenticação JWT (Loja Online / Clientes)
var jwtChave = builder.Configuration["Jwt:ChaveSecreta"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Emissor"],
        ValidAudience = builder.Configuration["Jwt:Emissor"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtChave))
    };
});
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});


var app = builder.Build();

// ===== SEED: cria o admin padrão se não existir nenhum usuário =====
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();

        if (!db.Usuarios.Any())
        {
            db.Usuarios.Add(new UsuarioModel
            {
                UsuarioNome = "Admin",
                UsuarioLogin = "admin",
                UsuarioSenhaHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                UsuarioPerfil = "Admin",   
                UsuarioAtivo = true,
                UsuarioCriadoEm = DateTime.Now
            });
            db.SaveChanges();
    }
    }
// ===================================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

var culture = new CultureInfo("en-US");

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culture),
    SupportedCultures = new[] { culture },
    SupportedUICultures = new[] { culture }
};

app.UseRequestLocalization(localizationOptions);
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

app.Run();