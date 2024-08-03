using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Migrations
{
    /// <inheritdoc />
    public partial class AddedFieldToEntityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Entities",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Entities");
        }
    }
}
