using System;
using Microsoft.EntityFrameworkCore;
using CardCatalog.Entities;
using System.Threading.Tasks;

namespace CardCatalog.Core
{
    // 2020-04-08 Originally "netstandard2.0" was TargetFramework for the .csproj
    // may need to switch to that if issues ever come up

    public class CardCatalogContext : DbContext
    {
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingTag> ListingTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

        public CardCatalogContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=card_catalog.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }

    public class Class1
    {
        CardCatalogContext _db;

        public Class1()
        {
            _db = new CardCatalogContext();
        }

        public async Task<bool> CreateListing()
        {
            using (_db)
            {
                _db.Listings.Add(new Listing
                {
                    Checksum = "test",
                    Created = DateTime.Now,
                    FileName = "test3",
                    FilePath = "C:/blah",
                    FileSize = 50,
                });

                var count = await _db.SaveChangesAsync();

                if (count < 1)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}