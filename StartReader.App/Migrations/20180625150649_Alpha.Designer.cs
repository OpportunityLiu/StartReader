using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using StartReader.App.Model;

namespace StartReader.App.Migrations
{
    [DbContext(typeof(BookShelf))]
    [Migration("20180625150649_Alpha")]
    partial class Alpha
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.5");

            modelBuilder.Entity("StartReader.App.Model.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AlternativeTitle");

                    b.Property<string>("Author")
                        .IsRequired();

                    b.Property<byte[]>("CoverData");

                    b.Property<string>("Description");

                    b.Property<bool>("IsFinished");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<int>("WordCount");

                    b.Property<string>("coverUri")
                        .HasColumnName("CoverUri");

                    b.HasKey("Id");

                    b.HasIndex("Title", "Author")
                        .IsUnique();

                    b.ToTable("Books");
                });

            modelBuilder.Entity("StartReader.App.Model.BookSource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookId");

                    b.Property<string>("BookKey")
                        .IsRequired();

                    b.Property<string>("ExtensionId")
                        .IsRequired();

                    b.Property<bool>("IsCurrent");

                    b.Property<string>("PackageFamilyName")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("BookKey");

                    b.HasIndex("ExtensionId", "PackageFamilyName");

                    b.ToTable("BookSources");
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.Property<int>("Index");

                    b.Property<int>("BookId");

                    b.Property<string>("Content");

                    b.Property<int>("SourceId");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<DateTime?>("UpdateTime");

                    b.Property<int>("WordCount");

                    b.HasKey("Index", "BookId");

                    b.HasIndex("BookId");

                    b.HasIndex("SourceId");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("StartReader.App.Model.BookSource", b =>
                {
                    b.HasOne("StartReader.App.Model.Book", "Book")
                        .WithMany("Sources")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.HasOne("StartReader.App.Model.Book", "Book")
                        .WithMany("ChaptersData")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("StartReader.App.Model.BookSource", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
