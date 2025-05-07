using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InkassoMiddleware
{
    /// <summary>
    /// Test program for the InkassoRawSoapClient
    /// </summary>
    public class TestSoapClient
    {
        /// <summary>
        /// Main entry point for testing the raw SOAP client against the real Inkasso service
        /// </summary>
        public static async Task RunTest()
        {
            Console.WriteLine("=== Inkasso Raw SOAP Client Test ===");
            Console.WriteLine("Testing connection to Inkasso SOAP service...");
            Console.WriteLine();

            try
            {
                // Test parameters as specified in the requirements
                string claimantId = "1021021020";
                DateTime fromDate = new DateTime(2024, 1, 1);
                DateTime toDate = new DateTime(2024, 12, 31);

                Console.WriteLine($"Querying claims for claimant: {claimantId}");
                Console.WriteLine($"Date range: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
                Console.WriteLine();

                // Call the raw SOAP client
                string response = await InkassoRawSoapClient.QueryClaimsAsync(claimantId, fromDate, toDate);

                // The response is already logged in the QueryClaimsAsync method
                Console.WriteLine();
                Console.WriteLine("Test completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("=== Test Failed ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine($"Error: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace: {ex.InnerException.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Test the mock endpoint directly with a simple SOAP request
        /// </summary>
        public static async Task RunMockTest()
        {
            Console.WriteLine("=== Inkasso Mock SOAP Endpoint Test ===");
            Console.WriteLine("Testing connection to mock SOAP endpoint...");
            Console.WriteLine();

            try
            {
                // Create a simple SOAP envelope
                string soapEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"">
  <soap:Body>
    <QueryClaims xmlns=""http://IcelandicOnlineBanking/2005/12/01/Claims"">
      <query>
        <EntryFrom>1</EntryFrom>
        <EntryTo>10</EntryTo>
        <Claimant>1021021020</Claimant>
        <Period>
          <DateFrom>2024-01-01</DateFrom>
          <DateTo>2024-12-31</DateTo>
          <DateSpanReferenceDate>CreationDate</DateSpanReferenceDate>
        </Period>
      </query>
    </QueryClaims>
  </soap:Body>
</soap:Envelope>";

                Console.WriteLine("Using mock endpoint: http://localhost:5284/mock-inkasso");
                Console.WriteLine();
                Console.WriteLine("Sending SOAP request:");
                Console.WriteLine(soapEnvelope);
                Console.WriteLine();

                // Create HTTP client
                using var httpClient = new HttpClient();
                
                // Create HTTP request
                using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5284/mock-inkasso");
                request.Content = new StringContent(soapEnvelope, Encoding.UTF8);
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/soap+xml");
                
                // Send request
                HttpResponseMessage response = await httpClient.SendAsync(request);
                
                // Get response content
                string responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response
                Console.WriteLine("--- SOAP Response ---");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine(responseContent);
                
                Console.WriteLine();
                Console.WriteLine("Mock endpoint test completed successfully.");
                Console.WriteLine("Verify that the response contains the fixed mock data with ClaimIds 123456789 and 987654321.");
                
                // Demonstrate how to use the mock endpoint with InkassoRawSoapClient
                Console.WriteLine();
                Console.WriteLine("=== How to use the mock endpoint with InkassoRawSoapClient ===");
                Console.WriteLine("To use the mock endpoint with InkassoRawSoapClient, set the INKASSO_URL environment variable:");
                Console.WriteLine("Environment.SetEnvironmentVariable(\"INKASSO_URL\", \"http://localhost:5284/mock-inkasso\");");
                Console.WriteLine();
                Console.WriteLine("This can be done in your application startup code or in your test setup.");
                Console.WriteLine("The InkassoRawSoapClient will then use the mock endpoint instead of the real Inkasso service.");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("=== Mock Test Failed ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Inner Exception:");
                    Console.WriteLine($"Error: {ex.InnerException.Message}");
                    Console.WriteLine($"Stack Trace: {ex.InnerException.StackTrace}");
                }
            }
        }
    }
}
