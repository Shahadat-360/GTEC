using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoredProceduresForModuleToRoleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Get all role-module assignments
            migrationBuilder.Sql(@"
            CREATE or alter PROCEDURE sp_GetAllRoleModuleAssignments
            AS
            BEGIN
                SELECT 
                    r.Name as RoleName,
                    m.ModuleName,
                    rm.AssignedDate as AssignedDate
                FROM RoleModules rm
                INNER JOIN AspNetRoles r ON rm.RoleId = r.Id
                INNER JOIN Modules m ON rm.ModuleId = m.ModuleId
                ORDER BY r.Name, m.ModuleName
            END
            ");

            // Get filtered role-module assignments
            migrationBuilder.Sql(@"
            CREATE or alter PROCEDURE sp_GetFilteredRoleModuleAssignments
                @RoleName NVARCHAR(256) = NULL,
                @ModuleName NVARCHAR(100) = NULL
            AS
            BEGIN
                SELECT 
                    r.Name as RoleName,
                    m.ModuleName,
                    rm.AssignedDate as AssignedDate
                FROM RoleModules rm
                INNER JOIN AspNetRoles r ON rm.RoleId = r.Id
                INNER JOIN Modules m ON rm.ModuleId = m.ModuleId
                WHERE 
                    (@RoleName IS NULL OR r.Name = @RoleName)
                    AND (@ModuleName IS NULL OR m.ModuleName = @ModuleName)
                ORDER BY r.Name, m.ModuleName
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetAllRoleModuleAssignments");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetFilteredRoleModuleAssignments");
        }
    }
}
