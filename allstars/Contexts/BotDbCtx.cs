using allstars.Models;
using Microsoft.EntityFrameworkCore;

namespace allstars.Contexts
{
    public class BotDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<QuickHelp> QuickHelps { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<SpecialChannel> SpecialChannels { get; set; }
        public DbSet<Mute> Mutes { get; set; }
        public DbSet<CmdChannel> CmdChannels { get; set; }
        public DbSet<CmdRole> CmdRoles { get; set; }
        public DbSet<CmdUserCd> CmdUserCds { get; set; }
        public DbSet<UpcomingRelease> UpcomingReleases { get; set; }
        public DbSet<AutoMessage> AutoMessages { get; set; }

        public BotDbContext(DbContextOptions<BotDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Mute>().HasKey(x => new { x.UserId, x.Expires });
            modelBuilder.Entity<CmdChannel>().HasKey(x => new { x.Command, x.ChannelId });
            modelBuilder.Entity<CmdUserCd>().HasKey(x => new { x.Command, x.UserId });
            modelBuilder.Entity<AutoMessage>().HasKey(x => new { x.Channel, x.Message });
        }
    }
}