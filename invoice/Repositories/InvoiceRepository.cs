using invoice.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace invoice.InvoiceRepository
{
    public class Repositories
    {
        private readonly List<Invoice> _invoices = new List<Invoice>();
        private int _lastInvoiceId = 1234;

        public IEnumerable<Invoice> GetInvoices() => _invoices;

        public Invoice GetInvoice(string id) => _invoices.FirstOrDefault(i => i.Id == id);

        public void AddInvoice(Invoice invoice)
        {
            _lastInvoiceId++;
            invoice.Id = _lastInvoiceId.ToString();
            _invoices.Add(invoice);
        }
        public void UpdateInvoice(Invoice invoice)
        {
            var index = _invoices.FindIndex(i => i.Id == invoice.Id);
            if (index != -1) _invoices[index] = invoice;
        }
    }
}
