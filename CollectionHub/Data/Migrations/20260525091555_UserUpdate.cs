using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectionHub.Data.Migrations
{
    // <inheritdoc />
    public partial class UserUpdate : Migration
    {
        // <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MyUser_BuyerId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MyUser_SellerId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserItems_MyUser_UserId",
                table: "UserItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MyUser",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "MyUser");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "MyUser");

            migrationBuilder.RenameTable(
                name: "MyUser",
                newName: "MyUsers");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "MyUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MyUsers",
                table: "MyUsers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_MyUsers_UserID",
                table: "MyUsers",
                column: "UserID",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MyUsers_BuyerId",
                table: "Transactions",
                column: "BuyerId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MyUsers_SellerId",
                table: "Transactions",
                column: "SellerId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserItems_MyUsers_UserId",
                table: "UserItems",
                column: "UserId",
                principalTable: "MyUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        // <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MyUsers_BuyerId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_MyUsers_SellerId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserItems_MyUsers_UserId",
                table: "UserItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MyUsers",
                table: "MyUsers");

            migrationBuilder.DropIndex(
                name: "IX_MyUsers_UserID",
                table: "MyUsers");

            migrationBuilder.RenameTable(
                name: "MyUsers",
                newName: "MyUser");

            migrationBuilder.AlterColumn<string>(
                name: "UserID",
                table: "MyUser",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "MyUser",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "MyUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MyUser",
                table: "MyUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MyUser_BuyerId",
                table: "Transactions",
                column: "BuyerId",
                principalTable: "MyUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_MyUser_SellerId",
                table: "Transactions",
                column: "SellerId",
                principalTable: "MyUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserItems_MyUser_UserId",
                table: "UserItems",
                column: "UserId",
                principalTable: "MyUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
