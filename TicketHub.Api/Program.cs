using Azure.Storage.Queues;
using FluentValidation;
using TicketHub.Api.Services;
using TicketHub.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TicketHub API", Version = "v1" });
});

// Add User Secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PurchaseRequestValidator>();

// Add Azure Queue Service
builder.Services.AddSingleton(provider =>
{
    var connectionString = provider.GetRequiredService<IConfiguration>()["AzureStorage:ConnectionString"]
        ?? throw new InvalidOperationException("Missing Azure Storage connection string");
    return new QueueClient(connectionString, "tickethub");
});

builder.Services.AddScoped<QueueService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketHub API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();