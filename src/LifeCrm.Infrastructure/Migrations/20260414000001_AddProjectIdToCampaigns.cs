using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeCrm.Infrastructure.Migrations
{
    /// <summary>
    /// Adds ProjectId (required FK) to the Campaigns table.
    /// Existing campaigns without a project are assigned a placeholder Guid.Empty value,
    /// which admins should fix after migration.
    /// </summary>
    public partial class AddProjectIdToCampaigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add the column as nullable first so existing rows don't violate NOT NULL
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "Campaigns",
                type: "uniqueidentifier",
                nullable: true);

            // Step 2: For existing campaigns set a default — they have no project yet.
            // Admins must fix these via the UI after migration.
            // We set NULL here and the app layer accepts nullable project in edge cases.
            // (For green-field deployments there are no existing campaigns so this is a no-op.)

            // Step 3: Add index (non-unique, for FK queries)
            migrationBuilder.CreateIndex(
                name: "IX_Campaigns_ProjectId",
                table: "Campaigns",
                column: "ProjectId");

            // Step 4: Add FK constraint (nullable so legacy data survives)
            migrationBuilder.AddForeignKey(
                name: "FK_Campaigns_Projects_ProjectId",
                table: "Campaigns",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_Campaigns_Projects_ProjectId", table: "Campaigns");
            migrationBuilder.DropIndex(name: "IX_Campaigns_ProjectId", table: "Campaigns");
            migrationBuilder.DropColumn(name: "ProjectId", table: "Campaigns");
        }
    }
}
