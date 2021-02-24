using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using LeftRover.Models;

namespace LeftRover.ViewModels
{
    public class MyDonationsViewModel
    {
        [Required]
        public int DonationID { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; }

        [Required]
        [Display(Name = "Instructions")]
        public string Instructions { get; set; }

        [Required]
        [Display(Name = "Contact")]
        public string Contact { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Estimated Value")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        
        [Required]
        [Display(Name = "Delivery Available")]
        public bool DeliveryAvailable { get; set; }

        [Required]
        [Display(Name = "Pickup Available")]
        public bool PickupAvailable { get; set; }

        [Required]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Display(Name = "Street Address2")]
        public string StreetAddress2 { get; set; }

        [Required]
        [Display(Name = "City")]
        public string City { get; set; }

        [Required]
        [Display(Name = "State")]
        public string State { get; set; }

        [Required]
        [Display(Name = "Zip")]
        [DataType(DataType.PostalCode)]
        public string Zip { get; set; }

        public IEnumerable<DonationsModel> Donations { get; set; }

        [Display(Name = "Organization")]
        public string Organization { get; set; }
    }
}
