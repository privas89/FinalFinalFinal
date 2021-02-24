using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeftRover.Models;
using Microsoft.EntityFrameworkCore;

namespace LeftRover.Models.AppDBContext
{
    public class DonationsDBContext : DbContext
    {
        public DonationsDBContext(DbContextOptions<DonationsDBContext> options)
            : base(options)
        {

        }

        public DbSet<DonationsModel> Donations { get; set; }
    }
}
