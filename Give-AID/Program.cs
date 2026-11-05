using Microsoft.EntityFrameworkCore;
using Give_AID;
using Give_AID.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure database connection with SQL Server
var connectionString = builder.Configuration.GetConnectionString("Give_AIDContext") 
    ?? throw new InvalidOperationException("Connection string 'Give_AIDContext' not found.");

builder.Services.AddDbContext<Give_AIDContext>(options => 
    options.UseSqlServer(connectionString));

// Configure Session support (same pattern as LMS project)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add MVC controllers and views
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Show a friendly 404 page when a route is not found (e.g. /Home/Error/404)
app.UseStatusCodePagesWithReExecute("/Home/Error404");

app.UseHttpsRedirection();
app.UseRouting();

// Enable Session middleware before authorization and endpoints
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Seed default administrator and baseline CMS content if not already present
DbInitializer.SeedData(app);

app.Run();
