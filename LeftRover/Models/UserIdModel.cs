using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LeftRover.Models
{
    [Table("AspNetUsers")]
    public class UserIdModel
    {
        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        [NotMapped]
        public UserInfoModel UserInfo { get; set; }
        [NotMapped]
        public bool IsAdmin { get; set; }
        [NotMapped]
        public bool IsDonor { get; set; }
        [NotMapped]
        public bool IsRecipient { get; set; }
        [NotMapped]
        public bool IsTaxStatusVerified { get; set; }
    }
}
