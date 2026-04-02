using Microsoft.EntityFrameworkCore;

using Boards_WP.Data.Models; // Adjust this to your actual namespace

namespace Boards_WP.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<Community> Communities { get; set; }
        public DbSet<CommunitiesUsers> CommunitiesUsers { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public DbSet<Bet> Bets { get; set; }
        public DbSet<UsersBets> UsersBets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}