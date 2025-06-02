using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProceduresForModuleToRoleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Get all role-module assignments
            migrationBuilder.Sql(@"
            CREATE PROCEDURE sp_GetAllRoleModuleAssignments
            AS
            BEGIN
                SELECT 
                    r.Name as RoleName,
                    m.ModuleName,
                    GETDATE() as AssignedDate
                FROM RoleModules rm
                INNER JOIN AspNetRoles r ON rm.RoleId = r.Id
                INNER JOIN Modules m ON rm.ModuleId = m.ModuleId
                ORDER BY r.Name, m.ModuleName
            END
            ");

            // Get filtered role-module assignments
            migrationBuilder.Sql(@"
            CREATE PROCEDURE sp_GetFilteredRoleModuleAssignments
                @RoleName NVARCHAR(256) = NULL,
                @ModuleName NVARCHAR(100) = NULL
            AS
            BEGIN
                SELECT 
                    r.Name as RoleName,
                    m.ModuleName,
                    GETDATE() as AssignedDate
                FROM RoleModules rm
                INNER JOIN AspNetRoles r ON rm.RoleId = r.Id
                INNER JOIN Modules m ON rm.ModuleId = m.ModuleId
                WHERE 
                    (@RoleName IS NULL OR r.Name = @RoleName)
                    AND (@ModuleName IS NULL OR m.ModuleName = @ModuleName)
                ORDER BY r.Name, m.ModuleName
            END
            ");

            // Get assignment statistics
            migrationBuilder.Sql(@"
            CREATE PROCEDURE sp_GetModuleAssignmentStatistics
            AS
            BEGIN
                SELECT 
                    (SELECT COUNT(*) FROM AspNetRoles) as TotalRoles,
                    (SELECT COUNT(*) FROM Modules) as TotalModules,
                    (SELECT COUNT(*) FROM RoleModules) as TotalAssignments,
                    (SELECT COUNT(DISTINCT RoleId) FROM RoleModules) as RolesWithModules
            END
            ");

            // Get all roles for dropdown
            migrationBuilder.Sql(@"
            CREATE PROCEDURE sp_GetAllRoles
            AS
            BEGIN
                SELECT Name FROM AspNetRoles ORDER BY Name
            END
            ");

            // Get all modules for dropdown
            migrationBuilder.Sql(@"
            CREATE PROCEDURE sp_GetAllModules
            AS
            BEGIN
                SELECT ModuleName FROM Modules ORDER BY ModuleName
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetAllRoleModuleAssignments");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetFilteredRoleModuleAssignments");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetModuleAssignmentStatistics");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetAllRoles");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_GetAllModules");
        }
    }
}
