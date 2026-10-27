using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ServiceCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryKey",
                table: "Services",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "general");
            migrationBuilder.Sql("UPDATE `Services` s INNER JOIN `ContentRevisions` r ON r.`Id`=s.`RevisionId` INNER JOIN `ContentItems` i ON i.`Id`=r.`ContentItemId` SET s.`CategoryKey`=i.`Key` WHERE i.`Key` IN ('low-current','software','scada','automation');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryKey",
                table: "Services");
        }
    }
}
