using Microsoft.EntityFrameworkCore.Migrations;

using MyOnlineStoreAPI.Helpers;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: false),
                    PasswordSalt = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    Role = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            var hashResult = PasswordHelper.Hash("123");

            migrationBuilder.InsertData(
                "users",
                new string[] { "Email", "FullName", "PasswordHash", "PasswordSalt", "Role", "IsActive" },
                new object[] { "user@example.com", "Admin", hashResult.PasswordHash, hashResult.Salt, "Admin", true });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
