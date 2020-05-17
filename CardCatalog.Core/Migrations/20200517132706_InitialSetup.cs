using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CardCatalog.Core.Migrations
{
    public partial class InitialSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Containers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Containers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    Checksum = table.Column<string>(nullable: true),
                    FoundOn = table.Column<DateTime>(nullable: false),
                    FileName = table.Column<string>(nullable: false),
                    FilePath = table.Column<string>(nullable: false),
                    FileSize = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    TagTitle = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.UniqueConstraint("AlternateKey_TagTitle", x => x.TagTitle);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    Container = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Containers_Container",
                        column: x => x.Container,
                        principalTable: "Containers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AppliedTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "NEWID()"),
                    File = table.Column<Guid>(nullable: true),
                    Item = table.Column<Guid>(nullable: true),
                    Tag = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppliedTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppliedTags_Files_File",
                        column: x => x.File,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppliedTags_Items_Item",
                        column: x => x.Item,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppliedTags_Tags_Tag",
                        column: x => x.Tag,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppliedTags_File",
                table: "AppliedTags",
                column: "File");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedTags_Item",
                table: "AppliedTags",
                column: "Item");

            migrationBuilder.CreateIndex(
                name: "IX_AppliedTags_Tag",
                table: "AppliedTags",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Container",
                table: "Items",
                column: "Container");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppliedTags");

            migrationBuilder.DropTable(
                name: "Files");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Containers");
        }
    }
}
