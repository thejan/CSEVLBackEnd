using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CSEVirtualLabDataAccessLayer.Models;

public partial class AtmecsevlabContext : DbContext
{
    public AtmecsevlabContext(DbContextOptions<AtmecsevlabContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Experiment> Experiments { get; set; }

    public virtual DbSet<Lab> Labs { get; set; }

    public virtual DbSet<LabEnrollment> LabEnrollments { get; set; }

    public virtual DbSet<LabStatus> LabStatuses { get; set; }

    public virtual DbSet<Registration> Registrations { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<UserActivitySession> UserActivitySessions { get; set; }

    public virtual DbSet<UserRequest> UserRequests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Experiment>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Experiments_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Experiments_IsActive");

            entity.HasOne(d => d.Lab).WithMany(p => p.Experiments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Experiments_Labs");
        });

        modelBuilder.Entity<Lab>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Labs_CreatedAt");
            entity.Property(e => e.IsActive).HasDefaultValue(true, "DF_Labs_IsActive");
        });

        modelBuilder.Entity<LabEnrollment>(entity =>
        {
            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(sysutcdatetime())", "DF_LabEnrollment_EnrolledAt");
            entity.Property(e => e.EnrollmentStatus).HasDefaultValue("Enrolled", "DF_LabEnrollment_Status");

            entity.HasOne(d => d.Lab).WithMany(p => p.LabEnrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LabEnrollment_Labs");

            entity.HasOne(d => d.User).WithMany(p => p.LabEnrollments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LabEnrollment_Registration");
        });

        modelBuilder.Entity<LabStatus>(entity =>
        {
            entity.Property(e => e.CompletionStatus).HasComputedColumnSql("(case when [Theory]=(1) AND [Execution]=(1) AND [Quiz]=(1) AND [Assignment1]=(1) AND [Assignment2]=(1) AND [Assignment3]=(1) AND [Assignment4]=(1) AND [Assignment5]=(1) then 'Completed' else 'Not Completed' end)", true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_LabStatus_CreatedAt");

            entity.HasOne(d => d.User).WithMany(p => p.LabStatuses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LabStatus_Registration");

            entity.HasOne(d => d.Experiment).WithMany(p => p.LabStatuses)
                .HasPrincipalKey(p => new { p.ExperimentId, p.LabId })
                .HasForeignKey(d => new { d.ExperimentId, d.LabId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LabStatus_Experiment_Lab");
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())", "DF_Registration_CreatedAt");
            entity.Property(e => e.RegistrationStatus).HasDefaultValue("Pending", "DF_Registration_Status");
            entity.Property(e => e.UserType).HasDefaultValue("Student", "DF_Registration_UserType");
            entity.HasIndex(e => e.Usn, "UQ_Registration_USN")
                .IsUnique()
                .HasFilter("[USN] IS NOT NULL");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.InverseApprovedByNavigation).HasConstraintName("FK_Registration_ApprovedBy");

            entity.HasOne(d => d.Role).WithMany(p => p.Registrations)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registration_Roles");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql(
                    "(sysutcdatetime())",
                    "DF_SystemSettings_UpdatedAt");
        });

        modelBuilder.Entity<UserActivitySession>(entity =>
        {
            entity.Property(e => e.IsSessionOpen).HasDefaultValue(true, "DF_UserActivity_IsSessionOpen");
            entity.Property(e => e.LastActivityTime).HasDefaultValueSql("(sysutcdatetime())", "DF_UserActivity_LastActivity");
            entity.Property(e => e.LastHeartbeatTime).HasDefaultValueSql("(sysutcdatetime())", "DF_UserActivity_LastHeartbeat");
            entity.Property(e => e.LoginTime).HasDefaultValueSql("(sysutcdatetime())", "DF_UserActivity_LoginTime");

            entity.HasOne(d => d.User).WithMany(p => p.UserActivitySessions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserActivitySessions_Registration");
        });

        modelBuilder.Entity<UserRequest>(entity =>
        {
            entity.Property(e => e.Status)
                .HasDefaultValue("Open", "DF_UserRequests_Status");

            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql(
                    "(sysutcdatetime())",
                    "DF_UserRequests_SubmittedAt");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserRequests)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserRequests_Registration");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
