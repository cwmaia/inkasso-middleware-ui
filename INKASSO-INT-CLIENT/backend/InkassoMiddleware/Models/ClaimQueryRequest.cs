using System;

namespace InkassoMiddleware.Models
{
    public class ClaimQueryRequest
    {
        public string ClaimantId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
} 