using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace questionnaire.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkQuestionnaire : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "access_link_token",
                table: "Questionnaire",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "access_link_token",
                table: "Questionnaire");
        }
    }
}
