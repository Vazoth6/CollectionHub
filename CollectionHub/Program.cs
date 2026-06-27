using System.Reflection;
using CollectionHub.Data;
using CollectionHub.Data.Seed;
using CollectionHub.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

/* Swagger */
builder.Services.AddEndpointsApiExplorer();

// Carteira Virtual
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CollectionHub API",
        Version = "v1",
        Description = "API da aplicação CollectionHub"
    });

    // Inclui os comentários XML dos controllers no Swagger, quando o ficheiro existir no build.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CollectionHub API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

await app.SeedDatabaseAsync();

app.Run();