using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectionHub.Data.Migrations
{
    // <inheritdoc />
    public partial class AddItemLikes : Migration
    {
        // <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemLikes_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemLikes_MyUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "MyUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemLikes_ItemId",
                table: "ItemLikes",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemLikes_UserId_ItemId",
                table: "ItemLikes",
                columns: new[] { "UserId", "ItemId" },
                unique: true);
        }

        // <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemLikes");
        }
    }
}
