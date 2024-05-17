using Shared.Helper;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Implementation
{
	public class UserService
	{
		public async Task<Result<UserProfile>> UpdateProfile()
		{
			return Result<UserProfile>.Fail();
		} 
	}
}
