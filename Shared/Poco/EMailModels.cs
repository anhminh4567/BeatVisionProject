﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Poco
{
    public class ConfirmEmailModel
    {
        [Required]
        public string ReceiverEmail { get; set; }
        [Required]
        public string CallbackUrl { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string ConfirmationToken { get; set; }
    }
}