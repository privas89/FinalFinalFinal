using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using LeftRover.Models;
using System.Threading.Tasks;

namespace LeftRover.Models.AppDBContext
{
    public class UserInfoDBContext : DbContext
    {
        public UserInfoDBContext(DbContextOptions<UserInfoDBContext> options)
        :   base(options)
        {

        }

        public DbSet<UserInfoModel> UserInfo { get; set; }
    }
}
