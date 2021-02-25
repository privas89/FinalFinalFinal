using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeftRover.Models;
using Microsoft.EntityFrameworkCore;

namespace LeftRover.Models.AppDBContext
{
    public class LeftRoverDBContext : DbContext
    {
        public LeftRoverDBContext(DbContextOptions<LeftRoverDBContext> options)
            : base(options)
        {

        }

        public DbSet<DonationsModel> Donations { get; set; }
        public DbSet<UserAddressModel> UserAddress { get; set; }
        public DbSet<UserInfoModel> UserInfoModel { get; set; }
        public DbSet<DonationClaimsModel> DonationClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<DonationClaimsModel>().HasKey(table => new {
                table.DonationID,
                table.UserId
            });
        }
    }
}
