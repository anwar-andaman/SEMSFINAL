using Microsoft.EntityFrameworkCore;
using System.Security.Permissions;

namespace SEMS.Models
{
    public class ElectionDBContext : DbContext
    {
        public ElectionDBContext(DbContextOptions options) : base(options) 
        {
            
        }
        
        public DbSet<UserModel> Users { get; set; }
    }
}
