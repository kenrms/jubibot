using DiscordBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot
{
    public class BotContext : DbContext
    {
        public DbSet<BotConfiguration> BotConfigurations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=bot_data.db");
    }
}