using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LeftRover.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LeftRover.Models;
using LeftRover.Models.AppDBContext;

namespace LeftRover.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserInfoDBContext _userInfoContext;
        private readonly UserAddressDBContext _userAddressContext;
        private readonly DonationsDBContext _donationsContext;
        private readonly LeftRoverDBContext _leftRoverContext;

        public AccountController(UserManager<IdentityUser> userManager,
                                 SignInManager<IdentityUser> signInManager,
                                 UserInfoDBContext userInfoContext,
                                 UserAddressDBContext userAddressContext,
                                 DonationsDBContext donationsContext,
                                 LeftRoverDBContext leftRoverContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userInfoContext = userInfoContext;
            _userAddressContext = userAddressContext;
            _donationsContext = donationsContext;
            _leftRoverContext = leftRoverContext;
        }

        [Authorize]
        public IActionResult MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            UserAddressModel address = _userAddressContext.UserAddress.Select(addrs => addrs).Where(add => add.Id.Equals(userId)).FirstOrDefault();
            UserInfoModel info = _userInfoContext.UserInfo.Select(infos => infos).Where(info => info.Id.Equals(userId)).FirstOrDefault();

            UpdateInfoViewModel model = new UpdateInfoViewModel
            {
                OrganizationName = info.OrganizationName,
                FirstName = info.FirstName,
                LastName = info.LastName,
                PhoneNumber = info.PhoneNumber,
                StreetAddress = address.StreetAddress,
                StreetAddress2 = address.StreetAddress2,
                City = address.City,
                State = address.State,
                Zip = address.Zip,
                TaxID = info.TaxID
            };

            // no error initially
            model.UpdateStatus = -1;
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [Authorize(Policy = "Admin")]
        public IActionResult Admin()
        {
            var users = _leftRoverContext.Users.Select(usr => usr).ToList();

            foreach (UserIdModel user in users)
            {
                user.UserInfo = _userInfoContext.UserInfo.Where(usr => usr.Id.Equals(user.Id)).FirstOrDefault();
                var curr_user = _userManager.FindByIdAsync(user.Id);
                var claims = _signInManager.UserManager.GetClaimsAsync(curr_user.Result);

                foreach (Claim claim in claims.Result)
                {
                    if (claim.Value.Equals("Admin"))
                    {
                        user.IsAdmin = true;
                    }

                    if (claim.Value.Equals("Recipient"))
                    {
                        user.IsRecipient = true;
                    }

                    if (claim.Value.Equals("Donor"))
                    {
                        user.IsDonor = true;
                    }
                }
            }

            return View(users);
        }

        [Authorize(Policy = "Admin")]
        public IActionResult UserInfo(string user_id)
        {
            UserIdModel user = _leftRoverContext.Users.Where(usr => usr.Id.Equals(user_id)).FirstOrDefault();
            user.UserInfo = _userInfoContext.UserInfo.Where(usr => usr.Id.Equals(user_id)).FirstOrDefault();

            var curr_user = _userManager.FindByIdAsync(user_id);
            var claims = _signInManager.UserManager.GetClaimsAsync(curr_user.Result);

            foreach (Claim claim in claims.Result)
            {
                if (claim.Value.Equals("Admin"))
                {
                    user.IsAdmin = true;
                }

                if (claim.Value.Equals("Recipient"))
                {
                    user.IsRecipient = true;
                }

                if (claim.Value.Equals("Donor"))
                {
                    user.IsDonor = true;
                }
            }

            return View(user);
        }

        [HttpPost]
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> EditUser(UserIdModel user_given)
        {
            Claim admin = new Claim("UserType", "Admin");
            UserIdModel user = _leftRoverContext.Users.Where(usr => usr.Id.Equals(user_given.Id)).FirstOrDefault();
            user.UserInfo = _userInfoContext.UserInfo.Where(usr => usr.Id.Equals(user_given.Id)).FirstOrDefault();

            var curr_user = await _userManager.FindByIdAsync(user_given.Id);
            var Identity = new ClaimsIdentity(curr_user.Id);


            var claims = await _signInManager.UserManager.GetClaimsAsync(curr_user);
            bool was_admin = false;
            bool was_donor = false;
            bool was_recipient = false;
            bool was_tax_id_verified = false;
            foreach (Claim claim in claims)
            {
                if (claim.Type.Equals("UserType") && claim.Value.Equals("Admin"))
                {
                    was_admin = true;
                }
                if (claim.Type.Equals("UserType") && claim.Value.Equals("Donor"))
                {
                    was_donor = true;
                }
                if (claim.Type.Equals("UserType") && claim.Value.Equals("Recipient"))
                {
                    was_recipient = true;
                }
                if (claim.Type.Equals("UserType") && claim.Value.Equals("TaxIdVerified"))
                {
                    was_tax_id_verified = true;
                }
            }

            if (user_given.IsAdmin && !was_admin)
            {    
                await _signInManager.UserManager.AddClaimAsync(curr_user, new Claim("UserType", "Admin"));
            }
            else if (!user_given.IsAdmin && was_admin)
            {
                await _signInManager.UserManager.RemoveClaimAsync(curr_user, new Claim("UserType", "Admin"));
            }

            if (user_given.IsDonor && !was_donor)
            {
                await _signInManager.UserManager.AddClaimAsync(curr_user, new Claim("UserType", "Donor"));
            }
            else if (!user_given.IsDonor && was_donor)
            {
                await _signInManager.UserManager.RemoveClaimAsync(curr_user, new Claim("UserType", "Donor"));
            }

            if (user_given.IsRecipient && !was_recipient)
            {
                await _signInManager.UserManager.AddClaimAsync(curr_user, new Claim("UserType", "Recipient"));
            }
            else if (!user_given.IsRecipient && was_recipient)
            {
                await _signInManager.UserManager.RemoveClaimAsync(curr_user, new Claim("UserType", "Recipient"));
            }

            if (user_given.IsTaxStatusVerified && !was_tax_id_verified)
            {
                await _signInManager.UserManager.AddClaimAsync(curr_user, new Claim("UserType", "TaxIdVerified"));
            }
            else if (!user_given.IsTaxStatusVerified && was_tax_id_verified)
            {
                await _signInManager.UserManager.RemoveClaimAsync(curr_user, new Claim("UserType", "TaxIdVerified"));
            }

            claims = await _signInManager.UserManager.GetClaimsAsync(curr_user);

            foreach (Claim claim in claims)
            {
                if (claim.Value.Equals("Admin"))
                {
                    user.IsAdmin = true;
                }

                if (claim.Value.Equals("Recipient"))
                {
                    user.IsRecipient = true;
                }

                if (claim.Value.Equals("Donor"))
                {
                    user.IsDonor = true;
                }
            }

            return View("UserInfo", user);
        }

        [Authorize(Policy = "Recipient")]
        public IActionResult RemoveClaimedDonation(int id)
        {
            MyClaimsViewModel my_claims = new MyClaimsViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DonationClaimsModel claim_to_remove = _leftRoverContext.DonationClaims.Where(dnt => dnt.DonationID == id && dnt.UserId.Equals(userId)).FirstOrDefault();

            try
            {
                _leftRoverContext.DonationClaims.Attach(claim_to_remove);
                _leftRoverContext.DonationClaims.Remove(claim_to_remove);
                _leftRoverContext.SaveChanges();
            }
            catch
            {

            }

            DonationsModel donation_selected = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == id).FirstOrDefault();

            donation_selected.Status = "Available";

            try
            {
                _leftRoverContext.SaveChanges();
            }
            catch
            {

            }

            List<DonationClaimsModel> claimed_donations = _leftRoverContext.DonationClaims.Where(dnts => dnts.UserId.Equals(userId)).ToList();
            List<DonationsModel> donations = new List<DonationsModel>();

            foreach (DonationClaimsModel claimed in claimed_donations)
            {
                DonationsModel donation = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == claimed.DonationID).FirstOrDefault();
                donations.Add(donation);
            }

            my_claims.ClaimedDonations = donations;

            return View("MyItems", my_claims);
        }

        [Authorize(Policy = "Recipient")]
        public IActionResult MarkDonationRecieved(int id)
        {
            MyClaimsViewModel my_claims = new MyClaimsViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DonationsModel donation_selected = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == id).FirstOrDefault();

            donation_selected.Status = "Received";

            try
            {
                _leftRoverContext.SaveChanges();
            }
            catch
            {

            }

            List<DonationClaimsModel> claimed_donations = _leftRoverContext.DonationClaims.Where(dnts => dnts.UserId.Equals(userId)).ToList();
            List<DonationsModel> donations = new List<DonationsModel>();

            foreach (DonationClaimsModel claimed in claimed_donations)
            {
                DonationsModel donation = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == claimed.DonationID).FirstOrDefault();
                donations.Add(donation);
            }

            my_claims.ClaimedDonations = donations;

            return View("MyItems", my_claims);
        }

        [Authorize(Policy = "Recipient")]
        public IActionResult MyItems(MyClaimsViewModel model)
        {
            MyClaimsViewModel my_claims = new MyClaimsViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<DonationClaimsModel> claimed_donations = _leftRoverContext.DonationClaims.Where(dnts => dnts.UserId.Equals(userId)).ToList();
            List<DonationsModel> donations = new List<DonationsModel>();

            foreach (DonationClaimsModel claimed in claimed_donations)
            {
                DonationsModel donation = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == claimed.DonationID).FirstOrDefault();
                donations.Add(donation);
            }

            my_claims.ClaimedDonations = donations;

            return View(my_claims);
        }

        [Authorize(Policy = "Recipient")]
        [HttpPost]
        public IActionResult ClaimDonation(MyDonationsViewModel model)
        {
            MyClaimsViewModel my_claims = new MyClaimsViewModel();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DonationClaimsModel claim = new DonationClaimsModel
            {
                DonationID = model.DonationID,
                UserId = userId
            };

            try
            {
                _leftRoverContext.DonationClaims.Add(claim);
                _leftRoverContext.SaveChanges();

                DonationsModel donation = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == model.DonationID).FirstOrDefault();
                donation.Status = "Claimed";
                _leftRoverContext.SaveChanges();
            }
            catch
            {

            }

            List<DonationClaimsModel> claimed_donations = _leftRoverContext.DonationClaims.Where(dnts => dnts.UserId.Equals(userId)).ToList();
            List<DonationsModel> donations = new List<DonationsModel>();

            foreach (DonationClaimsModel claimed in claimed_donations)
            {
                DonationsModel donation = _leftRoverContext.Donations.Where(dnt => dnt.DonationID == claimed.DonationID).FirstOrDefault();
                donations.Add(donation);
            }

            my_claims.ClaimedDonations = donations;

            return View("MyItems", my_claims);
        }

        [Authorize(Policy = "Donor")]
        public IActionResult UpdateDonation(MyDonationsViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DonationsModel donation_to_update = _donationsContext.Donations.Select(dnts => dnts).Where(dnt => dnt.DonationID == model.DonationID && dnt.UserId.Equals(userId)).FirstOrDefault();

            donation_to_update.Description = model.Description;
            donation_to_update.Instructions = model.Instructions;
            donation_to_update.Contact = model.Contact;
            donation_to_update.PhoneNumber = model.PhoneNumber;
            donation_to_update.Price = model.Price;
            donation_to_update.DeliveryAvailable = (bool)model.DeliveryAvailable;
            donation_to_update.PickupAvailable = (bool)model.PickupAvailable;
            donation_to_update.StreetAddress = model.StreetAddress;
            donation_to_update.StreetAddress2 = model.StreetAddress2;
            donation_to_update.City = model.City;
            donation_to_update.State = model.State;
            donation_to_update.Zip = model.Zip;

            try
            {
                _donationsContext.SaveChanges();
            }
            catch
            {

            }

            MyDonationsViewModel donation_vw_model = new MyDonationsViewModel();
            var donations = _donationsContext.Donations.Select(dnts => dnts).Where(dnts => dnts.UserId.Equals(userId));

            donation_vw_model.Donations = donations;

            return RedirectToAction("MyDonations", "Account", donation_vw_model);
        }

        [Authorize(Policy = "Donor")]
        public IActionResult SelectDonation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            DonationsModel donation_to_update = _donationsContext.Donations.Where(dnt => dnt.DonationID == id && dnt.UserId.Equals(userId)).FirstOrDefault();

            MyDonationsViewModel donation_vw_model = new MyDonationsViewModel
            {
                DonationID = id,
                UserId = userId,
                Description = donation_to_update.Description,
                Instructions = donation_to_update.Instructions,
                Contact = donation_to_update.Contact,
                PhoneNumber = donation_to_update.PhoneNumber,
                Price = donation_to_update.Price,
                DeliveryAvailable = donation_to_update.DeliveryAvailable,
                StreetAddress = donation_to_update.StreetAddress,
                StreetAddress2 = donation_to_update.StreetAddress2,
                City = donation_to_update.City,
                State = donation_to_update.State,
                Zip = donation_to_update.Zip
            };

            return View(donation_vw_model);
        }

        [Authorize(Policy = "Donor")]
        public IActionResult MyDonations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            MyDonationsViewModel donation_vw_model = new MyDonationsViewModel();
            var donations = _donationsContext.Donations.Select(dnts => dnts).Where(dnts => dnts.UserId.Equals(userId));

            donation_vw_model.Donations = donations;

            return View(donation_vw_model);
        }

        [Authorize(Policy = "Donor")]
        public IActionResult DeleteDonation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var deleted_donations = _donationsContext.Donations.Where(dnt => dnt.DonationID == id && dnt.UserId.Equals(userId));

            foreach (DonationsModel donation in deleted_donations)
            {
                _donationsContext.Attach(donation);
                _donationsContext.Remove(donation);
            }

            try
            {
                _donationsContext.SaveChanges();
            }
            catch
            {

            }

            MyDonationsViewModel donation_vw_model = new MyDonationsViewModel();
            var donations = _donationsContext.Donations.Select(dnts => dnts).Where(dnts => dnts.UserId.Equals(userId));

            donation_vw_model.Donations = donations;

            return View("MyDonations", donation_vw_model);
        }

        [Authorize(Policy = "Donor")]
        [HttpPost]
        public IActionResult CreateDonation(MyDonationsViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.FindByIdAsync(userId);

            DonationsModel new_donation = new DonationsModel
            {
                UserId = user.Result.Id,
                Description = model.Description,
                Instructions = model.Instructions,
                Status = "Available",
                Contact = model.Contact,
                PhoneNumber = model.PhoneNumber,
                Price = model.Price,
                DeliveryAvailable = (bool)model.DeliveryAvailable,
                PickupAvailable = (bool)model.PickupAvailable,
                StreetAddress = model.StreetAddress,
                StreetAddress2 = model.StreetAddress2,
                City = model.City,
                State = model.State,
                Zip = model.Zip
            };

            _donationsContext.Donations.Add(new_donation);

            try
            {
                _donationsContext.SaveChanges();
            }
            catch
            {

            }

            MyDonationsViewModel donation_vw_model = new MyDonationsViewModel();
            var donations = _donationsContext.Donations.Select(dnts => dnts).Where(dnts => dnts.UserId.Equals(userId));

            donation_vw_model.Donations = donations;

            return View("MyDonations", donation_vw_model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdateInfo(UpdateInfoViewModel model)
        {
            // no error initially
            model.UpdateStatus = -1;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.FindByIdAsync(userId);

            // try updating the database
            try
            {
                UserAddressModel address = _userAddressContext.UserAddress.Select(addrs => addrs).Where(add => add.Id.Equals(userId)).FirstOrDefault();
                UserInfoModel info = _userInfoContext.UserInfo.Select(infos => infos).Where(info => info.Id.Equals(userId)).FirstOrDefault();

                address.StreetAddress = model.StreetAddress;
                address.StreetAddress2 = model.StreetAddress2;
                address.City = model.City;
                address.State = model.State;
                address.Zip = model.Zip;

                info.OrganizationName = model.OrganizationName;
                info.FirstName = model.FirstName;
                info.LastName = model.LastName;
                info.PhoneNumber = model.PhoneNumber;
                info.TaxID = model.TaxID;

                _userAddressContext.SaveChanges();
                _userInfoContext.SaveChanges();

                model.UpdateStatus = 0;
            }
            catch (Exception e)
            {
                model.UpdateStatus = 1;
            }

            return View("MyProfile", model);
        }

        [Authorize]
        [HttpPost]
        public IActionResult UpdatePassword(UpdateInfoViewModel model)
        {
            model.UpdateStatus = -1;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userManager.FindByIdAsync(userId);

            if (!String.IsNullOrEmpty(model.Password) && 
                !String.IsNullOrEmpty(model.ConfirmPassword) && 
                model.Password.Equals(model.ConfirmPassword))
            {
                try
                { 
                    var result = _userManager.ChangePasswordAsync(user.Result, model.CurrentPassword, model.Password);
                    if (result.Result.Succeeded)
                    {
                        model.UpdateStatus = 0;
                    }
                }
                catch (Exception e)
                {
                    model.UpdateStatus = 1;
                }
            }

            return View("MyProfile", model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // try to add the claim
                    try
                    {
                        await _userManager.AddClaimAsync(user, new Claim("UserType", model.DonorRecipient));
                    }
                    catch (Exception e)
                    {

                    }

                    // try to add the address
                    UserAddressModel address = new UserAddressModel
                    {
                        Id = user.Id,
                        StreetAddress = model.StreetAddress,
                        StreetAddress2 = model.StreetAddress2,
                        City = model.City,
                        State = model.State,
                        Zip = model.Zip
                    };

                    try
                    {
                        await _userAddressContext.UserAddress.AddAsync(address);
                        await _userAddressContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {

                    }

                    // try to add the user info
                    UserInfoModel info = new UserInfoModel
                    {
                        Id = user.Id,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        PhoneNumber = model.PhoneNumber,
                        TaxID = model.TaxID,
                        OrganizationName = model.OrganizationName
                    };

                    try
                    {
                        await _userInfoContext.UserInfo.AddAsync(info);
                        await _userInfoContext.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {

                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel user)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(user.Email, user.Password, user.RememberMe, false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");

            }
            return View(user);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
