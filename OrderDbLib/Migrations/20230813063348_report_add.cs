using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderDbLib.Migrations
{
    /// <inheritdoc />
    public partial class report_add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderTag");

            migrationBuilder.RenameColumn(
                name: "DeliveryInfo_Fee",
                table: "DeliveryOrders",
                newName: "PaymentInfo_Fee");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_Price",
                table: "DeliveryOrders",
                newName: "PaymentInfo_Charge");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_PaymentReference",
                table: "DeliveryOrders",
                newName: "PaymentInfo_Reference");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_PaymentReceived",
                table: "DeliveryOrders",
                newName: "PaymentInfo_IsReceived");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_PaymentMethod",
                table: "DeliveryOrders",
                newName: "PaymentInfo_TransactionId");

            migrationBuilder.AlterColumn<double>(
                name: "ItemInfo_Volume",
                table: "DeliveryOrders",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<float>(
                name: "PaymentInfo_Fee",
                table: "DeliveryOrders",
                type: "real",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "real");

            migrationBuilder.AddColumn<int>(
                name: "PaymentInfo_Method",
                table: "DeliveryOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    TimeOfOccurrence = table.Column<long>(type: "bigint", nullable: false),
                    IncidentDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImpactDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resolve_Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resolve_OrderStatus = table.Column<int>(type: "int", nullable: true),
                    Resolve_PaymentStatus = table.Column<int>(type: "int", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reports_DeliveryOrders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "DeliveryOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeliveryOrderId = table.Column<int>(type: "int", nullable: true),
                    ReportId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tag_DeliveryOrders_DeliveryOrderId",
                        column: x => x.DeliveryOrderId,
                        principalTable: "DeliveryOrders",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tag_Reports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "Reports",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_OrderId",
                table: "Reports",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_DeliveryOrderId",
                table: "Tag",
                column: "DeliveryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_ReportId",
                table: "Tag",
                column: "ReportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropColumn(
                name: "PaymentInfo_Method",
                table: "DeliveryOrders");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_Fee",
                table: "DeliveryOrders",
                newName: "DeliveryInfo_Fee");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_TransactionId",
                table: "DeliveryOrders",
                newName: "PaymentInfo_PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_Reference",
                table: "DeliveryOrders",
                newName: "PaymentInfo_PaymentReference");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_IsReceived",
                table: "DeliveryOrders",
                newName: "PaymentInfo_PaymentReceived");

            migrationBuilder.RenameColumn(
                name: "PaymentInfo_Charge",
                table: "DeliveryOrders",
                newName: "PaymentInfo_Price");

            migrationBuilder.AlterColumn<int>(
                name: "ItemInfo_Volume",
                table: "DeliveryOrders",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<float>(
                name: "DeliveryInfo_Fee",
                table: "DeliveryOrders",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                oldClrType: typeof(float),
                oldType: "real",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "OrderTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<long>(type: "bigint", nullable: false),
                    DeliveryOrderId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderTag_DeliveryOrders_DeliveryOrderId",
                        column: x => x.DeliveryOrderId,
                        principalTable: "DeliveryOrders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderTag_DeliveryOrderId",
                table: "OrderTag",
                column: "DeliveryOrderId");
        }
    }
}
