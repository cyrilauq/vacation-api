using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacationApi.Migrations
{
    public partial class ModelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "761d3d86-97a7-448d-aaed-1b593392d5c0");

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                table: "VacationActivity",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Begin",
                table: "VacationActivity",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "3ba6384c-4076-4f71-bbaf-54d7a156817d", 0, "facc2b5f-e18e-41d5-9c37-9d4eae3e56f5", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "f39299ad-0035-412a-8d20-0f0114321105", false, "cyril_auquier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "3ba6384c-4076-4f71-bbaf-54d7a156817d");

            migrationBuilder.AlterColumn<DateTime>(
                name: "End",
                table: "VacationActivity",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Begin",
                table: "VacationActivity",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "761d3d86-97a7-448d-aaed-1b593392d5c0", 0, "f12558da-83da-4fe0-b69b-d6b3d05f0294", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "49ceb4a5-375a-449b-bc3e-add96db11605", false, "cyril_auquier" });
        }
    }
}
