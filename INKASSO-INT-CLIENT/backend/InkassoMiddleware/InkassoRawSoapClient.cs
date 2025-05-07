using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using InkassoMiddleware.Models;

namespace InkassoMiddleware
{
    /// <summary>
    /// Raw SOAP client for Inkasso integration that bypasses WCF complexity
    /// </summary>
    public static class InkassoRawSoapClient
    {
        // To use the mock endpoint for testing, set the INKASSO_URL environment variable:
        // For local development: http://localhost:5284/mock-inkasso
        // For Docker: http://host.docker.internal:5284/mock-inkasso
        // Example: Environment.SetEnvironmentVariable("INKASSO_URL", "http://localhost:5284/mock-inkasso");
        private static readonly string SoapEndpoint = Environment.GetEnvironmentVariable("INKASSO_URL") ?? 
            "https://demo-inkasso.azurewebsites.net/SOAP/IOBS/IcelandicOnlineBankingClaimsSoap.svc";
        
        // JSON endpoint for direct mock data (bypasses SOAP parsing)
        private static readonly string JsonMockEndpoint = "http://localhost:5284/mock-inkasso-json";
        
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
            
            // Check if we should use the JSON mock endpoint
            bool useMockJson = IsUsingMockEndpoint() && SoapEndpoint.Contains("mock-inkasso");
            
            if (useMockJson)
            {
                try
                {
                    Console.WriteLine("--- Using JSON Mock Endpoint ---");
                    Console.WriteLine($"Endpoint: {JsonMockEndpoint}");
                    
                    // Create HTTP request for JSON endpoint
                    using var jsonRequest = new HttpRequestMessage(HttpMethod.Get, JsonMockEndpoint);
                    
                    // Send request
                    HttpResponseMessage jsonResponse = await httpClient.SendAsync(jsonRequest);
                    
                    // Get response content
                    string jsonResponseContent = await jsonResponse.Content.ReadAsStringAsync();
                    
                    // Log the response for debugging
                    Console.WriteLine("--- JSON Mock Response ---");
                    Console.WriteLine($"Status: {jsonResponse.StatusCode}");
                    Console.WriteLine(jsonResponseContent);
                    
                    // Convert JSON to SOAP format for compatibility
                    string soapResponse = ConvertJsonResponseToSoap(jsonResponseContent);
                    
                    // Log the converted SOAP response
                    Console.WriteLine("--- Converted SOAP Response ---");
                    Console.WriteLine(FormatXml(soapResponse));
                    
                    // Return converted SOAP response
                    return soapResponse;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"--- Error using JSON mock endpoint ---");
                    Console.WriteLine($"Exception: {ex.GetType().Name}");
                    Console.WriteLine($"Message: {ex.Message}");
                    Console.WriteLine("Falling back to SOAP endpoint...");
                }
            }
            
            // Construct SOAP envelope with security header
            string soapEnvelope = ConstructSoapEnvelope(claimantId, fromDate, toDate);
            
            // Log the request for debugging
            Console.WriteLine("--- SOAP Request ---");
            Console.WriteLine(FormatXml(soapEnvelope));
            
            // Create HTTP request
            using var request = new HttpRequestMessage(HttpMethod.Post, SoapEndpoint);
            request.Content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
            
            // Add SOAPAction to Content-Type header
            request.Content.Headers.ContentType.Parameters.Add(
                new System.Net.Http.Headers.NameValueHeaderValue("action", $"\"{SoapAction}\""));
            
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
        /// Converts a JSON response to SOAP format for compatibility
        /// </summary>
        /// <param name="jsonResponse">The JSON response from the mock endpoint</param>
        /// <returns>A SOAP formatted response</returns>
        private static string ConvertJsonResponseToSoap(string jsonResponse)
        {
            // Create a SOAP envelope with the mock data
            StringBuilder soapBuilder = new StringBuilder();
            soapBuilder.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            soapBuilder.AppendLine("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\"");
            soapBuilder.AppendLine("               xmlns:iobs=\"http://IcelandicOnlineBanking/2005/12/01/Claims\">");
            soapBuilder.AppendLine("  <soap:Body>");
            soapBuilder.AppendLine("    <iobs:QueryClaimsResponse>");
            soapBuilder.AppendLine("      <iobs:QueryClaimsResult>");
            
            try
            {
                // Parse the JSON response - use a case-insensitive option to handle property name casing
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                // Try to deserialize directly
                var claims = System.Text.Json.JsonSerializer.Deserialize<List<ClaimResult>>(jsonResponse, options);
                
                // Add each claim to the SOAP response
                if (claims != null)
                {
                    foreach (var claim in claims)
                    {
                        Console.WriteLine($"Converting claim: {claim.ClaimId}, {claim.Reference}, {claim.DueDate}, {claim.Capital}");
                        
                        soapBuilder.AppendLine("        <iobs:Claim>");
                        soapBuilder.AppendLine($"          <iobs:ClaimId>{claim.ClaimId}</iobs:ClaimId>");
                        soapBuilder.AppendLine($"          <iobs:Reference>{claim.Reference}</iobs:Reference>");
                        soapBuilder.AppendLine($"          <iobs:DueDate>{claim.DueDate:yyyy-MM-dd}</iobs:DueDate>");
                        soapBuilder.AppendLine($"          <iobs:Capital>{claim.Capital}</iobs:Capital>");
                        soapBuilder.AppendLine("        </iobs:Claim>");
                    }
                }
                else
                {
                    Console.WriteLine("No claims were deserialized from JSON");
                    
                    // Try to parse manually as a fallback
                    Console.WriteLine("Attempting manual JSON parsing...");
                    
                    // Use System.Text.Json to parse the JSON manually
                    using (var document = System.Text.Json.JsonDocument.Parse(jsonResponse))
                    {
                        var root = document.RootElement;
                        
                        // Check if it's an array
                        if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                        {
                            foreach (var element in root.EnumerateArray())
                            {
                                string claimId = "";
                                string reference = "";
                                string dueDate = DateTime.Now.ToString("yyyy-MM-dd");
                                string capital = "0";
                                
                                if (element.TryGetProperty("claimId", out var claimIdElement))
                                {
                                    claimId = claimIdElement.GetString() ?? "";
                                }
                                
                                if (element.TryGetProperty("reference", out var referenceElement))
                                {
                                    reference = referenceElement.GetString() ?? "";
                                }
                                
                                if (element.TryGetProperty("dueDate", out var dueDateElement))
                                {
                                    if (dueDateElement.ValueKind == System.Text.Json.JsonValueKind.String)
                                    {
                                        var dateString = dueDateElement.GetString();
                                        if (DateTime.TryParse(dateString, out var date))
                                        {
                                            dueDate = date.ToString("yyyy-MM-dd");
                                        }
                                    }
                                }
                                
                                if (element.TryGetProperty("capital", out var capitalElement))
                                {
                                    if (capitalElement.ValueKind == System.Text.Json.JsonValueKind.Number)
                                    {
                                        capital = capitalElement.GetDecimal().ToString();
                                    }
                                }
                                
                                Console.WriteLine($"Manually parsed claim: {claimId}, {reference}, {dueDate}, {capital}");
                                
                                soapBuilder.AppendLine("        <iobs:Claim>");
                                soapBuilder.AppendLine($"          <iobs:ClaimId>{claimId}</iobs:ClaimId>");
                                soapBuilder.AppendLine($"          <iobs:Reference>{reference}</iobs:Reference>");
                                soapBuilder.AppendLine($"          <iobs:DueDate>{dueDate}</iobs:DueDate>");
                                soapBuilder.AppendLine($"          <iobs:Capital>{capital}</iobs:Capital>");
                                soapBuilder.AppendLine("        </iobs:Claim>");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting JSON to SOAP: {ex.Message}");
                Console.WriteLine($"JSON content: {jsonResponse}");
                
                // Add a default claim if JSON parsing fails
                soapBuilder.AppendLine("        <iobs:Claim>");
                soapBuilder.AppendLine("          <iobs:ClaimId>ERROR</iobs:ClaimId>");
                soapBuilder.AppendLine("          <iobs:Reference>Error parsing JSON</iobs:Reference>");
                soapBuilder.AppendLine("          <iobs:DueDate>2024-01-01</iobs:DueDate>");
                soapBuilder.AppendLine("          <iobs:Capital>0</iobs:Capital>");
                soapBuilder.AppendLine("        </iobs:Claim>");
            }
            
            soapBuilder.AppendLine("      </iobs:QueryClaimsResult>");
            soapBuilder.AppendLine("    </iobs:QueryClaimsResponse>");
            soapBuilder.AppendLine("  </soap:Body>");
            soapBuilder.AppendLine("</soap:Envelope>");
            
            return soapBuilder.ToString();
        }

        /// <summary>
        /// Constructs a SOAP 1.2 envelope with security header and query body
        /// </summary>
        private static string ConstructSoapEnvelope(string claimantId, DateTime fromDate, DateTime toDate)
        {
            // Create XML document
            var doc = new XmlDocument();
            
            // Create XML declaration
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);
            
            // Create SOAP envelope
            XmlElement envelope = doc.CreateElement("soap12", "Envelope", "http://www.w3.org/2003/05/soap-envelope");
            doc.AppendChild(envelope);
            
            // Add namespaces
            envelope.SetAttribute("xmlns:wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            envelope.SetAttribute("xmlns:wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            envelope.SetAttribute("xmlns:iob", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            
            // Create header
            XmlElement header = doc.CreateElement("soap12", "Header", "http://www.w3.org/2003/05/soap-envelope");
            envelope.AppendChild(header);
            
            // Create security header
            XmlElement security = doc.CreateElement("wsse", "Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            security.SetAttribute("soap12:mustUnderstand", "http://www.w3.org/2003/05/soap-envelope", "1");
            header.AppendChild(security);
            
            // Create UsernameToken
            XmlElement usernameToken = doc.CreateElement("wsse", "UsernameToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            usernameToken.SetAttribute("wsu:Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", "UsernameToken-" + Guid.NewGuid().ToString());
            security.AppendChild(usernameToken);
            
            // Add username
            XmlElement username = doc.CreateElement("wsse", "Username", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            username.InnerText = Username;
            usernameToken.AppendChild(username);
            
            // Add password
            XmlElement passwordElement = doc.CreateElement("wsse", "Password", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
            passwordElement.SetAttribute("Type", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText");
            passwordElement.InnerText = Password;
            usernameToken.AppendChild(passwordElement);
            
            // Create timestamp
            XmlElement timestamp = doc.CreateElement("wsu", "Timestamp", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            timestamp.SetAttribute("wsu:Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", "Timestamp-" + Guid.NewGuid().ToString());
            security.AppendChild(timestamp);
            
            // Add created timestamp
            XmlElement created = doc.CreateElement("wsu", "Created", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            created.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            timestamp.AppendChild(created);
            
            // Add expires timestamp
            XmlElement expires = doc.CreateElement("wsu", "Expires", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
            expires.InnerText = DateTime.UtcNow.AddMinutes(10).ToString("yyyy-MM-ddTHH:mm:ssZ");
            timestamp.AppendChild(expires);
            
            // Create body
            XmlElement body = doc.CreateElement("soap12", "Body", "http://www.w3.org/2003/05/soap-envelope");
            envelope.AppendChild(body);
            
            // Create QueryClaims element
            XmlElement queryClaims = doc.CreateElement("iob", "QueryClaims", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            body.AppendChild(queryClaims);
            
            // Create query element
            XmlElement query = doc.CreateElement("iob", "query", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            queryClaims.AppendChild(query);
            
            // Add EntryFrom
            XmlElement entryFrom = doc.CreateElement("iob", "EntryFrom", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            entryFrom.InnerText = "1";
            query.AppendChild(entryFrom);
            
            // Add EntryTo
            XmlElement entryTo = doc.CreateElement("iob", "EntryTo", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            entryTo.InnerText = "10";
            query.AppendChild(entryTo);
            
            // Add Claimant
            XmlElement claimant = doc.CreateElement("iob", "Claimant", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            claimant.InnerText = claimantId;
            query.AppendChild(claimant);
            
            // Create Period element
            XmlElement period = doc.CreateElement("iob", "Period", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            query.AppendChild(period);
            
            // Add DateFrom
            XmlElement dateFrom = doc.CreateElement("iob", "DateFrom", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            dateFrom.InnerText = fromDate.ToString("yyyy-MM-dd");
            period.AppendChild(dateFrom);
            
            // Add DateTo
            XmlElement dateTo = doc.CreateElement("iob", "DateTo", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            dateTo.InnerText = toDate.ToString("yyyy-MM-dd");
            period.AppendChild(dateTo);
            
            // Add DateSpanReferenceDate
            XmlElement dateSpanReferenceDate = doc.CreateElement("iob", "DateSpanReferenceDate", "http://IcelandicOnlineBanking/2005/12/01/Claims");
            dateSpanReferenceDate.InnerText = "CreationDate";
            period.AppendChild(dateSpanReferenceDate);
            
            // Convert to string
            return doc.OuterXml;
        }

        /// <summary>
        /// Parses the SOAP response and extracts claim data
        /// </summary>
        /// <param name="soapResponse">The raw SOAP response XML</param>
        /// <returns>A list of parsed claim results</returns>
        public static List<ClaimResult> ParseClaimsFromSoapResponse(string soapResponse)
        {
            var claims = new List<ClaimResult>();
            
            try
            {
                // Load the XML document
                var doc = XDocument.Parse(soapResponse);
                
                // Define namespaces
                XNamespace soapNs = "http://www.w3.org/2003/05/soap-envelope";
                XNamespace iobsNs = "http://IcelandicOnlineBanking/2005/12/01/Claims";
                
                // Log the XML for debugging
                Console.WriteLine("Parsing XML response:");
                Console.WriteLine(FormatXml(soapResponse));
                
                try
                {
                    // First try with the standard namespace approach
                    var claimElements = doc.Descendants(iobsNs + "Claim");
                    
                    if (!claimElements.Any())
                    {
                        // If no elements found, try with a more flexible approach using local name
                        Console.WriteLine("No claims found with standard namespace. Trying with local name...");
                        claimElements = doc.Descendants()
                            .Where(e => e.Name.LocalName == "Claim");
                    }
                    
                    foreach (var claimElement in claimElements)
                    {
                        try
                        {
                            // Helper function to get element value by local name
                            string GetElementValue(XElement parent, string localName, string defaultValue = "")
                            {
                                var element = parent.Elements()
                                    .FirstOrDefault(e => e.Name.LocalName == localName);
                                return element?.Value ?? defaultValue;
                            }
                            
                            var claim = new ClaimResult
                            {
                                ClaimId = GetElementValue(claimElement, "ClaimId"),
                                Reference = GetElementValue(claimElement, "Reference"),
                                DueDate = DateTime.Parse(GetElementValue(claimElement, "DueDate", DateTime.Now.ToString("yyyy-MM-dd"))),
                                Capital = decimal.Parse(GetElementValue(claimElement, "Capital", "0"))
                            };
                            
                            claims.Add(claim);
                            Console.WriteLine($"Parsed claim: {claim.ClaimId}, {claim.Reference}, {claim.DueDate}, {claim.Capital}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing claim: {ex.Message}");
                            // Continue with the next claim
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during claim element selection: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing SOAP response: {ex.Message}");
                // Return empty list on error
            }
            
            return claims;
        }

        /// <summary>
        /// Determines if the current endpoint is the mock endpoint
        /// </summary>
        /// <returns>True if using the mock endpoint, false otherwise</returns>
        public static bool IsUsingMockEndpoint()
        {
            string? endpoint = Environment.GetEnvironmentVariable("INKASSO_URL");
            return !string.IsNullOrEmpty(endpoint) && endpoint.Contains("mock-inkasso");
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
