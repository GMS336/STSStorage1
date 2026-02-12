using Microsoft.EntityFrameworkCore;

using STSStorage1.Middleware;

using STSStorage1.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<STSStorage1Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("STSStorage1Context")
    ?? throw new InvalidOperationException("Connection string 'STSStorage1Context' not found.")));

builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(1); // Set session timeout
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Make the session cookie essential
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// **STATIC FILES MUST BE EARLY**
app.UseStaticFiles();

// Session and custom middleware after static files
app.UseSession();
app.UseMiddleware<SessionTimeoutMiddleware>();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=STSHome}/{id?}");

app.Run();