using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStoredProceduresForAssigningModuleToRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE or alter PROCEDURE sp_AssignModuleToRole
                @RoleName NVARCHAR(256),
                @ModuleName NVARCHAR(100)
            AS
            BEGIN
                DECLARE @RoleId NVARCHAR(450)
                DECLARE @ModuleId INT

                SELECT @RoleId = Id FROM AspNetRoles WHERE Name = @RoleName
                SELECT @ModuleId = ModuleId FROM Modules WHERE ModuleName = @ModuleName

                IF @RoleId IS NULL OR @ModuleId IS NULL
                    RETURN

                IF NOT EXISTS (
                    SELECT 1 FROM RoleModules WHERE RoleId = @RoleId AND ModuleId = @ModuleId
                )
                BEGIN
                    INSERT INTO RoleModules (RoleId, ModuleId,AssignedDate)
                    VALUES (@RoleId, @ModuleId,GETDATE())
                END
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_AssignModuleToRole");
        }
    }
}
