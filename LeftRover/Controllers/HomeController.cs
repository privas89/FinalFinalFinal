using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LeftRover.Models;
using LeftRover.Models.AppDBContext;
using Microsoft.AspNetCore.Identity;
using LeftRover.ViewModels;
using System.Security.Claims;

namespace LeftRover.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly UserInfoDBContext _userInfoContext;
        private readonly UserAddressDBContext _userAddressContext;
        private readonly DonationsDBContext _donationsContext;
        private readonly LeftRoverDBContext _leftRoverContext;

        public HomeController(ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager,
            UserInfoDBContext userInfoContext,
            UserAddressDBContext userAddressContext,
            DonationsDBContext donationsContext,
            LeftRoverDBContext leftRoverDBContext)
        {
            _logger = logger;
            _userManager = userManager;
            _userInfoContext = userInfoContext;
            _userAddressContext = userAddressContext;
            _donationsContext = donationsContext;
            _leftRoverContext = leftRoverDBContext;
        }

        public IActionResult Index()
        {
            HomeViewModel home_model = new HomeViewModel();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            UserAddressModel user_address = _userAddressContext.UserAddress.Select(addrs => addrs).Where(add => add.Id.Equals(userId)).FirstOrDefault();

            if (user_address != null)
            {
                home_model.HomeAddress = user_address.StreetAddress + " " +
                    user_address.StreetAddress2 + " " +
                    user_address.City + ", " +
                    user_address.State + " " +
                    user_address.Zip;
            }

            List<DonationsModel> all_donations = _donationsContext.Donations.Select(dnts => dnts).Where(dnt => dnt.Status.Equals("Available")).ToList();
            List<string> donors = new List<string>();
            Dictionary<string, int> donor_donations_dict = new Dictionary<string, int>();
            var users = _userManager.GetUsersForClaimAsync(new Claim("UserType", "Donor")).Result;

            foreach (IdentityUser user in users)
            {
                var donations_per_user = _leftRoverContext.Donations.Where(dnts => dnts.UserId.Equals(user.Id));
                if (donations_per_user != null)
                {
                    donor_donations_dict.Add(user.Id, donations_per_user.ToList().Count);
                }
            }

            var ordered = donor_donations_dict.OrderByDescending(x => x.Value);

            int start = 0;
            foreach(KeyValuePair<string, int> entry in ordered)
            {
                if (start > 5)
                {
                    break;
                }
                else
                {
                    UserInfoModel info = _userInfoContext.UserInfo.Where(ui => ui.Id.Equals(entry.Key)).FirstOrDefault();

                    donors.Add(info.OrganizationName + " with " + entry.Value + " donations.");
                } 
            }

            home_model.Donations = all_donations;
            home_model.Top5Donors = donors;

            return View(home_model);
        }

        public IActionResult ViewDonation(int id)
        {
            DonationsModel donation = _donationsContext.Donations.Select(dnts => dnts).Where(dnt => dnt.DonationID == id).FirstOrDefault();
            MyDonationsViewModel dnt_vm = new MyDonationsViewModel
            {
                DonationID = id,
                Description = donation.Description,
                Instructions = donation.Instructions,
                Contact = donation.Contact,
                PhoneNumber = donation.PhoneNumber,
                Price = donation.Price,
                DeliveryAvailable = donation.DeliveryAvailable,
                PickupAvailable = donation.PickupAvailable,
                StreetAddress = donation.StreetAddress,
                StreetAddress2 = donation.StreetAddress2,
                City = donation.City,
                State = donation.State,
                Zip = donation.Zip
            };

            return View(dnt_vm);
        }

        public IActionResult SearchDonations(string searchQ)
        {
            HomeViewModel home_model = new HomeViewModel();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            UserAddressModel user_address = _userAddressContext.UserAddress.Select(addrs => addrs).Where(add => add.Id.Equals(userId)).FirstOrDefault();

            if (user_address != null)
            {
                home_model.HomeAddress = user_address.StreetAddress + " " +
                    user_address.StreetAddress2 + " " +
                    user_address.City + ", " +
                    user_address.State + " " +
                    user_address.Zip;
            }

            List<DonationsModel> all_donations = _donationsContext.
                Donations.
                Select(dnts => dnts).
                Where(dnt => dnt.Status.Equals("Available") && (dnt.Description.Contains(searchQ))).
                ToList();
            List<string> donors = new List<string>();
            Dictionary<string, int> donor_donations_dict = new Dictionary<string, int>();
            var users = _userManager.GetUsersForClaimAsync(new Claim("UserType", "Donor")).Result;

            home_model.Donations = all_donations;

            return View(home_model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
