using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LeftRover.Models
{
    public class DonationClaimsModel
    {
        [Key]
        [Required]
        public int DonationID { get; set; }

        [Key]
        [Required]
        public string UserId { get; set; }
    }
}
