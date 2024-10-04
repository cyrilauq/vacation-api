using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacationApi.Migrations
{
    public partial class UpdateVacation2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "02759a24-1b0b-4b15-8fbc-2134fd6d8cb8");

            migrationBuilder.AlterColumn<string>(
                name: "PicturePath",
                table: "Vacations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "d3c26764-edf6-47c4-9740-48a03d61ac34", 0, "3239d63b-7384-4990-bf27-24a3b5ec06d2", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "02f9e068-742f-4651-a055-6154af18968e", false, "cyril_auquier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d3c26764-edf6-47c4-9740-48a03d61ac34");

            migrationBuilder.AlterColumn<string>(
                name: "PicturePath",
                table: "Vacations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "02759a24-1b0b-4b15-8fbc-2134fd6d8cb8", 0, "a9bf9d05-ac49-4f3c-bb98-c2b077d55054", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "86c53c2d-9801-40ce-bd5b-dd0c61ecb248", false, "cyril_auquier" });
        }
    }
}
