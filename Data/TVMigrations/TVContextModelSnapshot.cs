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
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("CityID");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.City", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("Province")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("Province", "Name")
                        .IsUnique();

                    b.ToTable("Cities");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Director", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
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

                    b.Property<string>("Nickname")
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Directors");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.DirectorAvatar", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<int>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MimeType")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("DirectorID")
                        .IsUnique();

                    b.ToTable("DirectorAvatars");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.FileContent", b =>
                {
                    b.Property<int>("FileContentID")
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.HasKey("FileContentID");

                    b.ToTable("FileContent");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.Discussion", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ShiftID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ShiftID");

                    b.ToTable("Discussions");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.DiscussionVolunteer", b =>
                {
                    b.Property<int>("DiscussionID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VolunteerID")
                        .HasColumnType("INTEGER");

                    b.HasKey("DiscussionID", "VolunteerID");

                    b.ToTable("DiscussionVolunteers");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.Message", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("DiscussionID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("FromAccountID")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("VolunteerID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("DiscussionID");

                    b.HasIndex("VolunteerID");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Rehearsal", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ChapterID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("Note")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RehearsalDate")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TotalSingers")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.HasIndex("DirectorID", "RehearsalDate", "Status")
                        .IsUnique()
                        .HasFilter("[Status] = 0");

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

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
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
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("ChapterID");

                    b.HasIndex("FirstName", "LastName", "Email")
                        .IsUnique();

                    b.ToTable("Singers");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.UploadedFile", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<string>("MimeType")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("UploadedFiles");

                    b.HasDiscriminator().HasValue("UploadedFile");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Users.Role", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("VolunteerID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("DirectorID");

                    b.HasIndex("VolunteerID");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.CityEvent", b =>
                {
                    b.Property<int>("CityID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EventID")
                        .HasColumnType("INTEGER");

                    b.HasKey("CityID", "EventID");

                    b.HasIndex("EventID");

                    b.ToTable("CityEvents");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Event", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Descripion")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("Events");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Shift", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EndAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("StartAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("VolunteersNeeded")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("EventID");

                    b.ToTable("Shifts");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Volunteer", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CreatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
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

                    b.Property<string>("Nickname")
                        .HasColumnType("TEXT");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("BLOB");

                    b.Property<int>("Status")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UpdatedBy")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.ToTable("Volunteers");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.VolunteerAvatar", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte[]>("Content")
                        .HasColumnType("BLOB");

                    b.Property<string>("MimeType")
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.Property<int>("VolunteerID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("VolunteerID")
                        .IsUnique();

                    b.ToTable("VolunteerAvatars");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.VolunteerShift", b =>
                {
                    b.Property<int>("VolunteerID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ShiftID")
                        .HasColumnType("INTEGER");

                    b.HasKey("VolunteerID", "ShiftID");

                    b.HasIndex("ShiftID");

                    b.ToTable("VolunteerShifts");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.DirectorDocument", b =>
                {
                    b.HasBaseType("TomorrowsVoice_Toplevel.Models.UploadedFile");

                    b.Property<int>("DirectorID")
                        .HasColumnType("INTEGER");

                    b.HasIndex("DirectorID");

                    b.HasDiscriminator().HasValue("DirectorDocument");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Chapter", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.City", "City")
                        .WithMany()
                        .HasForeignKey("CityID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("City");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Director", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Chapter", "Chapter")
                        .WithMany("Directors")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chapter");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.DirectorAvatar", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Director", "Director")
                        .WithOne("Avatar")
                        .HasForeignKey("TomorrowsVoice_Toplevel.Models.DirectorAvatar", "DirectorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Director");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.FileContent", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.UploadedFile", "UploadedFile")
                        .WithOne("FileContent")
                        .HasForeignKey("TomorrowsVoice_Toplevel.Models.FileContent", "FileContentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UploadedFile");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.Discussion", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Shift", "Shift")
                        .WithMany()
                        .HasForeignKey("ShiftID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shift");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.DiscussionVolunteer", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Messaging.Discussion", null)
                        .WithMany("DiscussionVolunteers")
                        .HasForeignKey("DiscussionID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.Message", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Messaging.Discussion", null)
                        .WithMany("Messages")
                        .HasForeignKey("DiscussionID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Volunteer", "Volunteer")
                        .WithMany()
                        .HasForeignKey("VolunteerID");

                    b.Navigation("Volunteer");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Rehearsal", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Chapter", "Chapter")
                        .WithMany("Rehearsals")
                        .HasForeignKey("ChapterID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TomorrowsVoice_Toplevel.Models.Director", "Director")
                        .WithMany("Rehearsals")
                        .HasForeignKey("DirectorID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Chapter");

                    b.Navigation("Director");
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
                        .OnDelete(DeleteBehavior.Cascade)
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

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Users.Role", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Director", null)
                        .WithMany("Roles")
                        .HasForeignKey("DirectorID");

                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Volunteer", null)
                        .WithMany("Roles")
                        .HasForeignKey("VolunteerID");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.CityEvent", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.City", "City")
                        .WithMany("CityEvents")
                        .HasForeignKey("CityID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Event", "Event")
                        .WithMany("CityEvents")
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("City");

                    b.Navigation("Event");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Shift", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Event", "Event")
                        .WithMany("Shifts")
                        .HasForeignKey("EventID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.VolunteerAvatar", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Volunteer", "Volunteer")
                        .WithOne("Avatar")
                        .HasForeignKey("TomorrowsVoice_Toplevel.Models.Volunteering.VolunteerAvatar", "VolunteerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Volunteer");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.VolunteerShift", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Shift", "Shift")
                        .WithMany("VolunteerShifts")
                        .HasForeignKey("ShiftID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TomorrowsVoice_Toplevel.Models.Volunteering.Volunteer", "Volunteer")
                        .WithMany("VolunteerShifts")
                        .HasForeignKey("VolunteerID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Shift");

                    b.Navigation("Volunteer");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.DirectorDocument", b =>
                {
                    b.HasOne("TomorrowsVoice_Toplevel.Models.Director", "Director")
                        .WithMany("VulnerableSectorChecks")
                        .HasForeignKey("DirectorID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Director");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Chapter", b =>
                {
                    b.Navigation("Directors");

                    b.Navigation("Rehearsals");

                    b.Navigation("Singers");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.City", b =>
                {
                    b.Navigation("CityEvents");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Director", b =>
                {
                    b.Navigation("Avatar");

                    b.Navigation("Rehearsals");

                    b.Navigation("Roles");

                    b.Navigation("VulnerableSectorChecks");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Messaging.Discussion", b =>
                {
                    b.Navigation("DiscussionVolunteers");

                    b.Navigation("Messages");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Rehearsal", b =>
                {
                    b.Navigation("RehearsalAttendances");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Singer", b =>
                {
                    b.Navigation("RehearsalAttendances");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.UploadedFile", b =>
                {
                    b.Navigation("FileContent");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Event", b =>
                {
                    b.Navigation("CityEvents");

                    b.Navigation("Shifts");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Shift", b =>
                {
                    b.Navigation("VolunteerShifts");
                });

            modelBuilder.Entity("TomorrowsVoice_Toplevel.Models.Volunteering.Volunteer", b =>
                {
                    b.Navigation("Avatar");

                    b.Navigation("Roles");

                    b.Navigation("VolunteerShifts");
                });
#pragma warning restore 612, 618
        }
    }
}
