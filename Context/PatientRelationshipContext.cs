using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ProtoApp.Context.Models;

#nullable disable

namespace ProtoApp.Context
{
    public partial class PatientRelationshipContext : DbContext
    {
        public PatientRelationshipContext(DbContextOptions<PatientRelationshipContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationProgram> OrganizationPrograms { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<Practitioner> Practitioners { get; set; }
        public virtual DbSet<Relationship> Relationships { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");

                entity.HasIndex(e => e.Id, "Address_Id_uindex")
                    .IsUnique();

                entity.Property(e => e.AddressLine1)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.AddressLine2)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.State)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Zip)
                    .HasMaxLength(300)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Contact>(entity =>
            {
                entity.ToTable("Contact");

                entity.HasIndex(e => e.Id, "Contact_Id_uindex")
                    .IsUnique();

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Email)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(300)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Organization>(entity =>
            {
                entity.ToTable("Organization");

                entity.HasIndex(e => e.Id, "Organization_Id_uindex")
                    .IsUnique();

                entity.Property(e => e.AssigningAuthorityCode)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ParticipantName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.ParticipantSourceCode)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("Organization_Address_Id_fk");

                entity.HasOne(d => d.Contact)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.ContactId)
                    .HasConstraintName("Organization_Contact_Id_fk");

                entity.HasOne(d => d.Relationship)
                    .WithMany(p => p.Organizations)
                    .HasForeignKey(d => d.RelationshipId)
                    .HasConstraintName("Organization_Relationship_Id_fk");
            });

            modelBuilder.Entity<OrganizationProgram>(entity =>
            {
                entity.ToTable("OrganizationProgram");

                entity.HasIndex(e => e.Id, "Program_Id_uindex")
                    .IsUnique();

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Description)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.OrganizationPrograms)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("Program_Organization_Id_fk");
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patient");

                entity.HasIndex(e => e.Id, "Patient_Id_uindex")
                    .IsUnique();

                entity.HasIndex(e => new { e.Source, e.Mrn }, "Patient_pk")
                    .IsUnique();

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.Eid)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Mrn)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Source)
                    .HasMaxLength(300)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Practitioner>(entity =>
            {
                entity.ToTable("Practitioner");

                entity.HasIndex(e => e.Id, "Practitioner_Id_uindex")
                    .IsUnique();

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DisplayName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.License)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.HasOne(d => d.Address)
                    .WithMany(p => p.Practitioners)
                    .HasForeignKey(d => d.AddressId)
                    .HasConstraintName("Practitioner_Address_Id_fk");

                entity.HasOne(d => d.Contact)
                    .WithMany(p => p.Practitioners)
                    .HasForeignKey(d => d.ContactId)
                    .HasConstraintName("Practitioner_Contact_Id_fk");

                entity.HasOne(d => d.Organization)
                    .WithMany(p => p.Practitioners)
                    .HasForeignKey(d => d.OrganizationId)
                    .HasConstraintName("Practitioner_Organization_Id_fk");

                entity.HasOne(d => d.Relationship)
                    .WithMany(p => p.Practitioners)
                    .HasForeignKey(d => d.RelationshipId)
                    .HasConstraintName("Practitioner_Relationship_Id_fk");
            });

            modelBuilder.Entity<Relationship>(entity =>
            {
                entity.ToTable("Relationship");

                entity.HasIndex(e => e.Id, "Relationship_Id_uindex")
                    .IsUnique();

                entity.Property(e => e.DataSource)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.DateUpdated).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Patient)
                    .WithMany(p => p.Relationships)
                    .HasForeignKey(d => d.PatientId)
                    .HasConstraintName("Relationship_Patient_Id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}