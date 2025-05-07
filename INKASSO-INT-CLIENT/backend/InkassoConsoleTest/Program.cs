using System;
using System.Threading.Tasks;

namespace InkassoConsoleTest
{
    class Program
    {
        static async Task Main(string[] args)
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
                Console.WriteLine("Sending request...");
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

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
