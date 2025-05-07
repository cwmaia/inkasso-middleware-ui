using System;

namespace InkassoMiddleware.Models
{
    /// <summary>
    /// Represents a parsed claim result from the Inkasso SOAP response
    /// </summary>
    public class ClaimResult
    {
        public string ClaimId { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal Capital { get; set; }
    }
}
