using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachTether.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class SmallIndexChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_Name",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_SchoolId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Schools_SchoolOwnerId",
                table: "Schools");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SchoolId_Name",
                table: "Subjects",
                columns: new[] { "SchoolId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolOwnerId_Name",
                table: "Schools",
                columns: new[] { "SchoolOwnerId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subjects_SchoolId_Name",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Schools_SchoolOwnerId_Name",
                table: "Schools");

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_Name",
                table: "Subjects",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_SchoolId",
                table: "Subjects",
                column: "SchoolId");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_SchoolOwnerId",
                table: "Schools",
                column: "SchoolOwnerId");
        }
    }
}
