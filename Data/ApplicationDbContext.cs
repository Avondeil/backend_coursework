using System;
using System.Collections.Generic;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Bodytype> Bodytypes { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Generation> Generations { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserHistory> UserHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=EKBDetal_Base;Username=postgres;Password=12344321");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bodytype>(entity =>
        {
            entity.HasKey(e => e.Bodytypeid).HasName("bodytypes_pkey");

            entity.ToTable("bodytypes");

            entity.Property(e => e.Bodytypeid).HasColumnName("bodytypeid");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.GenerationId).HasColumnName("generation_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Brand).WithMany(p => p.Bodytypes)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("bodytypes_brand_id_fkey");

            entity.HasOne(d => d.Generation).WithMany(p => p.Bodytypes)
                .HasForeignKey(d => d.GenerationId)
                .HasConstraintName("bodytypes_generation_id_fkey");

            entity.HasOne(d => d.Model).WithMany(p => p.Bodytypes)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("bodytypes_model_id_fkey");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("brands_pkey");

            entity.ToTable("brands");

            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Generation>(entity =>
        {
            entity.HasKey(e => e.GenerationId).HasName("generations_pkey");

            entity.ToTable("generations");

            entity.Property(e => e.GenerationId).HasColumnName("generation_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Model).WithMany(p => p.Generations)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("generations_model_id_fkey");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.ModelId).HasName("models_pkey");

            entity.ToTable("models");

            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Brand).WithMany(p => p.Models)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("models_brand_id_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("orders_user_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderItemId).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.Property(e => e.OrderItemId).HasColumnName("order_item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_items_order_id_fkey");

            entity.HasOne(d => d.Part).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.PartId)
                .HasConstraintName("order_items_part_id_fkey");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("parts_pkey");

            entity.ToTable("parts");

            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.BodyType)
                .HasMaxLength(50)
                .HasColumnName("body_type");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GenerationId).HasColumnName("generation_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.ModelYear).HasColumnName("model_year");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0)
                .HasColumnName("stock_quantity");

            entity.HasOne(d => d.Brand).WithMany(p => p.Parts)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("parts_brand_id_fkey");

            entity.HasOne(d => d.Generation).WithMany(p => p.Parts)
                .HasForeignKey(d => d.GenerationId)
                .HasConstraintName("parts_generation_id_fkey");

            entity.HasOne(d => d.Model).WithMany(p => p.Parts)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("parts_model_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(255)
                .HasColumnName("full_name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<UserHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("user_history_pkey");

            entity.ToTable("user_history");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.PurchaseDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("purchase_date");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Part).WithMany(p => p.UserHistories)
                .HasForeignKey(d => d.PartId)
                .HasConstraintName("user_history_part_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("user_history_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
