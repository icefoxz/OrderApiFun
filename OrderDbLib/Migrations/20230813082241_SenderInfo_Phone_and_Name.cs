using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderDbLib.Migrations
{
    /// <inheritdoc />
    public partial class SenderInfo_Phone_and_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderInfo_Name",
                table: "DeliveryOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderInfo_NormalizedPhoneNumber",
                table: "DeliveryOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderInfo_PhoneNumber",
                table: "DeliveryOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderInfo_Name",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "SenderInfo_NormalizedPhoneNumber",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "SenderInfo_PhoneNumber",
                table: "DeliveryOrders");
        }
    }
}
