using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestDto
{
	public class CreateTagDto
	{
		[Required]
		[MinLength(1)]
		[NotNull]
		[DataType(DataType.Text)]
		public string Name { get; set; }
	}
}
