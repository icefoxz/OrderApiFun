using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderDbLib.Migrations
{
    /// <inheritdoc />
    public partial class lingau_userRefId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lingaus_AspNetUsers_UserId",
                table: "Lingaus");

            migrationBuilder.DropForeignKey(
                name: "FK_Lingaus_AspNetUsers_UserId1",
                table: "Lingaus");

            migrationBuilder.DropIndex(
                name: "IX_Lingaus_UserId",
                table: "Lingaus");

            migrationBuilder.DropIndex(
                name: "IX_Lingaus_UserId1",
                table: "Lingaus");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Lingaus");

            migrationBuilder.RenameColumn(
                name: "UserId1",
                table: "Lingaus",
                newName: "UserRefId");

            migrationBuilder.AddColumn<string>(
                name: "LingauId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lingaus_UserRefId",
                table: "Lingaus",
                column: "UserRefId",
                unique: true,
                filter: "[UserRefId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_LingauId",
                table: "AspNetUsers",
                column: "LingauId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Lingaus_LingauId",
                table: "AspNetUsers",
                column: "LingauId",
                principalTable: "Lingaus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lingaus_AspNetUsers_UserRefId",
                table: "Lingaus",
                column: "UserRefId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Lingaus_LingauId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Lingaus_AspNetUsers_UserRefId",
                table: "Lingaus");

            migrationBuilder.DropIndex(
                name: "IX_Lingaus_UserRefId",
                table: "Lingaus");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_LingauId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LingauId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "UserRefId",
                table: "Lingaus",
                newName: "UserId1");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Lingaus",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lingaus_UserId",
                table: "Lingaus",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lingaus_UserId1",
                table: "Lingaus",
                column: "UserId1",
                unique: true,
                filter: "[UserId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Lingaus_AspNetUsers_UserId",
                table: "Lingaus",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lingaus_AspNetUsers_UserId1",
                table: "Lingaus",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
