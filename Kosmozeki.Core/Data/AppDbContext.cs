using Kosmozeki.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text.Json;

namespace Kosmozeki.Core.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Character> Characters { get; set; }
        public DbSet<RangedWeapon> Arsenal { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            // Автоматически создает файл БД при первом запуске
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Конвертируем сложный Dictionary частей тела в JSON для простого хранения в 1 колонке
            modelBuilder.Entity<Character>()
                .Property(c => c.BodyParts)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<BodyPartType, BodyPart>>(v, (JsonSerializerOptions)null)
                );
        }
    }
}