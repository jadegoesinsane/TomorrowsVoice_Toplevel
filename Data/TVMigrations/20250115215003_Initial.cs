using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TomorrowsVoice_Toplevel.Data.TVMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "TEXT", nullable: false),
                    Postal = table.Column<string>(type: "TEXT", nullable: false),
                    DOW = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Rehearsal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ChapterID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rehearsal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rehearsal_Chapters_ChapterID",
                        column: x => x.ChapterID,
                        principalTable: "Chapters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_Name",
                table: "Chapters",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rehearsal_ChapterID",
                table: "Rehearsal",
                column: "ChapterID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Rehearsal");

            migrationBuilder.DropTable(
                name: "Chapters");
        }
    }
}
