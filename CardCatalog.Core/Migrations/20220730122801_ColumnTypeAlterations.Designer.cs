﻿// <auto-generated />
using System;
using CardCatalog.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CardCatalog.Core.Migrations
{
    [DbContext(typeof(CardCatalogContext))]
    [Migration("20220730122801_ColumnTypeAlterations")]
    partial class ColumnTypeAlterations
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "uuid-ossp");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CardCatalog.Core.AppliedTag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<Guid>("File")
                        .HasColumnType("uuid");

                    b.Property<Guid>("Tag")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("File");

                    b.HasIndex("Tag");

                    b.ToTable("AppliedTags");
                });

            modelBuilder.Entity("CardCatalog.Core.File", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<string>("Checksum")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<long>("FileSize")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("FoundOn")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Files");
                });

            modelBuilder.Entity("CardCatalog.Core.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<string>("TagTitle")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasAlternateKey("TagTitle")
                        .HasName("AlternateKey_TagTitle");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("CardCatalog.Core.AppliedTag", b =>
                {
                    b.HasOne("CardCatalog.Core.File", "FileRefId")
                        .WithMany()
                        .HasForeignKey("File")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CardCatalog.Core.Tag", "TagRefId")
                        .WithMany()
                        .HasForeignKey("Tag")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("FileRefId");

                    b.Navigation("TagRefId");
                });
#pragma warning restore 612, 618
        }
    }
}
