using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Crud.CrossCutting.Migrations
{
    /// <inheritdoc />
    public partial class AddedCategoryDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Text" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Life" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Science" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "History" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "General culture" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "Mathematics" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "Physics" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "Politics" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "Technology" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "Music" },
                    { new Guid("00000000-0000-0000-0000-00000000000a"), "Sports" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-00000000000a"));
        }
    }
}
