using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using LeftRover.Models;
using System.Threading.Tasks;

namespace LeftRover.Models.AppDBContext
{
    public class UserAddressDBContext : DbContext
    {
        public UserAddressDBContext(DbContextOptions<UserAddressDBContext> options)
            : base(options)
        {

        }

        public DbSet<UserAddressModel> UserAddress { get; set; }
    }
}
