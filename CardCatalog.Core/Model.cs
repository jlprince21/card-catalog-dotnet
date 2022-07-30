using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CardCatalog.Core
{
    public class CardCatalogContext : DbContext
    {
        // computer file tables
        public DbSet<File> Files { get; set; }  = default!;

        // shared tables
        public DbSet<AppliedTag> AppliedTags { get; set; }  = default!;
        public DbSet<Tag> Tags { get; set; }  = default!;

        private readonly string _connection = Environment.GetEnvironmentVariable("CARD_CATALOG_DB_CONNECTION");

        public CardCatalogContext() {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connection);
        }

        public CardCatalogContext(DbContextOptions<CardCatalogContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("uuid-ossp"); // remember to enable extension in database before running initial migrations

            modelBuilder.Entity<File>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Tag>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<AppliedTag>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");

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

    public class File
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        public string Checksum { get; set; }  = default!;

        [Required]
        public DateTime FoundOn { get; set; }

        [Required]
        public string FileName { get; set; }  = default!;

        [Required]
        public string FilePath { get; set; }  = default!;

        public long FileSize { get; set; }
    }

    public class Tag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string TagTitle { get; set; }  = default!;
    }

    public class AppliedTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("File")]
        public virtual File FileRefId { get; set; }  = default!;

        [ForeignKey("Tag")]
        [Required]
        public Tag TagRefId { get; set; }  = default!;
    }
}
