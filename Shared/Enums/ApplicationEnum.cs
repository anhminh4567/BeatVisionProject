using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Enums
{
	public enum AcccountStatus
	{
		ACTIVE,BANNED,SUSPENDED
	}
	public enum CartItemType
	{
		TRACK,ALBUM,SERVICE
	}
	public enum TrackStatus
	{
		PUBLISH,REMOVED,PENDING
	}
	public enum NotificationType
	{
		ALL,GROUP,SINGLE
	}
	public enum CommentType
	{
		TRACK,ALBUM
	}
}
