using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CardCatalog.Core.Migrations
{
    public partial class RemoveContainerAndItemTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppliedTags_Items_Item",
                table: "AppliedTags");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Containers");

            migrationBuilder.DropIndex(
                name: "IX_AppliedTags_Item",
                table: "AppliedTags");

            migrationBuilder.DropColumn(
                name: "Item",
                table: "AppliedTags");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Item",
                table: "AppliedTags",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Containers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Containers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Container = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_AppliedTags_Item",
                table: "AppliedTags",
                column: "Item");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Container",
                table: "Items",
                column: "Container");

            migrationBuilder.AddForeignKey(
                name: "FK_AppliedTags_Items_Item",
                table: "AppliedTags",
                column: "Item",
                principalTable: "Items",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
