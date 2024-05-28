using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ResponseDto
{
	public class PagingResponseDto<T> where T : class
	{
		public int TotalCount { get; set; }
		public T Value { get; set; }
	}
}
