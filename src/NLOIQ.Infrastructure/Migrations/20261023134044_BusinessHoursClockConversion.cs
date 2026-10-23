using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BusinessHoursClockConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OpensAt",
                table: "BusinessHours",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "ClosesAt",
                table: "BusinessHours",
                type: "time(6)",
                nullable: true,
                oldClrType: typeof(TimeOnly),
                oldType: "time",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeOnly>(
                name: "OpensAt",
                table: "BusinessHours",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<TimeOnly>(
                name: "ClosesAt",
                table: "BusinessHours",
                type: "time",
                nullable: true,
                oldClrType: typeof(TimeSpan),
                oldType: "time(6)",
                oldNullable: true);
        }
    }
}
