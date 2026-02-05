using Microsoft.EntityFrameworkCore;
using Auth_Level1_Basic.Data;
using Auth_Level1_Basic.Models;
using Auth_Level1_Basic.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=auth.db"));

// Add controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Auto-create database and seed default user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username = "admin",
            Password = "1234"
        });
        db.SaveChanges();
    }
}

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Redirect root "/" to Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Use Basic Auth middleware for all endpoints except Swagger and root
app.UseWhen(context =>
    !context.Request.Path.StartsWithSegments("/swagger") &&
    !context.Request.Path.Equals("/"),
    appBuilder =>
    {
        appBuilder.UseMiddleware<BasicAuthMiddleware>();
    });

// Map controllers
app.MapControllers();

app.Run();
