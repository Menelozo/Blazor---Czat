using Czat.Server;
using Czat.Server.Data;
using Czat.Server.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = TokenService.GetTokenValidationParameters(builder.Configuration);

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = (context) =>
        {
            if (context.Request.Path.StartsWithSegments("/hubs/czat"))
            {
                var jwt = context.Request.Query["access_token"];
                if (!string.IsNullOrWhiteSpace(jwt))
                {
                    context.Token = jwt;
                }
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddDbContext<ChatContext>(options => options.UseMySql(
        builder.Configuration.GetConnectionString("Chat"),
        new MySqlServerVersion(new Version(5, 7, 32)) // MySQL version 5.7.32
    ));

builder.Services.AddTransient<TokenService>();

builder.Services.AddSignalR();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<BlazingChatHub>("/hubs/czat");
app.MapFallbackToFile("index.html");

app.Run();
