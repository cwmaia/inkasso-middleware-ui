using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace InkassoMiddleware
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
