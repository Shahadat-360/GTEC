using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChartOfAccountsTableAndStoredProcedure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChartOfAccounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ParentAccountId = table.Column<int>(type: "int", nullable: true),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartOfAccounts", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_ChartOfAccounts_ChartOfAccounts_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalTable: "ChartOfAccounts",
                        principalColumn: "AccountId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_AccountCode",
                table: "ChartOfAccounts",
                column: "AccountCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChartOfAccounts_ParentAccountId",
                table: "ChartOfAccounts",
                column: "ParentAccountId");

            // Create stored procedure for managing chart of accounts
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_CreateChartOfAccount]
                    @AccountCode NVARCHAR(20),
                    @AccountName NVARCHAR(100),
                    @Description NVARCHAR(500) = NULL,
                    @ParentAccountId INT = NULL,
                    @AccountType INT,
                    @IsActive BIT = 1
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Check if account code already exists
                    IF EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = @AccountCode)
                    BEGIN
                        RAISERROR('Account code already exists', 16, 1);
                        RETURN;
                    END
                    
                    -- Check if parent account exists if provided
                    IF @ParentAccountId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountId = @ParentAccountId)
                    BEGIN
                        RAISERROR('Parent account does not exist', 16, 1);
                        RETURN;
                    END
                    
                    -- Insert the new account
                    INSERT INTO ChartOfAccounts (AccountCode, AccountName, Description, ParentAccountId, AccountType, IsActive, CreatedDate)
                    VALUES (@AccountCode, @AccountName, @Description, @ParentAccountId, @AccountType, @IsActive, GETUTCDATE());
                    
                    -- Return the new account ID
                    SELECT SCOPE_IDENTITY() AS AccountId;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_UpdateChartOfAccount]
                    @AccountId INT,
                    @AccountCode NVARCHAR(20),
                    @AccountName NVARCHAR(100),
                    @Description NVARCHAR(500) = NULL,
                    @ParentAccountId INT = NULL,
                    @AccountType INT,
                    @IsActive BIT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    -- Check if account exists
                    IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountId = @AccountId)
                    BEGIN
                        RAISERROR('Account does not exist', 16, 1);
                        RETURN;
                    END
                    
                    -- Check if account code already exists for another account
                    IF EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountCode = @AccountCode AND AccountId <> @AccountId)
                    BEGIN
                        RAISERROR('Account code already exists for another account', 16, 1);
                        RETURN;
                    END
                    
                    -- Check if parent account exists if provided
                    IF @ParentAccountId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountId = @ParentAccountId)
                    BEGIN
                        RAISERROR('Parent account does not exist', 16, 1);
                        RETURN;
                    END
                    
                    -- Check for circular reference
                    IF @ParentAccountId = @AccountId
                    BEGIN
                        RAISERROR('Cannot set account as its own parent', 16, 1);
                        RETURN;
                    END
                    
                    -- Update the account
                    UPDATE ChartOfAccounts
                    SET AccountCode = @AccountCode,
                        AccountName = @AccountName,
                        Description = @Description,
                        ParentAccountId = @ParentAccountId,
                        AccountType = @AccountType,
                        IsActive = @IsActive,
                        LastModifiedDate = GETUTCDATE()
                    WHERE AccountId = @AccountId;
                    
                    -- Return success
                    SELECT @AccountId AS AccountId;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_GetChartOfAccounts]
                    @AccountType INT = NULL,
                    @IsActive BIT = NULL,
                    @ParentAccountId INT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT 
                        a.AccountId,
                        a.AccountCode,
                        a.AccountName,
                        a.Description,
                        a.ParentAccountId,
                        p.AccountName AS ParentAccountName,
                        a.AccountType,
                        a.IsActive,
                        a.CreatedDate,
                        a.LastModifiedDate,
                        (SELECT COUNT(1) FROM ChartOfAccounts WHERE ParentAccountId = a.AccountId) AS ChildCount
                    FROM 
                        ChartOfAccounts a
                    LEFT JOIN 
                        ChartOfAccounts p ON a.ParentAccountId = p.AccountId
                    WHERE 
                        (@AccountType IS NULL OR a.AccountType = @AccountType)
                        AND (@IsActive IS NULL OR a.IsActive = @IsActive)
                        AND (@ParentAccountId IS NULL OR a.ParentAccountId = @ParentAccountId OR (@ParentAccountId = 0 AND a.ParentAccountId IS NULL))
                    ORDER BY 
                        a.AccountCode;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[sp_GetChartOfAccountById]
                    @AccountId INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    
                    SELECT 
                        a.AccountId,
                        a.AccountCode,
                        a.AccountName,
                        a.Description,
                        a.ParentAccountId,
                        p.AccountName AS ParentAccountName,
                        a.AccountType,
                        a.IsActive,
                        a.CreatedDate,
                        a.LastModifiedDate,
                        (SELECT COUNT(1) FROM ChartOfAccounts WHERE ParentAccountId = a.AccountId) AS ChildCount    
                    FROM 
                        ChartOfAccounts a
                    LEFT JOIN 
                        ChartOfAccounts p ON a.ParentAccountId = p.AccountId
                    WHERE 
                        a.AccountId = @AccountId;
                END
            ");

            migrationBuilder.Sql(@"
            CREATE PROCEDURE [dbo].[sp_DeleteChartOfAccount]
                @AccountId INT,
                @ErrorMessage NVARCHAR(500) OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
                SET @ErrorMessage = NULL;

                -- Check if account exists
                IF NOT EXISTS (SELECT 1 FROM ChartOfAccounts WHERE AccountId = @AccountId)
                BEGIN
                    SET @ErrorMessage = 'Account does not exist';
                    RETURN 1;
                END

                -- Check if account has child accounts
                IF EXISTS (SELECT 1 FROM ChartOfAccounts WHERE ParentAccountId = @AccountId)
                BEGIN
                    SET @ErrorMessage = 'Cannot delete account with child accounts';
                    RETURN 1;
                END

                -- Delete the account
                DELETE FROM ChartOfAccounts WHERE AccountId = @AccountId;

                -- Return success
                RETURN 0;
            END
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop stored procedures
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_CreateChartOfAccount]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_UpdateChartOfAccount]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetChartOfAccounts]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_GetChartOfAccountById]");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [dbo].[sp_DeleteChartOfAccount]");

            // Drop table
            migrationBuilder.DropTable(
                name: "ChartOfAccounts");
        }
    }
}