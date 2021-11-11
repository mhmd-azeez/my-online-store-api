using Microsoft.EntityFrameworkCore.Migrations;

using MyOnlineStoreAPI.Data;
using MyOnlineStoreAPI.Helpers;

using System;

namespace MyOnlineStoreAPI.Migrations
{
    public partial class Users : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PasswordSalt = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            var hashResult = PasswordHelper.HashPassword("123");

            migrationBuilder.InsertData(
                "users",
                new string[] { nameof(User.Id), nameof(User.FullName), nameof(User.Email), nameof(User.PasswordHash), nameof(User.PasswordSalt), nameof(User.Role), nameof(User.IsActive) },
                new object[] { Guid.NewGuid().ToString(), "Admin User", "admin@example.com", hashResult.PasswordHash, hashResult.PasswordSalt, Roles.Admin, true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
