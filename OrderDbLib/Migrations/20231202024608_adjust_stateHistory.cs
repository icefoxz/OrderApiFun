using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderDbLib.Migrations
{
    /// <inheritdoc />
    public partial class adjust_stateHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StatusHistory",
                table: "DeliveryOrders",
                newName: "StateHistory");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentInfo_TransactionId",
                table: "DeliveryOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentInfo_Method",
                table: "DeliveryOrders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StateHistory",
                table: "DeliveryOrders",
                newName: "StatusHistory");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentInfo_TransactionId",
                table: "DeliveryOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentInfo_Method",
                table: "DeliveryOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
