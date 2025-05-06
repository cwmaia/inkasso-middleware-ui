using System;

namespace InkassoMiddleware.Models
{
    public class ClaimsQuery
    {
        public int EntryFrom { get; set; }
        public bool EntryFromSpecified { get; set; }
        public int EntryTo { get; set; }
        public bool EntryToSpecified { get; set; }
        public string ClaimantId { get; set; } = string.Empty;
        public ClaimsQueryDateSpan Period { get; set; } = new();
    }

    public class ClaimsQueryDateSpan
    {
        public DateTime DateFrom { get; set; }
        public bool DateFromSpecified { get; set; }
        public DateTime DateTo { get; set; }
        public bool DateToSpecified { get; set; }
        public DateSpanReferenceDate DateSpanReferenceDate { get; set; }
        public bool DateSpanReferenceDateSpecified { get; set; }
    }

    public enum DateSpanReferenceDate
    {
        CreationDate,
        DueDate,
        LastPaymentDate
    }
} 