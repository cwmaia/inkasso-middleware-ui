using System.IO;
using InkassoMiddleware;
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
    var binding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.Transport);
    binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Basic;

    var endpoint = new System.ServiceModel.EndpointAddress("https://demo-inkasso.azurewebsites.net/SOAP/IOBS/IcelandicOnlineBankingClaimsSoap.svc");

    var client = new IcelandicOnlineBankingClaimsSoapClient(binding, endpoint);
    client.ClientCredentials.UserName.UserName = "testdev.inkasso";
    client.ClientCredentials.UserName.Password = "$ILove2Code";
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

// Add raw SOAP test endpoint
app.MapGet("/test-raw-soap", async (HttpRequest request) =>
{
    try
    {
        // Check if we should use the mock endpoint
        bool useMock = request.Query.ContainsKey("useMock") || InkassoRawSoapClient.IsUsingMockEndpoint();
        
        // If useMock is true and INKASSO_URL is not set, set it temporarily
        string? originalUrl = null;
        if (useMock && Environment.GetEnvironmentVariable("INKASSO_URL") == null)
        {
            originalUrl = Environment.GetEnvironmentVariable("INKASSO_URL");
            Environment.SetEnvironmentVariable("INKASSO_URL", "http://localhost:5284/mock-inkasso");
        }
        
        try
        {
            // Create a StringWriter to capture console output
            var originalOut = Console.Out;
            using var stringWriter = new StringWriter();
            Console.SetOut(stringWriter);

            // Test parameters
            string claimantId = "1021021020";
            DateTime fromDate = new DateTime(2024, 1, 1);
            DateTime toDate = new DateTime(2024, 12, 31);

            // Query claims
            string soapResponse = await InkassoRawSoapClient.QueryClaimsAsync(claimantId, fromDate, toDate);
            
            // Parse claims from the response
            var claims = InkassoRawSoapClient.ParseClaimsFromSoapResponse(soapResponse);

            // Restore console output
            Console.SetOut(originalOut);

            // Get the captured output
            string testOutput = stringWriter.ToString();

            return Results.Ok(new { 
                status = "success", 
                message = "Raw SOAP test executed", 
                usingMock = useMock,
                claims = claims,
                output = testOutput 
            });
        }
        finally
        {
            // Restore the original endpoint URL if we changed it
            if (useMock && originalUrl != null)
            {
                Environment.SetEnvironmentVariable("INKASSO_URL", originalUrl);
            }
        }
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            title: "Raw SOAP Test Error",
            detail: $"Failed to execute raw SOAP test. Error: {ex.Message}"
        );
    }
});

// Add mock SOAP test endpoint
app.MapGet("/test-mock-soap", async () =>
{
    try
    {
        // Create a StringWriter to capture console output
        var originalOut = Console.Out;
        using var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);

        // Run the mock test
        await TestSoapClient.RunMockTest();

        // Restore console output
        Console.SetOut(originalOut);

        // Get the captured output
        string testOutput = stringWriter.ToString();

        return Results.Ok(new { 
            status = "success", 
            message = "Mock SOAP test executed", 
            output = testOutput 
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            title: "Mock SOAP Test Error",
            detail: $"Failed to execute mock SOAP test. Error: {ex.Message}"
        );
    }
});

// Mock Inkasso SOAP endpoint for testing
app.MapPost("/mock-inkasso", async (HttpRequest request) =>
{
    try
    {
        // Read the incoming SOAP body but don't log it to avoid console issues
        // Just drain the request body to ensure it's processed
        using var reader = new StreamReader(request.Body);
        await reader.ReadToEndAsync();
        
        // Fixed SOAP 1.2 response
        string mockResponse = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope""
               xmlns:iobs=""http://IcelandicOnlineBanking/2005/12/01/Claims"">
  <soap:Body>
    <iobs:QueryClaimsResponse>
      <iobs:QueryClaimsResult>
        <iobs:Claim>
          <iobs:ClaimId>123456789</iobs:ClaimId>
          <iobs:Reference>ABC-2024</iobs:Reference>
          <iobs:DueDate>2024-06-01</iobs:DueDate>
          <iobs:Capital>99000</iobs:Capital>
        </iobs:Claim>
        <iobs:Claim>
          <iobs:ClaimId>987654321</iobs:ClaimId>
          <iobs:Reference>XYZ-2024</iobs:Reference>
          <iobs:DueDate>2024-08-15</iobs:DueDate>
          <iobs:Capital>112500</iobs:Capital>
        </iobs:Claim>
      </iobs:QueryClaimsResult>
    </iobs:QueryClaimsResponse>
  </soap:Body>
</soap:Envelope>";

        // Return the mock response with appropriate headers
        return Results.Text(
            mockResponse,
            contentType: "application/soap+xml; charset=utf-8",
            statusCode: 200
        );
    }
    catch (Exception ex)
    {
        return Results.Problem(
            statusCode: 500,
            title: "Mock SOAP Endpoint Error",
            detail: $"Error processing request: {ex.Message}"
        );
    }
});

// Add comment to show how to switch InkassoRawSoapClient to use this endpoint for testing
// To use the mock endpoint, set the INKASSO_URL environment variable:
// Environment.SetEnvironmentVariable("INKASSO_URL", "http://localhost:5284/mock-inkasso");

// Add a direct mock endpoint that returns JSON data for testing
app.MapGet("/mock-inkasso-json", () =>
{
    // Create mock claims data that matches the structure in the SOAP response
    var mockClaims = new List<InkassoMiddleware.Models.ClaimResult>
    {
        new InkassoMiddleware.Models.ClaimResult
        {
            ClaimId = "123456789",
            Reference = "ABC-2024",
            DueDate = DateTime.Parse("2024-06-01"),
            Capital = 99000
        },
        new InkassoMiddleware.Models.ClaimResult
        {
            ClaimId = "987654321",
            Reference = "XYZ-2024",
            DueDate = DateTime.Parse("2024-08-15"),
            Capital = 112500
        }
    };

    return Results.Ok(mockClaims);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
