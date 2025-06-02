using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeModuleToModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleModule_AspNetRoles_RoleId",
                table: "RoleModule");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleModule_Modules_ModuleId",
                table: "RoleModule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleModule",
                table: "RoleModule");

            migrationBuilder.RenameTable(
                name: "RoleModule",
                newName: "RoleModules");

            migrationBuilder.RenameIndex(
                name: "IX_RoleModule_RoleId_ModuleId",
                table: "RoleModules",
                newName: "IX_RoleModules_RoleId_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleModule_ModuleId",
                table: "RoleModules",
                newName: "IX_RoleModules_ModuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleModules",
                table: "RoleModules",
                column: "RoleModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleModules_AspNetRoles_RoleId",
                table: "RoleModules",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleModules_Modules_ModuleId",
                table: "RoleModules",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoleModules_AspNetRoles_RoleId",
                table: "RoleModules");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleModules_Modules_ModuleId",
                table: "RoleModules");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoleModules",
                table: "RoleModules");

            migrationBuilder.RenameTable(
                name: "RoleModules",
                newName: "RoleModule");

            migrationBuilder.RenameIndex(
                name: "IX_RoleModules_RoleId_ModuleId",
                table: "RoleModule",
                newName: "IX_RoleModule_RoleId_ModuleId");

            migrationBuilder.RenameIndex(
                name: "IX_RoleModules_ModuleId",
                table: "RoleModule",
                newName: "IX_RoleModule_ModuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoleModule",
                table: "RoleModule",
                column: "RoleModuleId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleModule_AspNetRoles_RoleId",
                table: "RoleModule",
                column: "RoleId",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoleModule_Modules_ModuleId",
                table: "RoleModule",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "ModuleId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
