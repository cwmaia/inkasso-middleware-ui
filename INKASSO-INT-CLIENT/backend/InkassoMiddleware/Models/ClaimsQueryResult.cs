using System;
using System.Collections.Generic;

namespace InkassoMiddleware.Models
{
    public class ClaimsQueryResult
    {
        public List<Claim> Claims { get; set; } = new();
        public int TotalCount { get; set; }
    }

    public class Claim
    {
        public string Id { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public decimal Capital { get; set; }
        public string Status { get; set; } = string.Empty;
    }
} 