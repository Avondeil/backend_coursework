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

    public virtual DbSet<AutoboxParameter> AutoboxParameters { get; set; }

    public virtual DbSet<BodyType> BodyTypes { get; set; }

    public virtual DbSet<BodytypesCar> BodytypesCars { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Generation> Generations { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<RoofRackParameter> RoofRackParameters { get; set; }

    public virtual DbSet<SparePartsParameter> SparePartsParameters { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserHistory> UserHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=EKBDetal_Base;Username=postgres;Password=12344321");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AutoboxParameter>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("autobox_parameters_pkey");

            entity.ToTable("autobox_parameters");

            entity.Property(e => e.PartId)
                .ValueGeneratedNever()
                .HasColumnName("part_id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(255)
                .HasColumnName("country_of_origin");
            entity.Property(e => e.DimensionsMm)
                .HasMaxLength(255)
                .HasColumnName("dimensions_mm");
            entity.Property(e => e.LoadKg).HasColumnName("load_kg");
            entity.Property(e => e.OpeningSystem)
                .HasMaxLength(255)
                .HasColumnName("opening_system");
            entity.Property(e => e.VolumeL).HasColumnName("volume_l");

            entity.HasOne(d => d.Part).WithOne(p => p.AutoboxParameter)
                .HasForeignKey<AutoboxParameter>(d => d.PartId)
                .HasConstraintName("autobox_parameters_part_id_fkey");
        });

        modelBuilder.Entity<BodyType>(entity =>
        {
            entity.HasKey(e => e.BodyTypeId).HasName("body_types_pkey");

            entity.ToTable("body_types");

            entity.Property(e => e.BodyTypeId).HasColumnName("body_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<BodytypesCar>(entity =>
        {
            entity.HasKey(e => e.Bodytypeid).HasName("bodytypes_pkey");

            entity.ToTable("bodytypes_car");

            entity.Property(e => e.Bodytypeid)
                .HasDefaultValueSql("nextval('bodytypes_bodytypeid_seq'::regclass)")
                .HasColumnName("bodytypeid");
            entity.Property(e => e.BodyTypeId).HasColumnName("body_type_id");
            entity.Property(e => e.GenerationId).HasColumnName("generation_id");

            entity.HasOne(d => d.BodyType).WithMany(p => p.BodytypesCars)
                .HasForeignKey(d => d.BodyTypeId)
                .HasConstraintName("bodytypes_body_type_id_fkey");

            entity.HasOne(d => d.Generation).WithMany(p => p.BodytypesCars)
                .HasForeignKey(d => d.GenerationId)
                .HasConstraintName("bodytypes_generation_id_fkey");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("brands_pkey");

            entity.ToTable("brands");

            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Generation>(entity =>
        {
            entity.HasKey(e => e.GenerationId).HasName("generations_pkey");

            entity.ToTable("generations");

            entity.Property(e => e.GenerationId).HasColumnName("generation_id");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Year)
                .HasMaxLength(255)
                .HasColumnName("year");

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
                .HasMaxLength(255)
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
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("imageURL");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0)
                .HasColumnName("stock_quantity");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Parts)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("parts_product_type_id_fkey");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.ProductTypeId).HasName("product_types_pkey");

            entity.ToTable("product_types");

            entity.Property(e => e.ProductTypeId).HasColumnName("product_type_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<RoofRackParameter>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("roof_rack_parameters_pkey");

            entity.ToTable("roof_rack_parameters");

            entity.Property(e => e.PartId)
                .ValueGeneratedNever()
                .HasColumnName("part_id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(255)
                .HasColumnName("country_of_origin");
            entity.Property(e => e.CrossbarShape)
                .HasMaxLength(255)
                .HasColumnName("crossbar_shape");
            entity.Property(e => e.LengthCm).HasColumnName("length_cm");
            entity.Property(e => e.LoadKg).HasColumnName("load_kg");
            entity.Property(e => e.Material)
                .HasMaxLength(255)
                .HasColumnName("material");
            entity.Property(e => e.MountingType)
                .HasMaxLength(255)
                .HasColumnName("mounting_type");

            entity.HasOne(d => d.Part).WithOne(p => p.RoofRackParameter)
                .HasForeignKey<RoofRackParameter>(d => d.PartId)
                .HasConstraintName("roof_rack_parameters_part_id_fkey");
        });

        modelBuilder.Entity<SparePartsParameter>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("spare_parts_parameters_pkey");

            entity.ToTable("spare_parts_parameters");

            entity.Property(e => e.PartId)
                .ValueGeneratedNever()
                .HasColumnName("part_id");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.CountryOfOrigin)
                .HasMaxLength(255)
                .HasColumnName("country_of_origin");

            entity.HasOne(d => d.Part).WithOne(p => p.SparePartsParameter)
                .HasForeignKey<SparePartsParameter>(d => d.PartId)
                .HasConstraintName("spare_parts_parameters_part_id_fkey");
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
