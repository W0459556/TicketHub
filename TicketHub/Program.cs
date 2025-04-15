using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TicketHub API",
        Version = "v1",
        Description = "Asst 3",
    });

    try
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading XML comments: {ex.Message}");
    }
});

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TicketHub API v1");
    c.DisplayRequestDuration();
    c.EnableDeepLinking();
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

try
{
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"Application failed to start: {ex}");
    throw;
}