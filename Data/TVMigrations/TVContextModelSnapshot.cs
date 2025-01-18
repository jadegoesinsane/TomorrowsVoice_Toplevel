﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TomorrowsVoice_Toplevel.Data;

#nullable disable

namespace TomorrowsVoice_Toplevel.Data.TVMigrations
{
    [DbContext(typeof(TVContext))]
    partial class TVContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.11");

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Chapter", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("DOW")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Postal")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Director", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID")
                        .IsUnique();

                    b.ToTable("Director");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Rehearsal", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.ToTable("Rehearsals");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.RehearsalAttendance", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("RehearsalID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SingerID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("RehearsalID");

                    b.HasIndex("SingerID");

                    b.ToTable("RehearsalAttendances");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Singer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContactName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.ToTable("Singers");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Director", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Chapter", "Chapter")
                        .WithOne("Director")
                        .HasForeignKey("TomorrowsVoice_Toplevel.Models.Director", "ChapterID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Rehearsal", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Chapter", "Chapter")
                        .WithMany("Rehearsals")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.RehearsalAttendance", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Rehearsal", "Rehearsal")
                        .WithMany("RehearsalAttendances")
                        .HasForeignKey("RehearsalID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TomorrowsVoice_Toplevel.Models.Singer", "Singer")
                        .WithMany("RehearsalAttendances")
                        .HasForeignKey("SingerID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Rehearsal");

                    b.Navigation("Singer");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Singer", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Chapter", "Chapter")
                        .WithMany("Singers")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Chapter", b =>
                {
                    b.Navigation("Director");

                    b.Navigation("Rehearsals");

                    b.Navigation("Singers");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Rehearsal", b =>
                {
                    b.Navigation("RehearsalAttendances");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Singer", b =>
                {
                    b.Navigation("RehearsalAttendances");
                });
#pragma warning restore 612, 618
        }
    }
}
