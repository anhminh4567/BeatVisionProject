using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Net.payOS.Types;
using Services.Implementation;
using Shared.ConfigurationBinding;
using Shared.Enums;
using Shared.RequestDto;
using Shared.ResponseDto;
using System.Runtime.CompilerServices;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace BeatVisionProject.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ManageOrderController : ControllerBase
	{
		private readonly AppsettingBinding _appsettings;
		private readonly PayosService _payOsService;
		private readonly OrderManager _orderManager;
		private readonly AppUserManager _userManager;

		public ManageOrderController(AppsettingBinding appsettings, PayosService payOsService, OrderManager orderManager, AppUserManager userManager)
		{
			_appsettings = appsettings;
			_payOsService = payOsService;
			_orderManager = orderManager;
			_userManager = userManager;
		}

		[HttpGet("get-payment-result")]
		public async Task<ActionResult> GetPaymentUrl()
		{
			var result = await _payOsService.CreatePaymentLink();
			return Ok(result);

		}
		[HttpGet("get-payment-information")]
		public async Task<ActionResult> GetPaymentInformation([FromQuery] long orderCode)
		{
			var result = await _payOsService.GetPaymentLinkInformation(orderCode);
			return Ok(result);
		}
		[HttpPost("cancel-link")]
		public async Task<ActionResult> CancelPaymentUrl([FromQuery] long orderCode, string reason)
		{
			var cancelResult = await _payOsService.CancelPaymentUrl(orderCode, reason);
			return Ok(cancelResult);
		}
		[HttpPost("add-cart-item")]
		public async Task<ActionResult> AddCartItem([FromForm] AddItemToCartModel addItemToCartModel)
		{
			var addResult = await _userManager.AddUserCartItem(addItemToCartModel.UserId, CartItemType.TRACK, addItemToCartModel.ItemId);
			if (addResult.isSuccess is false)
			{
				return StatusCode(addResult.Error.StatusCode, addResult.Error);
			}
			return Ok();
		}
		[HttpDelete("remove-cart-item")]
		public async Task<ActionResult> RemoveCartItem([FromQuery] RemoveItemFromCartModel removeItemFromCartModel)
		{
			var addResult = await _userManager.RemoveUserCartItem(removeItemFromCartModel.UserId, removeItemFromCartModel.ItemId);
			if (addResult.isSuccess is false)
			{
				return StatusCode(addResult.Error.StatusCode, addResult.Error);
			}
			return Ok();
		}
		[HttpGet("get-user-cart-items")]
		public async Task<ActionResult> GetUserCartItems([FromQuery] int userId)
		{
			if (userId <= 0)
				return BadRequest();
			var getResult = await _userManager.GetAllUserCartItems(userId);
			if (getResult.isSuccess is false)
				return StatusCode(getResult.Error.StatusCode, getResult.Error);
			return Ok(getResult.Value);
		}
		[HttpPost("checkout")]
		public async Task<ActionResult> Checkout([FromForm] int userProfileId)
		{
			if (userProfileId <= 0)
				return BadRequest();
			var checkoutResult = await _orderManager.Checkout(userProfileId);
			if (checkoutResult.isSuccess is false)
				return StatusCode(checkoutResult.Error.StatusCode, checkoutResult.Error);
			if (checkoutResult.Value is null)//if order is free
			{
				return StatusCode(StatusCodes.Status204NoContent);
			}
			return Ok(checkoutResult.Value);
		}

		[HttpPost("receive-webhook")]
		public async Task<ActionResult> ReceiveWebhook([FromBody]  WebhookType webhookType)
		{
			//var result = await _payOsService.AddWebhookUrl();
			var httpContext = HttpContext;
			var result = await _orderManager.OnWebhookPaymentReturn(webhookType);
			if (result.isSuccess is false)
				return StatusCode(result.Error.StatusCode, result.Error);
			var sendMailResult = await _orderManager.OnFinishOrder(result.Value);
			if(sendMailResult.isSuccess is false)
				return StatusCode(sendMailResult.Error.StatusCode, sendMailResult.Error);
			return NoContent();
		}
		[HttpGet("cancel-order-hook")]
		public async Task<ActionResult> CancelOrder([FromQuery] PayosReturnData payosReturnData)
		{
			var getOrder = await _orderManager.GetOrderByOrderCode(payosReturnData.orderCode);
			var cancelResult = await _orderManager.OnCancelUrl(payosReturnData, getOrder);
			var scheme = HttpContext.Request.Scheme;
			var host = HttpContext.Request.Host;
			var fullUrl = $"{scheme}://{host}/swagger/index.html";
			return Redirect(fullUrl);
		}
		[HttpGet("success-order-hook")]
		public async Task<ActionResult> SuccessOrder([FromQuery] PayosReturnData payosReturnData)
		{
			//var result = await _payOsService.AddWebhookUrl();
			var httpContext = HttpContext;
			var getOrder = await _orderManager.GetOrderByOrderCode(payosReturnData.orderCode);
			var paidResult = await _orderManager.OnReturnUrl(payosReturnData, getOrder);
			var scheme = HttpContext.Request.Scheme;
			var host = HttpContext.Request.Host;
			var fullUrl = $"{scheme}://{host}/swagger/index.html";
			return Redirect(fullUrl);
		}

	}
}
