﻿using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class ApplicationStaticValue
    {
        public static readonly string GoogleScheme = "Google";
        public static readonly string FacebookScheme = "Facebook";
        public static readonly string ApplicationTokenIssuer = "BeatVision";
        public static readonly string GoogleTokenIssuer = "Google";

        public static readonly string MailClaimType = "MyEmail";
        public static readonly string UserIdClaimType = "userid";
        public static readonly string UserRoleClaimType = "MyRole";
        public static readonly string UsernameClaimType = "MyUsername";
		public static readonly string ProfileImageUrlClaimType = "profileimageurl";

        public static readonly string BlobImageDirectory = "image";// this can be put whatever, no need for user or application specify
		public static readonly string BlobAudioDirectory = "audio";// this require extra subfolder, user, application, or whatever
        public static readonly string BlobPaidDirectory = "paid";
        public static readonly string BlobLicenseDirectory = "license";

        public static readonly string ContentTypeWav = "audio/wav";
        public static readonly string ContentTypeMp3 = "audio/mpeg";
        public static readonly string ContentTypePdf = "application/pdf";
        public static readonly string ContentTypeZip = "application/zip";



		public static readonly string DefaultProfileImageName = "defaultprofile.jpg";
		public static readonly string DefaultTrackImageName = "defaultSoundwave.jpg";

        public const string ADMIN_POLICY_NAME = "admin";
        public const string USER_POLICY_NAME = "User";
        public const string ADMIN_ROLE = "admin";
        public const string USER_ROLE = "User";

        // AUDIO FILE CONVENTION
        // PRIVATE CONTAINER:
        // - PAID       : audio/{userid}/paid/{filenameGen.mp3/wav}  
        // - PRIVATE    : audio/{userid}/{filenameGen.mp3/wav}
        // - LICENSE    : license/{filenameNOGEN.pdf}

        // PUBLIC CONTAINER:
        // - PUBLIC     : audio/{userid}/{filenameGen.mp3}

        //ALL PUBLIC uploaded asset exist in both public and private , but public only have mp3, private will hold wav. WAV do not exist outside private
        //All the generated name must be the same for all public and private
    }
    public class CacheNamingConvention
    {

    }
}
