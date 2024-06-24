
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using invoice.Models;
using invoice.InvoiceRepository;

namespace invoice.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly Repositories _repository;

        public InvoicesController(Repositories repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public IActionResult CreateInvoice([FromBody] Invoice invoice)
        {
            try
            {
                // invoice.Id = Guid.NewGuid().ToString();
                invoice.PaidAmount = 0;
                invoice.Status = "pending";
                _repository.AddInvoice(invoice);
                return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpGet]
        public IActionResult GetInvoices()
        {
            try
            {
                return Ok(_repository.GetInvoices());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetInvoice(string id)
        {
            try
            {
                var invoice = _repository.GetInvoice(id);
                if (invoice == null)
                    return NotFound();
                return Ok(invoice);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/payments")]
        public IActionResult PayInvoice(string id, [FromBody] Payment payment)
        {
            try
            {
            var invoice = _repository.GetInvoice(id);
            if (invoice == null)
                return NotFound();

            invoice.PaidAmount += payment.Amount;

            if (invoice.PaidAmount >= invoice.Amount)
            {
                invoice.Status = "paid";
                invoice.PaidAmount = invoice.Amount; // Ensure it doesn't go over the total amount
            }

            _repository.UpdateInvoice(invoice);
            return Ok(invoice);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("process-overdue")]

        public IActionResult ProcessOverdue([FromBody] OverdueProcessing overdueProcessing)
        {
            try
            {
                var overdueInvoices = _repository.GetInvoices().Where(i => i.Status == "pending" && i.DueDate.AddDays(overdueProcessing.OverdueDays) < DateTime.UtcNow).ToList();

                foreach (var invoice in overdueInvoices)
                {
                    if (invoice.PaidAmount > 0)
                    {
                        var newInvoice = new Invoice
                        {
                            Amount = (invoice.Amount - invoice.PaidAmount) + overdueProcessing.LateFee,
                            PaidAmount = 0,
                            DueDate = DateTime.UtcNow.AddDays(30),
                            Status = "pending"
                        };
                        _repository.AddInvoice(newInvoice);
                        invoice.Status = "paid";
                    }
                    else
                    {
                        var newInvoice = new Invoice
                        {
                            Amount = invoice.Amount + overdueProcessing.LateFee,
                            PaidAmount = 0,
                            DueDate = DateTime.UtcNow.AddDays(30),
                            Status = "pending"
                        };
                        _repository.AddInvoice(newInvoice);
                        invoice.Status = "void";
                    }
                    _repository.UpdateInvoice(invoice);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
         
        }
    }

    public class Payment
    {
        public decimal Amount { get; set; }
    }

    public class OverdueProcessing
    {
        public decimal LateFee { get; set; }
        public int OverdueDays { get; set; }
    }
}
