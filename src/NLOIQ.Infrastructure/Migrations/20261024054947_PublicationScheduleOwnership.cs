using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NLOIQ.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PublicationScheduleOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PublicationSchedules_RevisionHistoryTemp",
                table: "PublicationSchedules",
                columns: new[] { "ContentItemId", "RevisionId" });
            migrationBuilder.DropIndex(
                name: "IX_PublicationSchedules_ContentItemId_RevisionId",
                table: "PublicationSchedules");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "PublicationSchedules",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AddColumn<string>(
                name: "RequestedByLabel",
                table: "PublicationSchedules",
                type: "varchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RequestedByUserId",
                table: "PublicationSchedules",
                type: "char(36)",
                nullable: true);

            migrationBuilder.RenameIndex(
                name: "IX_PublicationSchedules_RevisionHistoryTemp",
                table: "PublicationSchedules",
                newName: "IX_PublicationSchedules_ContentItemId_RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicationSchedules_RequestedByUserId",
                table: "PublicationSchedules",
                column: "RequestedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicationSchedules_AspNetUsers_RequestedByUserId",
                table: "PublicationSchedules",
                column: "RequestedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Fails without removing history if repeat schedules exist; reconcile explicitly before downgrade.
            migrationBuilder.CreateIndex(
                name: "IX_PublicationSchedules_RevisionHistoryTemp",
                table: "PublicationSchedules",
                columns: new[] { "ContentItemId", "RevisionId" }, unique: true);

            migrationBuilder.DropForeignKey(
                name: "FK_PublicationSchedules_AspNetUsers_RequestedByUserId",
                table: "PublicationSchedules");

            migrationBuilder.DropIndex(
                name: "IX_PublicationSchedules_ContentItemId_RevisionId",
                table: "PublicationSchedules");

            migrationBuilder.DropIndex(
                name: "IX_PublicationSchedules_RequestedByUserId",
                table: "PublicationSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "PublicationSchedules");

            migrationBuilder.DropColumn(
                name: "RequestedByLabel",
                table: "PublicationSchedules");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "PublicationSchedules");

            migrationBuilder.RenameIndex(
                name: "IX_PublicationSchedules_RevisionHistoryTemp",
                table: "PublicationSchedules",
                newName: "IX_PublicationSchedules_ContentItemId_RevisionId");
        }
    }
}
