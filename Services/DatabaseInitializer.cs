using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace KursMVVM.Services;

public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var db = new KursContext();
        db.Database.EnsureCreated();

        if (db.Employees.Any()) return;

        db.Employees.AddRange(
            new Employee { DepartmentCode = 101, FullName = "Иванов Иван Иванович", Position = "Менеджер", Salary = 60000, Bonus = 10000, Month = "2025-01" },
            new Employee { DepartmentCode = 102, FullName = "Петров Петр Петрович", Position = "Разработчик", Salary = 80000, Bonus = 15000, Month = "2025-01" },
            new Employee { DepartmentCode = 103, FullName = "Сидоров Сидор Сидорович", Position = "Аналитик", Salary = 70000, Bonus = 12000, Month = "2025-02" },
            new Employee { DepartmentCode = 104, FullName = "Кузнецова Анна Михайловна", Position = "Бухгалтер", Salary = 55000, Bonus = 8000, Month = "2025-02" },
            new Employee { DepartmentCode = 105, FullName = "Смирнов Дмитрий Алексеевич", Position = "Тестировщик", Salary = 65000, Bonus = 10000, Month = "2025-03" },
            new Employee { DepartmentCode = 106, FullName = "Васильева Елена Сергеевна", Position = "HR-менеджер", Salary = 50000, Bonus = 5000, Month = "2025-03" },
            new Employee { DepartmentCode = 107, FullName = "Новиков Алексей Викторович", Position = "DevOps", Salary = 90000, Bonus = 20000, Month = "2025-04" },
            new Employee { DepartmentCode = 108, FullName = "Федорова Мария Игоревна", Position = "Дизайнер", Salary = 58000, Bonus = 7000, Month = "2025-04" },
            new Employee { DepartmentCode = 109, FullName = "Морозов Сергей Павлович", Position = "Системный администратор", Salary = 75000, Bonus = 11000, Month = "2025-05" },
            new Employee { DepartmentCode = 110, FullName = "Волкова Ольга Дмитриевна", Position = "Маркетолог", Salary = 62000, Bonus = 9000, Month = "2025-05" },
            new Employee { DepartmentCode = 111, FullName = "Лебедев Артём Николаевич", Position = "Backend-разработчик", Salary = 85000, Bonus = 18000, Month = "2025-06" },
            new Employee { DepartmentCode = 112, FullName = "Соколова Наталья Владимировна", Position = "Frontend-разработчик", Salary = 78000, Bonus = 14000, Month = "2025-06" },
            new Employee { DepartmentCode = 113, FullName = "Попов Игорь Семёнович", Position = "Data Scientist", Salary = 95000, Bonus = 25000, Month = "2025-07" },
            new Employee { DepartmentCode = 114, FullName = "Козлова Татьяна Андреевна", Position = "Product Owner", Salary = 88000, Bonus = 16000, Month = "2025-07" },
            new Employee { DepartmentCode = 115, FullName = "Соловьёв Владимир Олегович", Position = "QA Lead", Salary = 72000, Bonus = 12000, Month = "2025-08" }
        );
        db.SaveChanges();

        db.Contracts.AddRange(
            new Contract { ContractNumber = "Д001", EmployeeId = 1, OrganizationName = "ООО Ромашка", Date = new DateTime(2025, 1, 10) },
            new Contract { ContractNumber = "Д002", EmployeeId = 2, OrganizationName = "ООО Василёк", Date = new DateTime(2025, 1, 15) },
            new Contract { ContractNumber = "Д003", EmployeeId = 3, OrganizationName = "ИП Сидоров", Date = new DateTime(2025, 2, 5) },
            new Contract { ContractNumber = "Д004", EmployeeId = 4, OrganizationName = "ООО Лютик", Date = new DateTime(2025, 2, 20) },
            new Contract { ContractNumber = "Д005", EmployeeId = 5, OrganizationName = "ООО Ромашка", Date = new DateTime(2025, 3, 1) },
            new Contract { ContractNumber = "Д006", EmployeeId = 1, OrganizationName = "ООО Тюльпан", Date = new DateTime(2025, 3, 10) },
            new Contract { ContractNumber = "Д007", EmployeeId = 7, OrganizationName = "ООО Новые технологии", Date = new DateTime(2025, 4, 12) },
            new Contract { ContractNumber = "Д008", EmployeeId = 8, OrganizationName = "ООО Креатив", Date = new DateTime(2025, 4, 25) },
            new Contract { ContractNumber = "Д009", EmployeeId = 9, OrganizationName = "ООО Инфраструктура", Date = new DateTime(2025, 5, 8) },
            new Contract { ContractNumber = "Д010", EmployeeId = 10, OrganizationName = "ООО Продвижение", Date = new DateTime(2025, 5, 18) },
            new Contract { ContractNumber = "Д011", EmployeeId = 11, OrganizationName = "ООО Кодекс", Date = new DateTime(2025, 6, 5) },
            new Contract { ContractNumber = "Д012", EmployeeId = 12, OrganizationName = "ООО Вебмастер", Date = new DateTime(2025, 6, 22) },
            new Contract { ContractNumber = "Д013", EmployeeId = 13, OrganizationName = "ООО ДатаПро", Date = new DateTime(2025, 7, 10) },
            new Contract { ContractNumber = "Д014", EmployeeId = 14, OrganizationName = "ООО Продукт", Date = new DateTime(2025, 7, 28) },
            new Contract { ContractNumber = "Д015", EmployeeId = 15, OrganizationName = "ООО Качество", Date = new DateTime(2025, 8, 15) }
        );
        db.SaveChanges();

        db.Supplies.AddRange(
            new Supply { ContractNumber = "Д001", EquipmentType = "Сервер", Comment = "Работает стабильно", EmployeeId = 1 },
            new Supply { ContractNumber = "Д002", EquipmentType = "ПК", Comment = "Хорошая производительность", EmployeeId = 2 },
            new Supply { ContractNumber = "Д003", EquipmentType = "Ноутбук", Comment = null, EmployeeId = 3 },
            new Supply { ContractNumber = "Д004", EquipmentType = "Сервер", Comment = "Требует дополнительной настройки", EmployeeId = 4 },
            new Supply { ContractNumber = "Д005", EquipmentType = "ПК", Comment = null, EmployeeId = 5 },
            new Supply { ContractNumber = "Д006", EquipmentType = "Сетевое оборудование", Comment = "Отличное качество сборки", EmployeeId = 1 },
            new Supply { ContractNumber = "Д007", EquipmentType = "Сервер", Comment = "Высокая производительность", EmployeeId = 7 },
            new Supply { ContractNumber = "Д008", EquipmentType = "ПК", Comment = "Компактный и стильный", EmployeeId = 8 },
            new Supply { ContractNumber = "Д009", EquipmentType = "Сетевое оборудование", Comment = "Надёжное подключение", EmployeeId = 9 },
            new Supply { ContractNumber = "Д010", EquipmentType = "Ноутбук", Comment = "Лёгкий и мощный", EmployeeId = 10 },
            new Supply { ContractNumber = "Д011", EquipmentType = "Сервер", Comment = "Масштабируемое решение", EmployeeId = 11 },
            new Supply { ContractNumber = "Д012", EquipmentType = "ПК", Comment = "Для разработки", EmployeeId = 12 },
            new Supply { ContractNumber = "Д013", EquipmentType = "Сервер", Comment = "AI-оптимизированный", EmployeeId = 13 },
            new Supply { ContractNumber = "Д014", EquipmentType = "Ноутбук", Comment = "Для презентаций", EmployeeId = 14 },
            new Supply { ContractNumber = "Д015", EquipmentType = "ПК", Comment = "Тестовый стенд", EmployeeId = 15 }
        );
        db.SaveChanges();

        db.Organizations.AddRange(
            new Organization { ContractNumber = "Д001", CountryCode = "RU", City = "Москва", Address = "ул. Ленина, д. 10", Phone = "8-495-123-45-67", Email = "info@romashka.ru", Website = "www.romashka.ru" },
            new Organization { ContractNumber = "Д001", CountryCode = "RU", City = "Новосибирск", Address = "ул. Красная, д. 50", Phone = "8-383-123-45-67", Email = "ns@romashka.ru", Website = null },
            new Organization { ContractNumber = "Д002", CountryCode = "BY", City = "Минск", Address = "пр. Независимости, д. 25", Phone = "8-017-123-45-67", Email = "info@vasilek.by", Website = null },
            new Organization { ContractNumber = "Д003", CountryCode = "RU", City = "Санкт-Петербург", Address = "ул. Пушкина, д. 5", Phone = "8-812-123-45-67", Email = "sidorov@spb.ru", Website = null },
            new Organization { ContractNumber = "Д004", CountryCode = "KZ", City = "Алматы", Address = "пр. Абая, д. 100", Phone = "8-727-123-45-67", Email = "info@lyutik.kz", Website = "www.lyutik.kz" },
            new Organization { ContractNumber = "Д005", CountryCode = "RU", City = "Москва", Address = "ул. Гагарина, д. 20", Phone = "8-495-987-65-43", Email = "info@romashka.ru", Website = "www.romashka.ru" },
            new Organization { ContractNumber = "Д006", CountryCode = "RU", City = "Казань", Address = "ул. Баумана, д. 15", Phone = "8-843-123-45-67", Email = "info@tulip.ru", Website = "www.tulip.ru" },
            new Organization { ContractNumber = "Д007", CountryCode = "RU", City = "Екатеринбург", Address = "ул. Ленина, д. 1", Phone = "8-343-123-45-67", Email = "info@newtech.ru", Website = "www.newtech.ru" },
            new Organization { ContractNumber = "Д008", CountryCode = "RU", City = "Самара", Address = "ул. Куйбышева, д. 30", Phone = "8-846-123-45-67", Email = "info@creative.ru", Website = null },
            new Organization { ContractNumber = "Д009", CountryCode = "BY", City = "Гродно", Address = "ул. Советская, д. 12", Phone = "8-0152-123-45-67", Email = "info@infra.by", Website = "www.infra.by" },
            new Organization { ContractNumber = "Д010", CountryCode = "KZ", City = "Астана", Address = "пр. Тауелсиздик, д. 5", Phone = "8-7172-123-45-67", Email = "info@promo.kz", Website = null },
            new Organization { ContractNumber = "Д011", CountryCode = "RU", City = "Нижний Новгород", Address = "ул. Большая Покровская, д. 8", Phone = "8-831-123-45-67", Email = "info@kodex.ru", Website = "www.kodex.ru" },
            new Organization { ContractNumber = "Д012", CountryCode = "RU", City = "Ростов-на-Дону", Address = "ул. Большая Садовая, д. 40", Phone = "8-863-123-45-67", Email = "info@webmaster.ru", Website = null },
            new Organization { ContractNumber = "Д013", CountryCode = "RU", City = "Владивосток", Address = "ул. Светла́нская, д. 100", Phone = "8-423-123-45-67", Email = "info@datapro.ru", Website = "www.datapro.ru" },
            new Organization { ContractNumber = "Д014", CountryCode = "BY", City = "Брест", Address = "ул. Советская, д. 60", Phone = "8-0162-123-45-67", Email = "info@product.by", Website = null },
            new Organization { ContractNumber = "Д015", CountryCode = "KZ", City = "Шымкент", Address = "ул. Тауке хана, д. 15", Phone = "8-7252-123-45-67", Email = "info@quality.kz", Website = null },
            new Organization { ContractNumber = "Д011", CountryCode = "RU", City = "Москва", Address = "ул. Арбат, д. 25", Phone = "8-495-111-22-33", Email = "moscow@kodex.ru", Website = null },
            new Organization { ContractNumber = "Д013", CountryCode = "RU", City = "Сочи", Address = "ул. Курортный проспект, д. 75", Phone = "8-862-123-45-67", Email = "sochi@datapro.ru", Website = null }
        );
        db.SaveChanges();
    }
}