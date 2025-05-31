using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeachTether.Infrastructure.Persistence.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClassAssignmentsTableChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassAssignments_ClassGroups_ClassGroupId",
                table: "ClassAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassAssignments_Subjects_SubjectId",
                table: "ClassAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassAssignments_ClassGroupId_SubjectId_TeacherId",
                table: "ClassAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassAssignments_SubjectId",
                table: "ClassAssignments");

            migrationBuilder.DropColumn(
                name: "ClassGroupId",
                table: "ClassAssignments");

            migrationBuilder.RenameColumn(
                name: "SubjectId",
                table: "ClassAssignments",
                newName: "ClassGroupSubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignments_ClassGroupSubjectId_TeacherId",
                table: "ClassAssignments",
                columns: ["ClassGroupSubjectId", "TeacherId"],
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassAssignments_ClassGroupSubjects_ClassGroupSubjectId",
                table: "ClassAssignments",
                column: "ClassGroupSubjectId",
                principalTable: "ClassGroupSubjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassAssignments_ClassGroupSubjects_ClassGroupSubjectId",
                table: "ClassAssignments");

            migrationBuilder.DropIndex(
                name: "IX_ClassAssignments_ClassGroupSubjectId_TeacherId",
                table: "ClassAssignments");

            migrationBuilder.RenameColumn(
                name: "ClassGroupSubjectId",
                table: "ClassAssignments",
                newName: "SubjectId");

            migrationBuilder.AddColumn<int>(
                name: "ClassGroupId",
                table: "ClassAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignments_ClassGroupId_SubjectId_TeacherId",
                table: "ClassAssignments",
                columns: ["ClassGroupId", "SubjectId", "TeacherId"],
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassAssignments_SubjectId",
                table: "ClassAssignments",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassAssignments_ClassGroups_ClassGroupId",
                table: "ClassAssignments",
                column: "ClassGroupId",
                principalTable: "ClassGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassAssignments_Subjects_SubjectId",
                table: "ClassAssignments",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
