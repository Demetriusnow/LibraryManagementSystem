using System.Text.Json.Serialization;
using Library.BLL.Services;
using Library.Core.Interfaces;
using Library.DAL.Data;
using Library.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(TimeProvider.System);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<LibraryDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddScoped<IAuthorRepository, EfAuthorRepository>();
    builder.Services.AddScoped<IBookCopyRepository, EfBookCopyRepository>();
    builder.Services.AddScoped<ILoanRepository, EfLoanRepository>();
    builder.Services.AddScoped<IBookRepository, EfBookRepository>();
    builder.Services.AddScoped<IReaderRepository, EfReaderRepository>();
    builder.Services.AddScoped<IReservationRepository, EfReservationRepository>();
    builder.Services.AddScoped<INotificationService, EfNotificationService>();
}
else
{
    builder.Services.AddSingleton<IAuthorRepository, InMemoryAuthorRepository>();
    builder.Services.AddSingleton<IBookCopyRepository, InMemoryBookCopyRepository>();
    builder.Services.AddSingleton<ILoanRepository, InMemoryLoanRepository>();
    builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
    builder.Services.AddSingleton<IReaderRepository, InMemoryReaderRepository>();
    builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
    builder.Services.AddSingleton<INotificationService, InMemoryNotificationService>();
}

builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IAuthorService, AuthorService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

if (!string.IsNullOrWhiteSpace(connectionString))
{
    await InitializeDatabaseAsync(app.Services, app.Logger);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue("EnableSwagger", true))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Configuration.GetValue("DisableHttpsRedirection", false))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task InitializeDatabaseAsync(IServiceProvider services, ILogger logger)
{
    const int maxAttempts = 5;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();

            await dbContext.Database.MigrateAsync();
            await DbSeeder.SeedAsync(dbContext);
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                exception,
                "Database initialization attempt {Attempt}/{MaxAttempts} failed. Retrying in 2 seconds...",
                attempt,
                maxAttempts);

            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}
