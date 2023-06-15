using Microsoft.EntityFrameworkCore;
using System.IO;

namespace RG39.Data
{
    internal class MyContext : DbContext
    {
        internal DbSet<GameDTO> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            const string databaseFolder = "./Database/";
            if (!Directory.Exists(databaseFolder))
            {
                Directory.CreateDirectory(databaseFolder);
            }
            optionsBuilder.UseSqlite($"Data Source={databaseFolder}List.db");
        }
    }
}
