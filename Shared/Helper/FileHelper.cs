using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shared.Helper
{
	public static class FileHelper
	{
		public static Result<string> ExtractFileExtention(string filename, bool includeDot = false)
		{
			var error = new Error();
			if(string.IsNullOrEmpty(filename))
			{
				error.ErrorMessage = "filename is empty or null";
				return Result.Fail(error);
			}
			string[] arraySplitByDot = filename.Split('.');
			if(arraySplitByDot.Length == 1)// means there is no extention found 
			{
				error.ErrorMessage = "no extentions found for this file";
				return Result.Fail(error);
			}
			var extension = arraySplitByDot.Last();
			if (includeDot)
				return Result<string>.Success($".{extension}");
			return Result<string>.Success(extension);
		}
		public static Result<string> ExtractFileNameFromExtension(string filename)
		{
			var error = new Error();
			if (string.IsNullOrEmpty(filename))
			{
				error.ErrorMessage = "filename is empty or null";
				return Result.Fail(error);
			}
			string[] arraySplitByDot = filename.Split('.');
			if (arraySplitByDot.Length == 1)// means there is no extention found 
			{
				error.ErrorMessage = "no extentions found for this file";
				return Result.Fail(error);
			}
			var stringBuilder = new StringBuilder();
			for(int i = 0; i < arraySplitByDot.Length -1 ; i ++) //ignore the final extension 
			{
				stringBuilder.Append(arraySplitByDot[i]);
			}
			return Result.Success(stringBuilder.ToString());
		}
		public static Result IsExtensionAllowed(string extension,string[] allowedExtentsion) 
		{
            foreach (var ext in allowedExtentsion)
            {
                if(extension.Equals(ext,StringComparison.CurrentCulture))// will check even if the extension is case sensitive
				{
					return Result.Success();
				}
            }
            return Result.Fail();
		}
		public static decimal GetFileSizeMegabyte(long byteLength, int roundTo) 
		{
			decimal fileSizeInMb = byteLength / (1024 * 1024);
			decimal roundedFileSize = Math.Round(fileSizeInMb, roundTo);
			return roundedFileSize;
		}

		public static bool CheckFileNameAgainstRegrex(string fileName, string regrex) 
		{
			if(string.IsNullOrEmpty(regrex) || string.IsNullOrEmpty(fileName))
			{
				return false;
			}
			var regexInstance = new Regex(regrex);
			return regexInstance.IsMatch(fileName);
		}

		public static bool CheckFileNameAgainstRegrex(Result<string> getFileName, string allowedFileNameRegrex)
		{
			throw new NotImplementedException();
		}
	}
}
