namespace InkassoMiddleware.Models
{
    public class ClaimQueryRequest
    {
        public string ClaimantId { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
} 