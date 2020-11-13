using Microsoft.EntityFrameworkCore;

namespace PAB.Persol.IDP.Entities
{
    public class PABUserContext : DbContext
    {
        public PABUserContext(DbContextOptions<PABUserContext> options)
            : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
    }
}
