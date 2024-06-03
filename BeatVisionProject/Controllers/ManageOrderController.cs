using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Services.Implementation;
using Shared.ConfigurationBinding;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageOrderController : ControllerBase
	{
		private readonly AppsettingBinding _appsettings;
		private readonly PayosService _payOsService;

		public ManageOrderController(AppsettingBinding appsettings, PayosService payOsService)
		{
			_appsettings = appsettings;
			_payOsService = payOsService;
		}
		[HttpGet("get-payment-result")]
		public async Task<ActionResult> GetPaymentUrl()
		{
			var result = await _payOsService.CreatePaymentLink();
			return Ok(result);

		}
		[HttpGet("get-payment-information")]
		public async Task<ActionResult> GetPaymentInformation([FromQuery]long orderCode)
		{
			var result = await _payOsService.GetPaymentLinkInformation(orderCode);
			return Ok(result);
		}
		[HttpPost("huy-link")]
		public async Task<ActionResult> CancelPaymentUrl([FromQuery] long orderCode , string reason) 
		{
			var cancelResult = await _payOsService.CancelPaymentUrl(orderCode, reason);
			return Ok(cancelResult);
		}
		[HttpGet("confirm-webhook")]
		public async Task<ActionResult> ConfirmWebhook()
		{
			var result = await _payOsService.AddWebhookUrl();
			return Ok(result);
		}
		[HttpGet("receive-webhook")]
		public async Task<ActionResult> ReceiveWebhook()
		{
			//var result = await _payOsService.AddWebhookUrl();
			var httpContext = HttpContext;
			return Ok();
		}
		[HttpGet("cancel-order-hook")]
		public async Task<ActionResult> CancelOrder(
		[FromQuery] string code,
			[FromQuery] string id,
			[FromQuery] bool cancel,
			[FromQuery] string status,
			[FromQuery] long orderCode)
		{
			//var result = await _payOsService.AddWebhookUrl();
			var httpContext = HttpContext;
			var json = new JsonResult(new { StatusCode  = 200, description =  "yea shit work for cancel" });
			var scheme = HttpContext.Request.Scheme;
			var host = HttpContext.Request.Host;
			var fullUrl = $"{scheme}://{host}/swagger/index.html";
			return Redirect(fullUrl);
		}
		[HttpGet("success-order")]
		public async Task<ActionResult> SuccessOrder([FromQuery] string code,
			[FromQuery] string id,
			[FromQuery] bool cancel,
			[FromQuery] string status,
			[FromQuery] long orderCode)
		{
			//var result = await _payOsService.AddWebhookUrl();
			var httpContext = HttpContext;
			var scheme = HttpContext.Request.Scheme;
			var host = HttpContext.Request.Host;
			var fullUrl = $"{scheme}://{host}/swagger/index.html";
			return Redirect(fullUrl);
		}


	}
}
