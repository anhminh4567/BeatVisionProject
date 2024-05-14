using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helper
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class CacheKeyIdAttribute : Attribute
	{
		public CacheKeyIdAttribute()
		{
		}
	}
	// this class is used only for cachhing purpose, to reflect the key in the generic repository, so that the T property can be used to cache, but 
	// this for later use
}
