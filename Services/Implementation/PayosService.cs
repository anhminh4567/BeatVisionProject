﻿using AutoMapper;
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
		//private const string CLIENT_ID = "64c60d5f-b375-4167-aeaf-20d319b1e68b";
		//private const string CHECKSUM_KEY = "e6d2aef8a2aa1a8a02c0ccddd5f42ea1fc2023d062700f3f6ce94c6979f80806"; 
		//private const string API_KEY = "cc081399-ff07-4618-a6a0-301a6d962540";
		private readonly AppsettingBinding _appsettings;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		//private const string CURRENT_WEBHOOK = "https://sunbeam-fluent-eagle.ngrok-free.app/api/ManageOrder/receive-webhook";
		//private const string CURRENT_RETURN_URL = "https://sunbeam-fluent-eagle.ngrok-free.app/api/ManageOrder/success-order-hook";
		//private const string CURRENT_CANCEL_URL = "https://sunbeam-fluent-eagle.ngrok-free.app/api/ManageOrder/cancel-order-hook";
		private const string CURRENT_WEBHOOK = "https://api.beatvision.store/api/ManageOrder/receive-webhook";//"https://modest-ram-mentally.ngrok-free.app/api/ManageOrder/receive-webhook";
		private const string CURRENT_RETURN_URL = "https://api.beatvision.store/api/ManageOrder/success-order-hook";//"https://api.beatvision.store/api/ManageOrder/success-order-hook";//
        private const string CURRENT_CANCEL_URL = "https://api.beatvision.store/api/ManageOrder/cancel-order-hook";//"https://api.beatvision.store/api/ManageOrder/cancel-order-hook"; //


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
				//    if ((double)paymentData.orderCode < 0.0 - (Math.Pow(2.0, 53.0) - 1.0) || (double)paymentData.orderCode > Math.Pow(2.0, 53.0) - 1.0)
				//    {
				//        throw new ArgumentOutOfRangeException("orderCode", "orderCode is out of range.");
				//    }

				//    string signature = SignatureControl.CreateSignatureOfPaymentRequest(paymentData, CHECKSUM_KEY);
				//    string url = "https://api-merchant.payos.vn/v2/payment-requests";
				//    paymentData = paymentData with
				//    {
				//        signature = signature
				//    };
				//    string jsonString = JsonConvert.SerializeObject(paymentData);
				//    HttpClient httpClient = new HttpClient();
				//    JObject responseBodyJson = JObject.Parse(await (await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
				//    {
				//        Content = new StringContent(jsonString, Encoding.UTF8, "application/json"),
				//        Headers =
				//{
				//    { "x-client-id", CLIENT_ID },
				//    { "x-api-key", API_KEY }
				//}
				//    })).Content.ReadAsStringAsync());
				//    string code = responseBodyJson["code"]?.ToString();
				//    string desc = responseBodyJson["desc"]?.ToString();
				//    string data = responseBodyJson["data"]?.ToString();
				//    if (code == null)
				//    {
				//        throw new PayOSError("20", "Internal Server Error.");
				//    }

				//    if (code == "00" && data != null)
				//    {
				//        JObject dataJson = JObject.Parse(data);
				//        string paymentLinkResSignature = SignatureControl.CreateSignatureFromObj(dataJson, CHECKSUM_KEY);
				//        if (paymentLinkResSignature != responseBodyJson["signature"].ToString())
				//        {
				//            throw new Exception("The data is unreliable because the signature of the response does not match the signature of the data");
				//        }

				//        CreatePaymentResult response = JsonConvert.DeserializeObject<CreatePaymentResult>(data);
				//        if (response == null)
				//        {
				//            throw new InvalidOperationException("Error deserializing JSON response: Deserialized object is null.");
				//        }
				//        var paymentUrl = response.checkoutUrl;
				//        var mappedResult = _mapper.Map<CreatePaymentResultDto>(response);
				//        return Result<CreatePaymentResultDto>.Success(mappedResult);
				//        //return response;
				//    }

				//    throw new PayOSError(code, desc);
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
            //string url = "https://api-merchant.payos.vn/v2/payment-requests/" + orderCode;
            //HttpClient httpClient = new HttpClient();
            //JObject responseBodyJson = JObject.Parse(await (await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, url)
            //{
            //    Headers =
            //{
            //    { "x-client-id", CLIENT_ID },
            //    { "x-api-key", CHECKSUM_KEY }
            //}
            //})).Content.ReadAsStringAsync());
            //string code = responseBodyJson["code"]?.ToString();
            //string desc = responseBodyJson["desc"]?.ToString();
            //string data = responseBodyJson["data"]?.ToString();
            //if (code == null)
            //{
            //    throw new PayOSError("20", "Internal Server Error.");
            //}

            //if (code == "00" && data != null)
            //{
            //    JObject dataJson = JObject.Parse(data);
            //    string paymentLinkResSignature = SignatureControl.CreateSignatureFromObj(dataJson, CHECKSUM_KEY);
            //    if (paymentLinkResSignature != responseBodyJson["signature"].ToString())
            //    {
            //        throw new Exception("The data is unreliable because the signature of the response does not match the signature of the data");
            //    }

            //    PaymentLinkInformation response = JsonConvert.DeserializeObject<PaymentLinkInformation>(data);
            //    if (response == null)
            //    {
            //        throw new InvalidOperationException("Error deserializing JSON response: Deserialized object is null.");
            //    }

            //    return Result<PaymentLinkInformation>.Success(response);
            //}

            //throw new PayOSError(code, desc);
            return Result<PaymentLinkInformation>.Success(paymentLinkInfomation);
		}
		
		public async Task<Result<PaymentLinkInformation>> CancelPaymentUrl(long orderId, string reasons) 
		{
			var error = new Error();
			try
			{
				PayOS payOS = new PayOS(CLIENT_ID, API_KEY, CHECKSUM_KEY);
				var cancelResult = await payOS.cancelPaymentLink(orderId, reasons);
                //string url = "https://api-merchant.payos.vn/v2/payment-requests/" + orderId + "/cancel";
                //HttpClient httpClient = new HttpClient();
                //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                //if (reasons != null)
                //{
                //    request.Content = new StringContent("{\"cancellationReason\": \"" + reasons + "\"}", Encoding.UTF8, "application/json");
                //}

                //request.Headers.Add("x-client-id", CLIENT_ID);
                //request.Headers.Add("x-api-key", API_KEY);
                //JObject responseBodyJson = JObject.Parse(await (await httpClient.SendAsync(request)).Content.ReadAsStringAsync());
                //string code = responseBodyJson["code"]?.ToString();
                //string desc = responseBodyJson["desc"]?.ToString();
                //string data = responseBodyJson["data"]?.ToString();
                //if (code == null)
                //{
                //    throw new PayOSError("20", "Internal Server Error.");
                //}

                //if (code == "00" && data != null)
                //{
                //    JObject dataJson = JObject.Parse(data);
                //    string paymentLinkResSignature = SignatureControl.CreateSignatureFromObj(dataJson, CHECKSUM_KEY);
                //    if (paymentLinkResSignature != responseBodyJson["signature"].ToString())
                //    {
                //        throw new Exception("The data is unreliable because the signature of the response does not match the signature of the data");
                //    }

                //    PaymentLinkInformation response = JsonConvert.DeserializeObject<PaymentLinkInformation>(data);
                //    if (response == null)
                //    {
                //        throw new InvalidOperationException("Error deserializing JSON response: Deserialized object is null.");
                //    }

                //    return Result<PaymentLinkInformation>.Success(response);
                //}

                //throw new PayOSError(code, desc);
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
