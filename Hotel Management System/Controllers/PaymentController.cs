using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Hotel_Management_System.Services; // ✅ Ensure this is added
using Hotel_Management_System.Models;  // ✅ If you have a PaymentRequest model

namespace Hotel_Management_System.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PayMongoService _payMongoService;

        public PaymentController(PayMongoService payMongoService)
        {
            _payMongoService = payMongoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        {
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Invalid amount." });
            }

            if (string.IsNullOrEmpty(request.PaymentMethod))
            {
                return BadRequest(new { message = "Payment method is required." });
            }

            string? paymentIntentId = await _payMongoService.CreatePayment(request.Amount, "PHP", request.PaymentMethod);

            if (paymentIntentId == null)
            {
                return BadRequest(new { message = "Failed to create payment intent." });
            }

            return Ok(new { paymentIntentId });
        }

    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; } 
    }


}
