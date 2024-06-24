using System;

namespace invoice.Models
{
    public class Invoice
    {
        public string Id { get; set; }
        public decimal Amount { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } // pending, paid, void
    }
}
