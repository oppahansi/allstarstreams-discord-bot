using allstars.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.IO;

namespace allstars.Contexts
{
    public class BotDbCtxDesign : IDesignTimeDbContextFactory<BotDbContext>
    {
        public BotDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();
            optionsBuilder.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, Constants.ConfigDbName)}");

            return new BotDbContext(optionsBuilder.Options);
        }
    }
}