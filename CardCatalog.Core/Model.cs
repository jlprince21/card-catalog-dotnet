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
        public DbSet<File> Files { get; set; }

        // physical object tables
        public DbSet<Container> Containers { get; set; }
        public DbSet<Item> Items { get; set; }

        // shared tables
        public DbSet<AppliedTag> AppliedTags { get; set; }
        public DbSet<Tag> Tags { get; set; }

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
            modelBuilder.Entity<Item>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");
            modelBuilder.Entity<Container>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");

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

        public string Checksum { get; set; }

        [Required]
        public DateTime FoundOn { get; set; }

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

    public class AppliedTag
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("File")]
        public virtual File FileRefId { get; set; }

        [ForeignKey("Item")]
        public virtual Item ItemRefId { get; set; }

        [ForeignKey("Tag")]
        [Required]
        public Tag TagRefId { get; set; }
    }

    public class Item
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("Container")]
        [Required]
        public Container ContainerRefId { get; set; }

        [Required]
        public string Description { get; set; }
    }

    public class Container
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
