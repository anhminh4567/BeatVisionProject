using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
namespace Services.Implementation
{
	public class AudioFileTransformationService
	{
		private readonly string _cloudinaryApiKey = "731438785958833";
		private readonly string _cloudinarySecret = "9kRV_osNVmFQ2C_12uZ3p3FjPrg";
		private readonly string _cloudinaryUrl = "cloudinary://731438785958833:9kRV_osNVmFQ2C_12uZ3p3FjPrg@dyuzzqjqv";

		public AudioFileTransformationService()
		{
		}
		public async Task upload(IFormFile file)
		{
			var cloudinary = new Cloudinary(_cloudinaryUrl);
			cloudinary.Api.Secure = true;
			var uploadResult = await cloudinary.UploadAsync(new VideoUploadParams()
			{
				File = new FileDescription(Guid.NewGuid().ToString(), file.OpenReadStream())
			}); ;

			//cloudinary.Api.UrlVideoUp.BuildUrl("F:\\ThuNghiem\\Strorage\\Demo DMS Group 2.mp4");
		}
		public void Test()
		{
			Console.WriteLine(Directory.GetCurrentDirectory());
		}
	}
}
