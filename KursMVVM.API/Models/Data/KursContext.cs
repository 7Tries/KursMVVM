using Microsoft.EntityFrameworkCore;
using KursMVVM.API.Models;

namespace KursMVVM.API.Models.Data;

public class KursContext : DbContext
{
    public KursContext(DbContextOptions<KursContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("СОТРУДНИКИ");
            entity.Property(e => e.Id).HasColumnName("КОД_СОТРУДНИКА").ValueGeneratedOnAdd();
            entity.Property(e => e.DepartmentCode).HasColumnName("КОД_ОТДЕЛА").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("ФИО").IsRequired();
            entity.Property(e => e.Position).HasColumnName("ДОЛЖНОСТЬ").IsRequired();
            entity.Property(e => e.Salary).HasColumnName("ОКЛАД").HasColumnType("REAL").IsRequired();
            entity.Property(e => e.Bonus).HasColumnName("НАДБАВКА").HasColumnType("REAL").IsRequired();
            entity.Property(e => e.Month).HasColumnName("МЕСЯЦ").IsRequired();
            entity.HasIndex(e => e.DepartmentCode).HasDatabaseName("idx_СОТРУДНИКИ_ОТДЕЛ");
            entity.HasIndex(e => e.FullName).HasDatabaseName("idx_СОТРУДНИКИ_ФИО");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("ДОГОВОРА");
            entity.HasKey(e => e.ContractNumber);
            entity.Property(e => e.ContractNumber).HasColumnName("НОМЕР_ДОГОВОРА");
            entity.Property(e => e.EmployeeId).HasColumnName("КОД_СОТРУДНИКА").IsRequired();
            entity.Property(e => e.OrganizationName).HasColumnName("НАЗВАНИЕ_ОРГАНИЗАЦИИ").IsRequired();
            entity.Property(e => e.Date).HasColumnName("ДАТА_ДОГОВОРА").IsRequired();
            entity.HasIndex(e => e.EmployeeId).HasDatabaseName("idx_ДОГОВОРА_СОТРУДНИКА");
            entity.HasIndex(e => e.Date).HasDatabaseName("idx_ДОГОВОРА_ДАТА");
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Contracts)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supply>(entity =>
        {
            entity.ToTable("ПОСТАВКИ");
            entity.HasKey(e => e.ContractNumber);
            entity.Property(e => e.ContractNumber).HasColumnName("НОМЕР_ДОГОВОРА");
            entity.Property(e => e.EquipmentType).HasColumnName("ТИП_ОБОРУДОВАНИЯ").IsRequired();
            entity.Property(e => e.Comment).HasColumnName("КОММЕНТАРИЙ");
            entity.Property(e => e.EmployeeId).HasColumnName("КОД_СОТРУДНИКА").IsRequired();
            entity.HasIndex(e => e.EquipmentType).HasDatabaseName("idx_ПОСТАВКИ_ТИП");
            entity.HasOne(e => e.Contract)
                  .WithOne(c => c.Supply)
                  .HasForeignKey<Supply>(e => e.ContractNumber)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Supplies)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("ОРГАНИЗАЦИИ");
            entity.Property(e => e.Id).HasColumnName("id_организации").ValueGeneratedOnAdd();
            entity.Property(e => e.ContractNumber).HasColumnName("НОМЕР_ДОГОВОРА").IsRequired();
            entity.Property(e => e.CountryCode).HasColumnName("КОД_СТРАНЫ").IsRequired();
            entity.Property(e => e.City).HasColumnName("ГОРОД").IsRequired();
            entity.Property(e => e.Address).HasColumnName("АДРЕС").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("ТЕЛЕФОН").IsRequired();
            entity.Property(e => e.Email).HasColumnName("e_mail").IsRequired();
            entity.Property(e => e.Website).HasColumnName("web_site");
            entity.HasIndex(e => e.ContractNumber).HasDatabaseName("idx_ОРГ_ДОГОВОР");
            entity.HasIndex(e => e.CountryCode).HasDatabaseName("idx_ОРГ_СТРАНА");
            entity.HasOne(e => e.Supply)
                  .WithMany(s => s.Organizations)
                  .HasForeignKey(e => e.ContractNumber)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
