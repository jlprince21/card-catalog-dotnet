// System namespaces

// Third-party namespaces (NuGet packages)
using Microsoft.EntityFrameworkCore;

// Project-specific namespaces

namespace CardCatalog.Core;

public class CardCatalogContext : DbContext
{
    // computer file tables
    public DbSet<Models.File> Files { get; set; }  = default!;

    // shared tables
    public DbSet<Models.AppliedTag> AppliedTags { get; set; }  = default!;
    public DbSet<Models.Tag> Tags { get; set; }  = default!;
    public DbSet<Models.Job> Jobs { get; set; }  = default!;

    private readonly string _connection = Environment.GetEnvironmentVariable("CARD_CATALOG_DB_CONNECTION");

    public CardCatalogContext() {}
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connection, o => o.UseNodaTime()); // Enable NodaTime support for Npgsql
    }

    public CardCatalogContext(DbContextOptions<CardCatalogContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp"); // remember to enable extension in database before running initial migrations

        modelBuilder.Entity<Models.File>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");
        modelBuilder.Entity<Models.Tag>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");
        modelBuilder.Entity<Models.AppliedTag>().Property(x => x.Id).HasColumnType("uuid").HasDefaultValueSql("uuid_generate_v4()");

        // this is needed to set a unique constraint on TagTitle column in Tags table
        modelBuilder.Entity<Models.Tag>()
            .HasAlternateKey(c => c.TagTitle)
            .HasName("AlternateKey_TagTitle");

        // force all foreign key contraints to restrict deletion
        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }
}
