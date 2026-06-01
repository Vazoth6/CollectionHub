using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectionHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserUpdateTwo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Transactions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CellPhone",
                table: "MyUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "CellPhone",
                table: "MyUsers");
        }
    }
}
