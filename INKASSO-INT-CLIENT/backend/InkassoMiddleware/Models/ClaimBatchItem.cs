namespace InkassoMiddleware.Models;

public class ClaimBatchItem
{
    public string ClaimantId { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Capital { get; set; }
    public string ClientReference { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public string PayorId { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
} 