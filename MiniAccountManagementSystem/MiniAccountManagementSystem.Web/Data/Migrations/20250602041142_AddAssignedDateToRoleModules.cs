﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniAccountManagementSystem.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignedDateToRoleModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedDate",
                table: "RoleModules",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedDate",
                table: "RoleModules");
        }
    }
}
