using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StartReader.App.Migrations
{
    public partial class TestMig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BookSource",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(nullable: true),
                    BookKey = table.Column<string>(nullable: true),
                    ProviderId = table.Column<string>(nullable: true),
                    SourcePFN = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AlternativeTitle = table.Column<string>(nullable: true),
                    Author = table.Column<string>(nullable: true),
                    CurrentSourceId = table.Column<int>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Books_BookSource_CurrentSourceId",
                        column: x => x.CurrentSourceId,
                        principalTable: "BookSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Chapter",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BookId = table.Column<int>(nullable: true),
                    BookKey = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: true),
                    Key = table.Column<string>(nullable: true),
                    Preview = table.Column<string>(nullable: true),
                    SourceId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chapter_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chapter_BookSource_SourceId",
                        column: x => x.SourceId,
                        principalTable: "BookSource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Books_Author",
                table: "Books",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_Books_CurrentSourceId",
                table: "Books",
                column: "CurrentSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_Title",
                table: "Books",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_BookSource_BookId",
                table: "BookSource",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapter_BookId",
                table: "Chapter",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Chapter_SourceId",
                table: "Chapter",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_BookSource_Books_BookId",
                table: "BookSource",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_BookSource_CurrentSourceId",
                table: "Books");

            migrationBuilder.DropTable(
                name: "Chapter");

            migrationBuilder.DropTable(
                name: "BookSource");

            migrationBuilder.DropTable(
                name: "Books");
        }
    }
}
