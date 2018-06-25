using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using StartReader.App.Model;

namespace StartReader.App.Migrations
{
    [DbContext(typeof(BookShelf))]
    [Migration("20180625083858_Alpha1")]
    partial class Alpha1
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

                    b.Property<int>("CurrentSourceId");

                    b.Property<string>("Description");

                    b.Property<bool>("IsFinished");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<string>("coverUri")
                        .HasColumnName("CoverUri");

                    b.HasKey("Id");

                    b.HasIndex("Author");

                    b.HasIndex("CurrentSourceId");

                    b.HasIndex("Title");

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

                    b.Property<string>("PackageFamilyName")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.ToTable("BookSource");
                });

            modelBuilder.Entity("StartReader.App.Model.Chapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BookId");

                    b.Property<string>("Content")
                        .IsRequired();

                    b.Property<string>("Key")
                        .IsRequired();

                    b.Property<string>("Preview");

                    b.Property<int>("SourceId");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<DateTime?>("UpdateTime");

                    b.Property<int>("WordCount");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.HasIndex("SourceId");

                    b.ToTable("Chapter");
                });

            modelBuilder.Entity("StartReader.App.Model.Book", b =>
                {
                    b.HasOne("StartReader.App.Model.BookSource", "CurrentSource")
                        .WithMany()
                        .HasForeignKey("CurrentSourceId")
                        .OnDelete(DeleteBehavior.Cascade);
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
                        .WithMany("Chapters")
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
