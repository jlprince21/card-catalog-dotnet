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
        //     => options.UseSqlite(_connection);

        // eg: "Data Source=/path/to/card_catalog_core.db";
        private readonly string  _connection = Environment.GetEnvironmentVariable("CARD_CATALOG_SQLITE_PATH");

        public CardCatalogContext() {}
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(_connection);
        }

        public CardCatalogContext(DbContextOptions<CardCatalogContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<File>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Tag>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<AppliedTag>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Item>().Property(x => x.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Container>().Property(x => x.Id).HasDefaultValueSql("NEWID()");

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
