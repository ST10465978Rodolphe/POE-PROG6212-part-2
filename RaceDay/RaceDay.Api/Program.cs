using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// SQL Server through Entity Framework Core (Code First)
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Server-side session: remembers the logged-in UserId and Role between requests
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
});

// Azure Blob Storage for image uploads (the service is only created when an upload endpoint is used)
builder.Services.AddSingleton<IImageStorageService, AzureBlobImageService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseSession();

app.MapControllers();

app.Run();

// Needed so the test project can start the API (WebApplicationFactory)
public partial class Program { }