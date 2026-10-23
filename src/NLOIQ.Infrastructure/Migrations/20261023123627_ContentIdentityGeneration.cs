using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;
#nullable disable
namespace NLOIQ.Infrastructure.Migrations;
public partial class ContentIdentityGeneration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DropForeignKey(name: "FK_ContentRevisions_ContentItems_ContentItemId", table: "ContentRevisions");
            migrationBuilder.DropForeignKey(name: "FK_NavigationItems_ContentItems_TargetContentItemId", table: "NavigationItems");
            migrationBuilder.DropForeignKey(name: "FK_AuditLogs_ContentItems_ContentItemId", table: "AuditLogs");
            migrationBuilder.DropForeignKey(name: "FK_ContentItems_ContentRevisions_Id_DraftRevisionId", table: "ContentItems");
            migrationBuilder.DropForeignKey(name: "FK_ContentItems_ContentRevisions_Id_PublishedRevisionId", table: "ContentItems");
        migrationBuilder.AlterColumn<long>(name: "Id", table: "ContentItems", type: "bigint", nullable: false, oldClrType: typeof(long), oldType: "bigint").Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
            migrationBuilder.AddForeignKey(name: "FK_ContentRevisions_ContentItems_ContentItemId", table: "ContentRevisions", column: "ContentItemId", principalTable: "ContentItems", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_NavigationItems_ContentItems_TargetContentItemId", table: "NavigationItems", column: "TargetContentItemId", principalTable: "ContentItems", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_AuditLogs_ContentItems_ContentItemId", table: "AuditLogs", column: "ContentItemId", principalTable: "ContentItems", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ContentItems_ContentRevisions_Id_DraftRevisionId", table: "ContentItems", columns: new[] { "Id", "DraftRevisionId" }, principalTable: "ContentRevisions", principalColumns: new[] { "ContentItemId", "Id" }, onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ContentItems_ContentRevisions_Id_PublishedRevisionId", table: "ContentItems", columns: new[] { "Id", "PublishedRevisionId" }, principalTable: "ContentRevisions", principalColumns: new[] { "ContentItemId", "Id" }, onDelete: ReferentialAction.Restrict);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DropForeignKey(name: "FK_ContentRevisions_ContentItems_ContentItemId", table: "ContentRevisions");
            migrationBuilder.DropForeignKey(name: "FK_NavigationItems_ContentItems_TargetContentItemId", table: "NavigationItems");
            migrationBuilder.DropForeignKey(name: "FK_AuditLogs_ContentItems_ContentItemId", table: "AuditLogs");
            migrationBuilder.DropForeignKey(name: "FK_ContentItems_ContentRevisions_Id_DraftRevisionId", table: "ContentItems");
            migrationBuilder.DropForeignKey(name: "FK_ContentItems_ContentRevisions_Id_PublishedRevisionId", table: "ContentItems");
        migrationBuilder.AlterColumn<long>(name: "Id", table: "ContentItems", type: "bigint", nullable: false, oldClrType: typeof(long), oldType: "bigint").OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
            migrationBuilder.AddForeignKey(name: "FK_ContentRevisions_ContentItems_ContentItemId", table: "ContentRevisions", column: "ContentItemId", principalTable: "ContentItems", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_NavigationItems_ContentItems_TargetContentItemId", table: "NavigationItems", column: "TargetContentItemId", principalTable: "ContentItems", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_AuditLogs_ContentItems_ContentItemId", table: "AuditLogs", column: "ContentItemId", principalTable: "ContentItems", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ContentItems_ContentRevisions_Id_DraftRevisionId", table: "ContentItems", columns: new[] { "Id", "DraftRevisionId" }, principalTable: "ContentRevisions", principalColumns: new[] { "ContentItemId", "Id" }, onDelete: ReferentialAction.Restrict);
            migrationBuilder.AddForeignKey(name: "FK_ContentItems_ContentRevisions_Id_PublishedRevisionId", table: "ContentItems", columns: new[] { "Id", "PublishedRevisionId" }, principalTable: "ContentRevisions", principalColumns: new[] { "ContentItemId", "Id" }, onDelete: ReferentialAction.Restrict);
    }
}
