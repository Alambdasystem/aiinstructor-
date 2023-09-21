using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// If you want to serve static files, ensure to use HTTPS
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    // Enable HSTS (HTTP Strict Transport Security)
    app.UseHsts();
}

app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

// Configure Kestrel to listen on all IPs and use HTTPS
var host = new WebHostBuilder()
    .UseKestrel(options =>
    {
        options.ListenAnyIP(5001);
    })
    .UseContentRoot(Directory.GetCurrentDirectory())
    .UseIISIntegration()
    .UseStartup<Startup>()
    .Build();

host.Run();
