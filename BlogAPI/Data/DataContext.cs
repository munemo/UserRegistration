

using Microsoft.AspNetCore.Identity;

namespace BlogAPI.Data
{
    public class DataContext : IdentityDbContext<IdentityUser>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

       /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("server=localhost\\sqlexpress;database=Blog;trusted_connection=true");
        }*/

        public DbSet<User> Users => Set<User>();
    }
}
