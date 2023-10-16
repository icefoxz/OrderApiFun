using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderDbLib.Migrations
{
    /// <inheritdoc />
    public partial class Do_Sender_UserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderInfo_SenderUserId",
                table: "DeliveryOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderInfo_SenderUserId",
                table: "DeliveryOrders",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
