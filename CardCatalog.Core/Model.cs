using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CardCatalog.Core
{
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
            // this is needed to set a unique constraint on TagTitle column in Tags table
            modelBuilder.Entity<Tag>()
                .HasAlternateKey(c => c.TagTitle)
                .HasName("AlternateKey_TagTitle");
        }
    }

    public class Listing
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        public string Checksum { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public long FileSize { get; set; }
    }

    public class Tag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [Required]
        public string TagTitle { get; set; }
    }

    public class ListingTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int Id { get; set; }

        [ForeignKey("Listing")]
        [Required]
        public Listing ListingRefId { get; set; }

        [ForeignKey("Tag")]
        [Required]
        public Tag TagRefId { get; set; }
    }
}
