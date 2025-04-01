using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachTether.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class SchoolForeignKeyNameChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolOwners_OwnerUserId",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "Schools",
                newName: "SchoolOwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Schools_OwnerUserId",
                table: "Schools",
                newName: "IX_Schools_SchoolOwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolOwners_SchoolOwnerId",
                table: "Schools",
                column: "SchoolOwnerId",
                principalTable: "SchoolOwners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schools_SchoolOwners_SchoolOwnerId",
                table: "Schools");

            migrationBuilder.RenameColumn(
                name: "SchoolOwnerId",
                table: "Schools",
                newName: "OwnerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Schools_SchoolOwnerId",
                table: "Schools",
                newName: "IX_Schools_OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schools_SchoolOwners_OwnerUserId",
                table: "Schools",
                column: "OwnerUserId",
                principalTable: "SchoolOwners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
