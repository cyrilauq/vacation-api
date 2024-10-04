using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacationApi.Migrations
{
    public partial class UpdateVacation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "292f9fc7-2171-4ba3-8985-bf417fe334c8");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Vacations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PicturePath",
                table: "Vacations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "02759a24-1b0b-4b15-8fbc-2134fd6d8cb8", 0, "a9bf9d05-ac49-4f3c-bb98-c2b077d55054", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "86c53c2d-9801-40ce-bd5b-dd0c61ecb248", false, "cyril_auquier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "02759a24-1b0b-4b15-8fbc-2134fd6d8cb8");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Vacations");

            migrationBuilder.DropColumn(
                name: "PicturePath",
                table: "Vacations");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "292f9fc7-2171-4ba3-8985-bf417fe334c8", 0, "c8135cd5-2a84-4d8d-922a-6e90e68f3d10", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "dbb25177-72dd-431f-b217-3a497a64b404", false, "cyril_auquier" });
        }
    }
}
