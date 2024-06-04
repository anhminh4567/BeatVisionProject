using AutoMapper;
using Mapster;
using Net.payOS;
using Net.payOS.Errors;
using Net.payOS.Types;
using Net.payOS.Utils;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Repository.Interface;
using Shared.ConfigurationBinding;
using Shared.ResponseDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Shared.Helper;
using Microsoft.AspNetCore.Http;
using Shared.Models;

namespace Services.Implementation
{
	public class PayosService
	{
		private const string CLIENT_ID = "23947287-50b8-4f3f-adbf-c2ff09977e24";
		private const string CHECKSUM_KEY = "c9a184be283e934e896ed0977e86232426f7611d469e755e96b38e3de1e43e87"; //c9a184be283e934e896ed0977e86232426f7611d469e755e96b38e3de1e43e87
		private const string API_KEY = "e14ea841-5ca2-4c7c-8647-f03495f66275";
		private readonly AppsettingBinding _appsettings;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private const string CURRENT_WEBHOOK = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/receive-webhook";
		private const string CURRENT_RETURN_URL = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/success-order-hook";
		private const string CURRENT_CANCEL_URL = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/cancel-order-hook";
		//private const string CURRENT_WEBHOOK = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/receive-webhook";
		//private const string CURRENT_RETURN_URL = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/success-order-hook";
		//private const string CURRENT_CANCEL_URL = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/cancel-order-hook";

		public PayosService(AppsettingBinding appsettings, IUnitOfWork unitOfWork, IMapper mapper)
		{
			_appsettings = appsettings;
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<Result<CreatePaymentResultDto>> CreatePaymentLink()
		{
			var error = new Error();
			PayOS payOS = new PayOS(CLIENT_ID, API_KEY, CHECKSUM_KEY);
			ItemData item = new ItemData("Test Don hang", quantity: 1, price: 1000);
			List<ItemData> items = new List<ItemData>();
			items.Add(item);

			var myOrderCode = GenerateRandomOrderCode();
			PaymentData paymentData = new PaymentData(orderCode: myOrderCode, amount: 2000, description: "Thanh toan don hang",
				 items, cancelUrl: CURRENT_CANCEL_URL, returnUrl: CURRENT_RETURN_URL);
			try
			{
				CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
				var paymentUrl = createPayment.checkoutUrl;
				var mappedResult  = _mapper.Map<CreatePaymentResultDto>(createPayment);
				return Result<CreatePaymentResultDto>.Success(mappedResult);
			}
			catch (Exception ex) 
			{
				error.isException = true;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				error.ErrorMessage = ex.Message;
				return Result<CreatePaymentResultDto>.Fail(error);	
			}
		}
		public async Task<Result<CreatePaymentResultDto>> CreateOrderPaymentLink(Order order)
		{
			var error = new Error();
			PayOS payOS = new PayOS(CLIENT_ID, API_KEY, CHECKSUM_KEY);
			List<ItemData> items = new List<ItemData>();
			foreach(var item in order.OrderItems)
			{
				var newItem = new ItemData(name: item.TrackName, quantity: 1, price: Convert.ToInt32(item.CurrentPrice));
				items.Add(newItem);
			}
			var myOrderCode = GenerateRandomOrderCode();
			PaymentData paymentData = new PaymentData(orderCode: myOrderCode, amount: order.Price, description: "Thanh toan don hang",
				 items, cancelUrl: CURRENT_CANCEL_URL, returnUrl: CURRENT_RETURN_URL);
			try
			{
				CreatePaymentResult createPayment = await payOS.createPaymentLink(paymentData);
				var paymentUrl = createPayment.checkoutUrl;
				var mappedResult = _mapper.Map<CreatePaymentResultDto>(createPayment);
				return Result<CreatePaymentResultDto>.Success(mappedResult);
			}
			catch (Exception ex)
			{
				error.isException = true;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				error.ErrorMessage = ex.Message;
				return Result<CreatePaymentResultDto>.Fail(error);
			}
		}
		public async Task<Result<PaymentLinkInformation>> GetPaymentLinkInformation(long orderCode) 
		{
			var error = new Error();
			PayOS payOS = new PayOS(CLIENT_ID, API_KEY, CHECKSUM_KEY);
			PaymentLinkInformation paymentLinkInfomation = await payOS.getPaymentLinkInformation(orderCode);
			return Result<PaymentLinkInformation>.Success(paymentLinkInfomation);
		}
		
		public async Task<Result<PaymentLinkInformation>> CancelPaymentUrl(long orderId, string reasons) 
		{
			var error = new Error();
			try
			{
				PayOS payOS = new PayOS(CLIENT_ID, API_KEY, CHECKSUM_KEY);
				var cancelResult = await payOS.cancelPaymentLink(orderId, reasons);
				return Result<PaymentLinkInformation>.Success(cancelResult);
			}
			catch(Exception ex) 
			{
				error.isException = true;
				error.ErrorMessage = ex.Message;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result<PaymentLinkInformation>.Fail(error);
			}
			
		}
		//public async Task<string> AddWebhookUrl()
		//{

		//	PayOS payOS = new PayOS(clientId: CLIENT_ID, apiKey: API_KEY, checksumKey: CHECKSUM_KEY);
		//	var webhookUrl = "https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/receive-webhook";
		//	//var webhookUrl = "https://localhost/api/ManageOrder/receive-webhook";

		//	//var confirmResult = await payOS.confirmWebhook("https://localhost:5234");

		//	if (webhookUrl == null || webhookUrl.Length == 0)
		//	{
		//		throw new Exception("Invalid Parameter.");
		//	}
		//	string url = "https://api-merchant.payos.vn/confirm-webhook";
		//	HttpClient httpClient = new HttpClient();
		//	HttpResponseMessage responseApi = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
		//	{
		//		Content = new StringContent("{\"webhookUrl\": \"" + webhookUrl + "\"}", Encoding.UTF8, "application/json"),
		//		Headers =
		//	{
		//		{ "x-client-id", CLIENT_ID },
		//		{ "x-api-key", API_KEY }
		//	}
		//	});
		//	var body = await responseApi.Content.ReadAsStringAsync();
		//	if (responseApi.IsSuccessStatusCode)
		//	{
		//		return webhookUrl;
		//	}
		//	if (responseApi.StatusCode == HttpStatusCode.NotFound)
		//	{
		//		throw new PayOSError("20", "Webhook URL invalid.");
		//	}
		//	if (responseApi.StatusCode == HttpStatusCode.Unauthorized)
		//	{
		//		throw new PayOSError("401", "Unauthorized.");
		//	}
		//	throw new PayOSError("20", "Internal Server Error.");
		//}
		public async Task ConfirmWebhook()
		{
			
		}
		public async Task<Result<WebhookData>> VerifyPayment(WebhookType webhookType)
		{
			var error = new Error();
			PayOS payOS = new PayOS(CLIENT_ID, API_KEY, CHECKSUM_KEY);
			try
			{
				var verifyResult = payOS.verifyPaymentWebhookData(webhookType);
				if (verifyResult == null)
					return Result<WebhookData>.Fail(error);
				return Result<WebhookData>.Success(verifyResult);
			}
			catch (Exception ex) 
			{
				error.isException = true;
				error.ErrorMessage  = ex.Message;
				error.StatusCode = StatusCodes.Status500InternalServerError;
				return Result<WebhookData>.Fail(error);
			}
		}
		private static long GenerateRandomOrderCode()
		{
			var random = new Random();
			return random.NextInt64(0, 1099511627776l); // 2^40 =1099511627776 // do limit payos la 2^53
		}
	}

}
