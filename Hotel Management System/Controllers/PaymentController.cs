using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading.Tasks;
using Hotel_Management_System.Services;
using Hotel_Management_System.Models;

namespace Hotel_Management_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly PayMongoService _payMongoService;

        public PaymentController(PayMongoService payMongoService)
        {
            _payMongoService = payMongoService;
        }

        [HttpPost("CreatePaymentIntent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest(new { message = "Invalid amount." });

            if (string.IsNullOrEmpty(request.PaymentMethod))
                return BadRequest(new { message = "Payment method is required." });

            var response = await _payMongoService.CreatePayment(request.Amount, "PHP", request.PaymentMethod);

            if (response == null)
                return BadRequest(new { message = "Failed to create payment intent." });

            var jsonResponse = JsonSerializer.Deserialize<PayMongoResponse>(response);

            if (jsonResponse?.Data == null)
                return BadRequest(new { message = "Invalid PayMongo response.", details = response });

            return Ok(new
            {
                paymentIntentId = jsonResponse.Data.Id,
                clientKey = jsonResponse.Data.Attributes.ClientKey,
                redirectUrl = jsonResponse.Data.Attributes.NextAction?.Redirect.Url ??
                              Url.Action("Receipt", "Payment", new { paymentIntentId = jsonResponse.Data.Id }, Request.Scheme)
            });
        }

        [HttpPost("AttachPaymentMethod")]
        public async Task<IActionResult> AttachPaymentMethod([FromBody] PaymentMethodAttachment request)
        {
            if (string.IsNullOrEmpty(request.PaymentIntentId) || string.IsNullOrEmpty(request.PaymentMethodId))
                return BadRequest(new { message = "PaymentIntentId and PaymentMethodId are required." });

            var response = await _payMongoService.AttachPaymentMethod(request.PaymentIntentId, request.PaymentMethodId);

            if (response == null)
                return BadRequest(new { message = "Failed to attach payment method." });

            return Ok(JsonSerializer.Deserialize<PayMongoResponse>(response));
        }

        [HttpGet("payment-confirmation")]
        public IActionResult PaymentConfirmation([FromQuery] string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
                return BadRequest("Invalid payment intent ID.");

            return Ok($"Payment confirmed for intent: {paymentIntentId}");
        }

        [HttpGet("Receipt")]
        public IActionResult Receipt(string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
                return BadRequest("Invalid payment intent ID.");

            ViewBag.PaymentIntentId = paymentIntentId;
            ViewBag.Amount = "Unknown";
            ViewBag.Status = "Pending";
            ViewBag.TransactionDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            return View("~/Views/PaymentView/Receipt.cshtml");
        }
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class PaymentMethodAttachment
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string PaymentMethodId { get; set; } = string.Empty;
    }

    public class PayMongoResponse
    {
        public PayMongoData? Data { get; set; }
    }

    public class PayMongoData
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public PayMongoAttributes Attributes { get; set; } = new PayMongoAttributes();
    }

    public class PayMongoAttributes
    {
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ClientKey { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public PayMongoNextAction? NextAction { get; set; }
    }

    public class PayMongoNextAction
    {
        public string Type { get; set; } = string.Empty;
        public PayMongoRedirect Redirect { get; set; } = new PayMongoRedirect();
    }

    public class PayMongoRedirect
    {
        public string Url { get; set; } = string.Empty;
    }
}
