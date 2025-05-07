using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InkassoConsoleTest
{
    /// <summary>
    /// Raw SOAP client for Inkasso integration that bypasses WCF complexity
    /// </summary>
    public static class InkassoRawSoapClient
    {
        private const string SoapEndpoint = "https://demo-inkasso.azurewebsites.net/SOAP/IOBS/IcelandicOnlineBankingClaimsSoap.svc";
        private const string SoapAction = "http://IcelandicOnlineBanking/2005/12/01/Claims/IIcelandicOnlineBankingClaimsSoap/QueryClaims";
        private const string Username = "servicetest";
        private const string Password = "znvwYV5";

        /// <summary>
        /// Queries claims using raw SOAP 1.2 request
        /// </summary>
        /// <param name="claimantId">The claimant ID to query</param>
        /// <param name="fromDate">Start date for the query range</param>
        /// <param name="toDate">End date for the query range</param>
        /// <returns>Raw XML response from the SOAP service</returns>
        public static async Task<string> QueryClaimsAsync(string claimantId, DateTime fromDate, DateTime toDate)
        {
            // Create HTTP client
            using var httpClient = new HttpClient();
            
            // Construct SOAP envelope with security header
            string soapEnvelope = ConstructSoapEnvelope(claimantId, fromDate, toDate);
            
            // Log the request for debugging
            Console.WriteLine("--- SOAP Request ---");
            Console.WriteLine(FormatXml(soapEnvelope));
            
            // Create HTTP request
            using var request = new HttpRequestMessage(HttpMethod.Post, SoapEndpoint);
            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
            
            // Add SOAPAction to Content-Type header
            if (request.Content.Headers.ContentType != null)
            {
                request.Content.Headers.ContentType.Parameters.Add(
                    new System.Net.Http.Headers.NameValueHeaderValue("action", $"\"{SoapAction}\""));
            }
            
            try
            {
                // Send request
                HttpResponseMessage response = await httpClient.SendAsync(request);
                
                // Get response content
                string responseContent = await response.Content.ReadAsStringAsync();
                
                // Log the response for debugging
                Console.WriteLine("--- SOAP Response ---");
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine(FormatXml(responseContent));
                
                // Return raw response
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--- Error ---");
                Console.WriteLine($"Exception: {ex.GetType().Name}");
                Console.WriteLine($"Message: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Constructs a SOAP 1.2 envelope with security header and query body
        /// </summary>
        private static string ConstructSoapEnvelope(string claimantId, DateTime fromDate, DateTime toDate)
        {
            // Instead of using XmlDocument, let's use a simple string builder approach
            // This avoids issues with XML namespaces and attributes
            var sb = new StringBuilder();
            
            // Create a unique ID for the security elements
            string tokenId = "UsernameToken-" + Guid.NewGuid().ToString();
            string timestampId = "Timestamp-" + Guid.NewGuid().ToString();
            
            // Format timestamps
            string createdTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string expiresTime = DateTime.UtcNow.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:ssZ");
            
            // Build the SOAP envelope as a string
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<soap12:Envelope xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\"");
            sb.AppendLine("                xmlns:wsse=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd\"");
            sb.AppendLine("                xmlns:wsu=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd\"");
            sb.AppendLine("                xmlns:iob=\"http://IcelandicOnlineBanking/2005/12/01/Claims\">");
            
            // Header with security
            sb.AppendLine("  <soap12:Header>");
            sb.AppendLine("    <wsse:Security soap12:mustUnderstand=\"1\">");
            
            // Username token
            sb.AppendLine($"      <wsse:UsernameToken wsu:Id=\"{tokenId}\">");
            sb.AppendLine($"        <wsse:Username>{Username}</wsse:Username>");
            sb.AppendLine($"        <wsse:Password Type=\"http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText\">{Password}</wsse:Password>");
            sb.AppendLine("      </wsse:UsernameToken>");
            
            // Timestamp
            sb.AppendLine($"      <wsu:Timestamp wsu:Id=\"{timestampId}\">");
            sb.AppendLine($"        <wsu:Created>{createdTime}</wsu:Created>");
            sb.AppendLine($"        <wsu:Expires>{expiresTime}</wsu:Expires>");
            sb.AppendLine("      </wsu:Timestamp>");
            sb.AppendLine("    </wsse:Security>");
            sb.AppendLine("  </soap12:Header>");
            
            // Body with query
            sb.AppendLine("  <soap12:Body>");
            sb.AppendLine("    <iob:QueryClaims>");
            sb.AppendLine("      <iob:query>");
            sb.AppendLine("        <iob:EntryFrom>1</iob:EntryFrom>");
            sb.AppendLine("        <iob:EntryTo>10</iob:EntryTo>");
            sb.AppendLine($"        <iob:Claimant>{claimantId}</iob:Claimant>");
            sb.AppendLine("        <iob:Period>");
            sb.AppendLine($"          <iob:DateFrom>{fromDate:yyyy-MM-dd}</iob:DateFrom>");
            sb.AppendLine($"          <iob:DateTo>{toDate:yyyy-MM-dd}</iob:DateTo>");
            sb.AppendLine("          <iob:DateSpanReferenceDate>CreationDate</iob:DateSpanReferenceDate>");
            sb.AppendLine("        </iob:Period>");
            sb.AppendLine("      </iob:query>");
            sb.AppendLine("    </iob:QueryClaims>");
            sb.AppendLine("  </soap12:Body>");
            sb.AppendLine("</soap12:Envelope>");
            
            return sb.ToString();
        }

        /// <summary>
        /// Formats XML for better readability in console output
        /// </summary>
        private static string FormatXml(string xml)
        {
            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                
                var sb = new StringBuilder();
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace
                };
                
                using (var writer = XmlWriter.Create(sb, settings))
                {
                    doc.Save(writer);
                }
                
                return sb.ToString();
            }
            catch
            {
                // If XML formatting fails, return the original string
                return xml;
            }
        }
    }
}
