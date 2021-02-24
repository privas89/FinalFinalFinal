using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeftRover.Models;

namespace LeftRover.ViewModels
{
    public class HomeViewModel
    {
        public string HomeAddress { get; set; }
        public List<DonationsModel> Donations { get; set; }
        public List<String> Top5Donors { get; set; }
    }
}
