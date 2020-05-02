using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CardCatalog.Core
{
    public class CardCatalogContext : DbContext
    {
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingTag> ListingTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

        // NOTE 2020-05-02 This constructor and OnConfiguring were used when the
        // CLI project was main way of interacting with the Core project. If troubles
        // arise using migrations or when attaching another project to the solution
        // may need these back in here.
        // public CardCatalogContext()
        // {
        //      // NOTE 2020-04-23 this may come in handy for verifying the database
        //      // is created at startup in other projects but it will cause issues
        //      // when trying to run migrations inside this project. Keeping just in case
        //      // Database.EnsureCreated();
        // }

        // protected override void OnConfiguring(DbContextOptionsBuilder options)
        //     => options.UseSqlite("Data Source=card_catalog.db");

        public CardCatalogContext(DbContextOptions<CardCatalogContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Listing>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Tag>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<ListingTag>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

            // this is needed to set a unique constraint on TagTitle column in Tags table
            modelBuilder.Entity<Tag>()
                .HasAlternateKey(c => c.TagTitle)
                .HasName("AlternateKey_TagTitle");

            // force all foreign key contraints to restrict deletion
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

    public class Listing
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

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
        public Guid Id { get; set; }

        [Required]
        public string TagTitle { get; set; }
    }

    public class ListingTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Listing")]
        [Required]
        public Listing ListingRefId { get; set; }

        [ForeignKey("Tag")]
        [Required]
        public Tag TagRefId { get; set; }
    }
}
