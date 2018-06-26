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
                    IsFinished = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    WordCount = table.Column<int>(nullable: false),
                    CoverUri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BookSources",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(nullable: false),
                    BookKey = table.Column<string>(nullable: false),
                    ExtensionId = table.Column<string>(nullable: false),
                    IsCurrent = table.Column<bool>(nullable: false),
                    PackageFamilyName = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookSources_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    BookId = table.Column<int>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    Content = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    SourceId = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    UpdateTime = table.Column<DateTime>(nullable: true),
                    WordCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => new { x.BookId, x.Index });
                    table.ForeignKey(
                        name: "FK_Chapters_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chapters_BookSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "BookSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title_Author",
                table: "Books",
                columns: new[] { "Title", "Author" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookSources_BookId",
                table: "BookSources",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_BookSources_BookKey",
                table: "BookSources",
                column: "BookKey");

            migrationBuilder.CreateIndex(
                name: "IX_BookSources_PackageFamilyName_ExtensionId",
                table: "BookSources",
                columns: new[] { "PackageFamilyName", "ExtensionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_SourceId",
                table: "Chapters",
                column: "SourceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "BookSources");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
