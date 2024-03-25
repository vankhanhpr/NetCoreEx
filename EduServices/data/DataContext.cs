using Microsoft.EntityFrameworkCore;
using ModelClass.user;
using System.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FuBonServices.data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }
        public DbSet<User> users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            //builder.Entity<SalaryProcess>().HasKey(i => new { i.storeid, i.processid });
        }
    }
}
