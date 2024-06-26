﻿using Repository.Interface;
using Repository.Interface.User;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Implementation
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
		public RepositoryWrapper(ICustomIdentityUserRepository customIdentityUser, ICustomIdentityUserLoginsRepository customIdentityUserLogins, ICustomIdentityUserClaimsRepository customIdentityUserClaims, ICustomIdentityUserTokenRepository customIdentityUserToken, ICustomIdentityRoleRepository customIdentityRole, ICustomIdentityRoleClaimRepository customIdentityRoleClaim, ICustomIdentityUserRoleRepository customIdentityUserRole, IRepositoryBase<UserProfile> userProfileRepository, IRepositoryBase<Notification> notificationRepository, IRepositoryBase<Message> messageRepository, IRepositoryBase<CartItem> cartItemRepository, IRepositoryBase<Comment> commentRepository, IRepositoryBase<TrackComment> trackCommentRepository, IRepositoryBase<Coupon> couponRepository, IRepositoryBase<Track> trackRepository, IRepositoryBase<TrackLicense> trackLicenseRepository, IRepositoryBase<Tag> tagRepository, IRepositoryBase<BlobFileData> blobFileDataRepository, IRepositoryBase<Order> orderRepository, IRepositoryBase<OrderItem> orderItemRepository, IRepositoryBase<OrderTransaction> orderTransactionRepository)
		{
			this.customIdentityUser = customIdentityUser;
			this.customIdentityUserLogins = customIdentityUserLogins;
			this.customIdentityUserClaims = customIdentityUserClaims;
			this.customIdentityUserToken = customIdentityUserToken;
			this.customIdentityRole = customIdentityRole;
			this.customIdentityRoleClaim = customIdentityRoleClaim;
			this.customIdentityUserRole = customIdentityUserRole;
			this.userProfileRepository = userProfileRepository;
			this.notificationRepository = notificationRepository;
			this.messageRepository = messageRepository;
			this.cartItemRepository = cartItemRepository;
			this.commentRepository = commentRepository;
			this.trackCommentRepository = trackCommentRepository;
			this.couponRepository = couponRepository;
			this.trackRepository = trackRepository;
			this.trackLicenseRepository = trackLicenseRepository;
			this.tagRepository = tagRepository;
			this.blobFileDataRepository = blobFileDataRepository;
			this.orderRepository = orderRepository;
			this.orderItemRepository = orderItemRepository;
			this.orderTransactionRepository = orderTransactionRepository;
		}

		public ICustomIdentityUserRepository customIdentityUser { get; set; }

        public ICustomIdentityUserLoginsRepository customIdentityUserLogins { get; set; }

        public ICustomIdentityUserClaimsRepository customIdentityUserClaims { get; set; }

        public ICustomIdentityUserTokenRepository customIdentityUserToken { get; set; }

        public ICustomIdentityRoleRepository customIdentityRole { get; set; }

        public ICustomIdentityRoleClaimRepository customIdentityRoleClaim { get; set; }

        public ICustomIdentityUserRoleRepository customIdentityUserRole { get; set; }

		//public IRepositoryBase<UserProfile> userProfileRepository { get; set; }

		public IRepositoryBase<UserProfile> userProfileRepository { get; set; }
		public IRepositoryBase<Notification> notificationRepository { get; set; }
		public IRepositoryBase<Message> messageRepository { get; set; }
		public IRepositoryBase<CartItem> cartItemRepository { get; set; }
		public IRepositoryBase<Comment> commentRepository { get; set; }
		public IRepositoryBase<TrackComment> trackCommentRepository { get; set; }
		public IRepositoryBase<Coupon>  couponRepository { get; set; }
		public IRepositoryBase<Track> trackRepository { get; set; }
		public IRepositoryBase<TrackLicense> trackLicenseRepository { get; set; }

		public IRepositoryBase<Tag> tagRepository { get; set; }
		public IRepositoryBase<BlobFileData> blobFileDataRepository { get; set; }

		//ORDER
		public IRepositoryBase<Order> orderRepository { get; set; }
		public IRepositoryBase<OrderItem> orderItemRepository { get; set; }
		public IRepositoryBase<OrderTransaction> orderTransactionRepository { get; set; }
	}
}
