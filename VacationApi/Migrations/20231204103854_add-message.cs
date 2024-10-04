using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacationApi.Migrations
{
    public partial class addmessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3ba6384c-4076-4f71-bbaf-54d7a156817d");

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SendDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VacationId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "292f9fc7-2171-4ba3-8985-bf417fe334c8", 0, "c8135cd5-2a84-4d8d-922a-6e90e68f3d10", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "dbb25177-72dd-431f-b217-3a497a64b404", false, "cyril_auquier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "292f9fc7-2171-4ba3-8985-bf417fe334c8");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "3ba6384c-4076-4f71-bbaf-54d7a156817d", 0, "facc2b5f-e18e-41d5-9c37-9d4eae3e56f5", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "f39299ad-0035-412a-8d20-0f0114321105", false, "cyril_auquier" });
        }
    }
}
