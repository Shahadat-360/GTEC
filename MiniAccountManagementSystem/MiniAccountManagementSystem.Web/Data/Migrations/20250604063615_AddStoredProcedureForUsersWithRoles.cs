using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedureForUsersWithRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            -- Create the corrected stored procedure
            CREATE PROCEDURE sp_GetUsersWithRoles
                @RoleId NVARCHAR(450) = NULL,
                @SearchTerm NVARCHAR(256) = NULL,
                @PageNumber INT = 1,
                @PageSize INT = 10,
                @TotalRecords INT OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
    
                DECLARE @SkipRows INT = (@PageNumber - 1) * @PageSize;
    
                -- Get all users with their roles aggregated
                SELECT 
                    u.Id,
                    u.UserName, 
                    u.Email, 
                    u.EmailConfirmed,
                    u.PhoneNumber,
                    u.LockoutEnabled,
                    u.AccessFailedCount,
                    STRING_AGG(r.Name, ', ') WITHIN GROUP (ORDER BY r.Name) AS Roles
                INTO #FilteredUsers
                FROM AspNetUsers u
                LEFT JOIN AspNetUserRoles ur ON u.Id = ur.UserId
                LEFT JOIN AspNetRoles r ON ur.RoleId = r.Id
                WHERE (@SearchTerm IS NULL OR 
                       u.UserName LIKE '%' + @SearchTerm + '%' OR 
                       u.Email LIKE '%' + @SearchTerm + '%')
                GROUP BY u.Id, u.UserName, u.Email, u.EmailConfirmed, 
                         u.PhoneNumber, u.LockoutEnabled, u.AccessFailedCount;
    
                -- Apply role filter if specified
                IF @RoleId IS NOT NULL
                BEGIN
                    DELETE FROM #FilteredUsers 
                    WHERE Id NOT IN (
                        SELECT DISTINCT ur.UserId 
                        FROM AspNetUserRoles ur 
                        WHERE ur.RoleId = @RoleId
                    );
                END
    
                -- Get total count
                SELECT @TotalRecords = COUNT(*) FROM #FilteredUsers;
    
                -- Return paginated results
                SELECT 
                    Id AS UserId,
                    UserName, 
                    Email, 
                    EmailConfirmed,
                    PhoneNumber,
                    LockoutEnabled,
                    AccessFailedCount,
                    Roles
                FROM (
                    SELECT *,
                           ROW_NUMBER() OVER (ORDER BY UserName) AS RowNum
                    FROM #FilteredUsers
                ) AS PagedResults
                WHERE RowNum BETWEEN (@SkipRows + 1) AND (@SkipRows + @PageSize)
                ORDER BY UserName;
    
                DROP TABLE #FilteredUsers;
            END
            GO
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP PROCEDURE IF EXISTS sp_GetUsersWithRoles;");
        }
    }
}
