using Shared.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interface
{
	public interface ICacheService
	{
		Task<Result<T>> GetData<T>(string key, CancellationToken cancellationToken = default);
		Task<Result> SetData<T>(string key, T value,  CancellationToken cancellationToken = default);
		Task<Result> RemoveData(string key, CancellationToken cancellationToken = default);
		Task<Result> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
	}
}
