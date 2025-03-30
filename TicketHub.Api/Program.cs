using Azure.Storage.Queues;
using FluentValidation;
using TicketHub.Api.Services;
using TicketHub.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsDevelopment()){
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddValidatorsFromAssemblyContaining<PurchaseRequestValidator>();

builder.Services.AddSingleton(provider =>{
    var connectionString = provider.GetRequiredService<IConfiguration>()["AzureStorage:ConnectionString"]
        ?? throw new InvalidOperationException("Missing Azure Storage connection string");
    return new QueueClient(connectionString, "tickethub");
});

builder.Services.AddScoped<QueueService>();

var app = builder.Build();

if (app.Environment.IsDevelopment()){
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();