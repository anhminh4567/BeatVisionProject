using Microsoft.AspNetCore.Http;
using Shared.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.MyAttribute
{
	public class FileTypeAttribute : ValidationAttribute
	{
		private readonly string[] _allowedType;

		public FileTypeAttribute(string[] allowedType)
		{
			_allowedType = allowedType;
		}

		protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
		{
			if (value is IFormFile)
			{
				var formFile = (IFormFile)value;
				var fileName = formFile.FileName;
				var contentType = formFile.ContentType;
				var fileExtension = FileHelper.ExtractFileExtention(fileName);
				if (fileExtension.isSuccess is false)
				{
					return new ValidationResult("there is not extention from this file, please send appropriate file");
				}
				var isExist = _allowedType.Contains(contentType);
				if (isExist is true)
				{
					return ValidationResult.Success;
				}
				return new ValidationResult($"{contentType} type is not allowed");
			}
			else
			{
				return new ValidationResult("require type to validate is IFormFile");
			}
		}
	}
}
