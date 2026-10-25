using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HeaderAppearance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoBorderColor",
                table: "ThemeSettings",
                type: "char(7)",
                nullable: false,
                defaultValue: "#687C50");

            migrationBuilder.AddColumn<bool>(
                name: "LogoBorderEnabled",
                table: "ThemeSettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LogoHeight",
                table: "ThemeSettings",
                type: "int",
                nullable: false,
                defaultValue: 48);

            migrationBuilder.AddColumn<int>(
                name: "LogoRadius",
                table: "ThemeSettings",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<int>(
                name: "ThemeSelectorRadius",
                table: "ThemeSettings",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<int>(
                name: "ThemeSelectorStyle",
                table: "ThemeSettings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoBorderColor",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "LogoBorderEnabled",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "LogoHeight",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "LogoRadius",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "ThemeSelectorRadius",
                table: "ThemeSettings");

            migrationBuilder.DropColumn(
                name: "ThemeSelectorStyle",
                table: "ThemeSettings");
        }
    }
}
