using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper
{
	public class ImageFile
	{
		public static string ConvertImageToBase64String(string imagePath)
		{
			byte[] imageBytes = File.ReadAllBytes(imagePath);
			return Convert.ToBase64String(imageBytes);
		}
	}
}
