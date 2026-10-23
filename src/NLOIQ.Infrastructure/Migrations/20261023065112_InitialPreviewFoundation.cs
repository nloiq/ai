using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialPreviewFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServicePreviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Key = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsDemo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePreviews", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ServicePreviewTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ServicePreviewId = table.Column<int>(type: "int", nullable: false),
                    Culture = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    Title = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false),
                    Summary = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePreviewTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePreviewTranslations_ServicePreviews_ServicePreviewId",
                        column: x => x.ServicePreviewId,
                        principalTable: "ServicePreviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePreviews_DisplayOrder",
                table: "ServicePreviews",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePreviews_Key",
                table: "ServicePreviews",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicePreviewTranslations_ServicePreviewId_Culture",
                table: "ServicePreviewTranslations",
                columns: new[] { "ServicePreviewId", "Culture" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicePreviewTranslations");

            migrationBuilder.DropTable(
                name: "ServicePreviews");
        }
    }
}
