using InkassoMiddleware.Models;
using InkassoMiddleware.IOBS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Register SOAP client
builder.Services.AddSingleton<InkassoIOBSClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// Create claim endpoint
app.MapPost("/create-claim", (ClaimBatchItem claim) =>
{
    // Log the received claim
    Console.WriteLine($"Received claim: {System.Text.Json.JsonSerializer.Serialize(claim)}");
    
    return Results.Ok(new { message = "Claim received successfully" });
});

// Query claims endpoint
app.MapPost("/query-claims", async (InkassoIOBSClient client, ClaimQueryRequest query) =>
{
    try
    {
        var claims = await client.QueryClaimsAsync(new ClaimsQuery
        {
            EntryFrom = 1,
            EntryFromSpecified = true,
            EntryTo = 1000,
            EntryToSpecified = true,
            ClaimantId = query.ClaimantId,
            Period = new ClaimsQueryDateSpan
            {
                DateFrom = query.FromDate,
                DateTo = query.ToDate,
                DateFromSpecified = true,
                DateToSpecified = true,
                DateSpanReferenceDate = DateSpanReferenceDate.CreationDate,
                DateSpanReferenceDateSpecified = true
            }
        });

        return Results.Ok(claims);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
