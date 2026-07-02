using Microsoft.EntityFrameworkCore;
using KursMVVM.Models;

namespace KursMVVM.Models;

public class KursContext : DbContext
{
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Supply> Supplies => Set<Supply>();
    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=kurs.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Сотрудники");
            entity.Property(e => e.Id).HasColumnName("код_сотрудника").ValueGeneratedOnAdd();
            entity.Property(e => e.DepartmentCode).HasColumnName("код_отдела").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("фио").IsRequired();
            entity.Property(e => e.Position).HasColumnName("должность").IsRequired();
            entity.Property(e => e.Salary).HasColumnName("оклад").HasColumnType("REAL").IsRequired();
            entity.Property(e => e.Bonus).HasColumnName("премия").HasColumnType("REAL").IsRequired();
            entity.Property(e => e.Month).HasColumnName("месяц").IsRequired();
            entity.HasIndex(e => e.DepartmentCode).HasDatabaseName("idx_сотрудники_отдел");
            entity.HasIndex(e => e.FullName).HasDatabaseName("idx_сотрудники_фио");
        });

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Договоры");
            entity.HasKey(e => e.ContractNumber);
            entity.Property(e => e.ContractNumber).HasColumnName("номер_договора");
            entity.Property(e => e.EmployeeId).HasColumnName("код_сотрудника").IsRequired();
            entity.Property(e => e.OrganizationName).HasColumnName("наименование_организации").IsRequired();
            entity.Property(e => e.Date).HasColumnName("дата_заключения").IsRequired();
            entity.HasIndex(e => e.EmployeeId).HasDatabaseName("idx_договоры_сотрудник");
            entity.HasIndex(e => e.Date).HasDatabaseName("idx_договоры_дата");
            entity.HasOne(e => e.Employee)
                  .WithMany(e => e.Contracts)
                  .HasForeignKey(e => e.EmployeeId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supply>(entity =>
        {
            entity.ToTable("Поставки");
            entity.HasKey(e => e.ContractNumber);
            entity.Property(e => e.ContractNumber).HasColumnName("номер_договора");
            entity.Property(e => e.EquipmentType).HasColumnName("тип_оборудования").IsRequired();
            entity.Property(e => e.Comment).HasColumnName("комментарий");
            entity.Property(e => e.EmployeeId).HasColumnName("код_сотрудника").IsRequired();
            entity.HasIndex(e => e.EquipmentType).HasDatabaseName("idx_поставки_тип");
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
            entity.ToTable("Организации");
            entity.Property(e => e.Id).HasColumnName("id_организации").ValueGeneratedOnAdd();
            entity.Property(e => e.ContractNumber).HasColumnName("номер_договора").IsRequired();
            entity.Property(e => e.CountryCode).HasColumnName("код_страны").IsRequired();
            entity.Property(e => e.City).HasColumnName("город").IsRequired();
            entity.Property(e => e.Address).HasColumnName("адрес").IsRequired();
            entity.Property(e => e.Phone).HasColumnName("телефон").IsRequired();
            entity.Property(e => e.Email).HasColumnName("e_mail").IsRequired();
            entity.Property(e => e.Website).HasColumnName("web_site");
            entity.HasIndex(e => e.ContractNumber).HasDatabaseName("idx_орг_договор");
            entity.HasIndex(e => e.CountryCode).HasDatabaseName("idx_орг_страна");
            entity.HasOne(e => e.Supply)
                  .WithMany(s => s.Organizations)
                  .HasForeignKey(e => e.ContractNumber)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}