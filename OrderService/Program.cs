using OrderService.Services;
using Microsoft.EntityFrameworkCore;
using OrderService.Persistance;
using OrderService.Persistance.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register repository and service
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddHttpClient<ICartService, CartService>(client =>
{
    client.BaseAddress = new Uri("https://your-product-service-url/api/");
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// Apply database migrations only if there are pending migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<AppDbContext>();

    // Check if there are pending migrations
    var pendingMigrations = dbContext.Database.GetPendingMigrations().ToList();
    if (pendingMigrations.Any())
    {
        Console.WriteLine($"Applying {pendingMigrations.Count} pending migrations");
        dbContext.Database.Migrate();
    }
    else
    {
        Console.WriteLine("No pending migrations to apply");
    }
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
