using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VacationApi.Migrations
{
    public partial class aftermerge : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "d3c26764-edf6-47c4-9740-48a03d61ac34");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "399bd445-47e3-421b-82ac-6c45dcb6d1bb", 0, "c3f7bbce-89ee-4168-b0fa-2efc8e1112a6", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "17045a95-f7f7-42a2-9d9f-f74fe98da0ea", false, "cyril_auquier" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "399bd445-47e3-421b-82ac-6c45dcb6d1bb");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "FirstName", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PicturePath", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "d3c26764-edf6-47c4-9740-48a03d61ac34", 0, "3239d63b-7384-4990-bf27-24a3b5ec06d2", "cyril.auquier@vacation.api", false, "Cyril", false, null, "Auquier", null, null, null, null, false, "url", "02f9e068-742f-4651-a055-6154af18968e", false, "cyril_auquier" });
        }
    }
}
