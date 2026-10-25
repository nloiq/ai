using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ContactInquiries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactInquiries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SubmissionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    Phone = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    Company = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Service = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactInquiries", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ContactInquiries_IsRead_CreatedUtc",
                table: "ContactInquiries",
                columns: new[] { "IsRead", "CreatedUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactInquiries_SubmissionId",
                table: "ContactInquiries",
                column: "SubmissionId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactInquiries");
        }
    }
}
