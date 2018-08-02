using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StartReader.App.Migrations
{
    public partial class Alpha : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlternativeTitle = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: false),
                    CoverData = table.Column<byte[]>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    ExtensionId = table.Column<string>(nullable: false),
                    IsFinished = table.Column<bool>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    PackageFamilyName = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    WordCount = table.Column<int>(nullable: false),
                    CoverUri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    ChapterId = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: true),
                    VolumeTitle = table.Column<string>(nullable: true),
                    WordCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => new { x.BookId, x.ChapterId });
                    table.ForeignKey(
                        name: "FK_Chapters_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_PackageFamilyName_ExtensionId",
                table: "Books",
                columns: new[] { "PackageFamilyName", "ExtensionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title_Author",
                table: "Books",
                columns: new[] { "Title", "Author" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
