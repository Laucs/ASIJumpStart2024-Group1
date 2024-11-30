﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Services.ServiceModels
{
    public class ChangeUsernameViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string NewUsername { get; set; }

    }
}
