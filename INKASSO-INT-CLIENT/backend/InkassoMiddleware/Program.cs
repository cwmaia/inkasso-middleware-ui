using InkassoMiddleware.Models;
using InkassoMiddleware.Soap;

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

// Add WCF Client
builder.Services.AddSingleton(sp =>
{
    var binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly);
    binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;

    var endpoint = new System.ServiceModel.EndpointAddress("http://demo-inkasso.azurewebsites.net/SOAP/IOBS/IcelandicOnlineBankingClaimsSoap.svc");

    var client = new IcelandicOnlineBankingClaimsSoapClient(binding, endpoint);
    client.ClientCredentials.UserName.UserName = "servicetest";
    client.ClientCredentials.UserName.Password = "znvwYV5";
    return client;
});

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
app.MapPost("/query-claims", async (IcelandicOnlineBankingClaimsSoapClient client, ClaimQueryRequest req) =>
{
    try
    {
        var query = new InkassoMiddleware.Soap.ClaimsQuery
        {
            EntryFrom = 1,
            EntryFromSpecified = true,
            EntryTo = 10,
            EntryToSpecified = true,
            Claimant = req.ClaimantId,
            Period = new InkassoMiddleware.Soap.ClaimsQueryDateSpan
            {
                DateFrom = req.FromDate,
                DateTo = req.ToDate,
                DateFromSpecified = true,
                DateToSpecified = true,
                DateSpanReferenceDate = InkassoMiddleware.Soap.DateSpanReferenceDate.CreationDate,
                DateSpanReferenceDateSpecified = true
            }
        };

        var result = await client.QueryClaimsAsync(query);
        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"SOAP Error: {ex.Message}");
    }
});

// Add test connection endpoint
app.MapGet("/test-connection", async (IcelandicOnlineBankingClaimsSoapClient client) =>
{
    try
    {
        var query = new InkassoMiddleware.Soap.ClaimsQuery
        {
            EntryFrom = 1,
            EntryFromSpecified = true,
            EntryTo = 10,
            EntryToSpecified = true,
            Claimant = "1021021020",
            Period = new InkassoMiddleware.Soap.ClaimsQueryDateSpan
            {
                DateFrom = DateTime.UtcNow.AddYears(-1),
                DateTo = DateTime.UtcNow,
                DateFromSpecified = true,
                DateToSpecified = true,
                DateSpanReferenceDate = InkassoMiddleware.Soap.DateSpanReferenceDate.CreationDate,
                DateSpanReferenceDateSpecified = true
            }
        };

        var result = await client.QueryClaimsAsync(query);
        return Results.Ok(new { status = "success", message = "Connected and received response from Inkasso API." });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            title: "Connection Error",
            detail: $"Failed to connect to Inkasso API. Error: {ex.Message}"
        );
    }
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
