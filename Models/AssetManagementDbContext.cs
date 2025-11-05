using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.API.Models;

public partial class AssetManagementDbContext : DbContext
{
    public AssetManagementDbContext()
    {
    }

    public AssetManagementDbContext(DbContextOptions<AssetManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<MstDepartment> MstDepartments { get; set; }

    public virtual DbSet<MstUser> MstUsers { get; set; }

    public virtual DbSet<TrxAsset> TrxAssets { get; set; }

    public virtual DbSet<TrxAssetApproval> TrxAssetApprovals { get; set; }

    public virtual DbSet<TrxMaintenanceRecord> TrxMaintenanceRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=GapTechIng;Database=AssetManagement;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MstDepartment>(entity =>
        {
            entity.ToTable("mstDepartment");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DepartmentName)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<MstUser>(entity =>
        {
            entity.ToTable("mstUser");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DepartmentId).HasColumnName("DepartmentID");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.SupervisorId).HasColumnName("SupervisorID");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TrxAsset>(entity =>
        {
            entity.ToTable("trxAsset");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ID");
            entity.Property(e => e.AssetName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AssetValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Category)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.RequesterId).HasColumnName("RequesterID");
            entity.Property(e => e.ResponsiblePersonId).HasColumnName("ResponsiblePersonID");
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Subcategory)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TrxAssetApproval>(entity =>
        {
            entity.ToTable("trxAssetApproval");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ApprovalDate).HasColumnType("datetime");
            entity.Property(e => e.ApproverId).HasColumnName("ApproverID");
            entity.Property(e => e.AssetId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("AssetID");
            entity.Property(e => e.Comments)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TrxMaintenanceRecord>(entity =>
        {
            entity.ToTable("trxMaintenanceRecord");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Comments).HasMaxLength(1000);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.LinkedAssetId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("LinkedAssetID");
            entity.Property(e => e.MaintenanceCost).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.MaintenanceDate).HasColumnType("datetime");
            entity.Property(e => e.MaintenanceType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            entity.Property(e => e.Vendor)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
