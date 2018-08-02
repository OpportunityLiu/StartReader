using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using StartReader.App.Model;

namespace StartReader.App.Migrations
{
    [DbContext(typeof(BookShelf))]
    partial class BookShelfModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
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

                    b.Property<string>("ExtensionId")
                        .IsRequired();

                    b.Property<bool>("IsFinished");

                    b.Property<string>("Key")
                        .IsRequired();

                    b.Property<string>("PackageFamilyName")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<int>("WordCount");

                    b.Property<string>("coverUri")
                        .HasColumnName("CoverUri");

                    b.HasKey("Id");

                    b.HasIndex("PackageFamilyName", "ExtensionId");

                    b.HasIndex("Title", "Author");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.Property<int>("BookId");

                    b.Property<int>("ChapterId");

                    b.Property<string>("Content");

                    b.Property<string>("Key")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<DateTime?>("UpdateTime");

                    b.Property<string>("VolumeTitle");

                    b.Property<int>("WordCount");

                    b.HasKey("BookId", "ChapterId");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.HasOne("StartReader.App.Model.Book", "Book")
                        .WithMany("ChaptersData")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
