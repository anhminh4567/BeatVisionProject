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
		TRACK,SERVICE
	}
	public enum TrackStatus
	{
		PUBLISH,REMOVED,WAIT_FOR_PUBLISH, NOT_FOR_PUBLISH
	}
	public enum NotificationType
	{
		ALL,GROUP,SINGLE
	}
	public enum NotificationWeight
	{
		// days to remove
		MINOR = 1, MAJOR = 2
	}
	public enum CommentType
	{
		TRACK
	}
	public enum CouponType
	{
		Track,User
	}
	public enum OrderStatus{
		PENDING = 0 , PAID = 1, CANCELLED = 2
	}
//	PAID - Đã thanh toán
//PENDING - Chờ thanh toán
//PROCESSING - Đang xử lý
//CANCELLED - Đã hủy
}
