using CsvHelper;
using CsvHelper.Configuration;
using KursMVVM.Messages;
using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.Messaging;

namespace KursMVVM.Services;

public class ImportExportService
{
    private void NotifyDataChanged() => WeakReferenceMessenger.Default.Send(new DataChangedMessage());

    // ===================== IMPORT =====================

    public async Task<List<ImportError>> ImportEmployeesFromCsvAsync(string path)
    {
        var errors = new List<ImportError>();
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read(); csv.ReadHeader();
        csv.Context.RegisterClassMap<EmployeeMap>();
        int row = 1;
        while (csv.Read())
        {
            row++;
            try
            {
                var emp = csv.GetRecord<Employee>();
                if (emp == null) continue;
                var validation = ValidateEmployee(emp, row);
                if (validation.Any()) { errors.AddRange(validation); continue; }
                using var db = new KursContext();
                if (!db.Employees.Any(e => e.Id == emp.Id))
                {
                    db.Employees.Add(emp);
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { Row = row, Message = ex.Message });
            }
        }
        NotifyDataChanged();
        return errors;
    }

    public async Task<List<ImportError>> ImportContractsFromCsvAsync(string path)
    {
        var errors = new List<ImportError>();
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read(); csv.ReadHeader();
        int row = 1;
        while (csv.Read())
        {
            row++;
            try
            {
                var num = csv.GetField("номер_договора");
                var empIdStr = csv.GetField("код_сотрудника");
                var org = csv.GetField("наименование_организации");
                var dateStr = csv.GetField("дата_заключения");
                if (!int.TryParse(empIdStr, out var empId))
                {
                    errors.Add(new ImportError { Row = row, Message = "Неверный код сотрудника: " + empIdStr });
                    continue;
                }
                if (!DateTime.TryParse(dateStr, out var date))
                {
                    errors.Add(new ImportError { Row = row, Message = "Неверная дата: " + dateStr });
                    continue;
                }
                if (date > DateTime.Now)
                {
                    errors.Add(new ImportError { Row = row, Message = "Дата в будущем" });
                    continue;
                }
                using var db = new KursContext();
                if (!db.Employees.Any(e => e.Id == empId))
                {
                    errors.Add(new ImportError { Row = row, Message = "Сотрудник не найден: " + empId });
                    continue;
                }
                if (!db.Contracts.Any(c => c.ContractNumber == num))
                {
                    db.Contracts.Add(new Contract { ContractNumber = num!, EmployeeId = empId, OrganizationName = org!, Date = date });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { Row = row, Message = ex.Message });
            }
        }
        NotifyDataChanged();
        return errors;
    }

    public async Task<List<ImportError>> ImportSuppliesFromCsvAsync(string path)
    {
        var errors = new List<ImportError>();
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read(); csv.ReadHeader();
        int row = 1;
        while (csv.Read())
        {
            row++;
            try
            {
                var conNum = csv.GetField("номер_договора");
                var equip = csv.GetField("тип_оборудования");
                var comment = csv.GetField("комментарий");
                var empIdStr = csv.GetField("код_сотрудника");
                if (!int.TryParse(empIdStr, out var empId))
                {
                    errors.Add(new ImportError { Row = row, Message = "Неверный код сотрудника" });
                    continue;
                }
                using var db = new KursContext();
                if (!db.Contracts.Any(c => c.ContractNumber == conNum))
                {
                    errors.Add(new ImportError { Row = row, Message = "Договор не найден: " + conNum });
                    continue;
                }
                if (db.Supplies.Any(s => s.ContractNumber == conNum))
                {
                    errors.Add(new ImportError { Row = row, Message = "Поставка уже существует для договора: " + conNum });
                    continue;
                }
                if (!db.Employees.Any(e => e.Id == empId))
                {
                    errors.Add(new ImportError { Row = row, Message = "Сотрудник не найден: " + empId });
                    continue;
                }
                db.Supplies.Add(new Supply
                {
                    ContractNumber = conNum!,
                    EquipmentType = equip!,
                    Comment = comment,
                    EmployeeId = empId
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { Row = row, Message = ex.Message });
            }
        }
        NotifyDataChanged();
        return errors;
    }

    public async Task<List<ImportError>> ImportOrganizationsFromCsvAsync(string path)
    {
        var errors = new List<ImportError>();
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Read(); csv.ReadHeader();
        int row = 1;
        while (csv.Read())
        {
            row++;
            try
            {
                var conNum = csv.GetField("номер_договора");
                var country = csv.GetField("код_страны");
                var city = csv.GetField("город");
                var addr = csv.GetField("адрес");
                var phone = csv.GetField("телефон");
                var email = csv.GetField("e_mail");
                var web = csv.GetField("web_site");
                if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
                {
                    errors.Add(new ImportError { Row = row, Message = "Неверный email: " + email });
                    continue;
                }
                using var db = new KursContext();
                if (!db.Contracts.Any(c => c.ContractNumber == conNum))
                {
                    errors.Add(new ImportError { Row = row, Message = "Договор не найден: " + conNum });
                    continue;
                }
                db.Organizations.Add(new Organization
                {
                    ContractNumber = conNum!,
                    CountryCode = country!,
                    City = city!,
                    Address = addr!,
                    Phone = phone!,
                    Email = email!,
                    Website = string.IsNullOrEmpty(web) ? null : web
                });
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                errors.Add(new ImportError { Row = row, Message = ex.Message });
            }
        }
        NotifyDataChanged();
        return errors;
    }

    public async Task<List<ImportError>> ImportFromJsonAsync(string path)
    {
        var errors = new List<ImportError>();
        try
        {
            using var fs = File.OpenRead(path);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = false };
            var data = await JsonSerializer.DeserializeAsync<FullData>(fs, options);
            if (data == null) { errors.Add(new ImportError { Message = "Нет данных в JSON" }); return errors; }

            using var db = new KursContext();
            db.Organizations.RemoveRange(db.Organizations);
            db.Supplies.RemoveRange(db.Supplies);
            db.Contracts.RemoveRange(db.Contracts);
            db.Employees.RemoveRange(db.Employees);
            await db.SaveChangesAsync();

            foreach (var e in data.сотрудники ?? new()) db.Employees.Add(e);
            foreach (var c in data.договоры ?? new()) db.Contracts.Add(c);
            foreach (var s in data.поставки ?? new()) db.Supplies.Add(s);
            foreach (var o in data.организации ?? new()) db.Organizations.Add(o);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            errors.Add(new ImportError { Message = "JSON ошибка: " + ex.Message });
        }
        NotifyDataChanged();
        return errors;
    }

    public async Task<List<ImportError>> ImportFromXmlAsync(string path)
    {
        var errors = new List<ImportError>();
        try
        {
            using var fs = File.OpenRead(path);
            var serializer = new XmlSerializer(typeof(FullDataXml));
            if (serializer.Deserialize(fs) is not FullDataXml data) { errors.Add(new ImportError { Message = "Нет данных в XML" }); return errors; }

            using var db = new KursContext();
            db.Organizations.RemoveRange(db.Organizations);
            db.Supplies.RemoveRange(db.Supplies);
            db.Contracts.RemoveRange(db.Contracts);
            db.Employees.RemoveRange(db.Employees);
            await db.SaveChangesAsync();

            foreach (var e in data.Employees ?? new()) db.Employees.Add(e);
            foreach (var c in data.Contracts ?? new()) db.Contracts.Add(c);
            foreach (var s in data.Supplies ?? new()) db.Supplies.Add(s);
            foreach (var o in data.Organizations ?? new()) db.Organizations.Add(o);
            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            errors.Add(new ImportError { Message = "XML ошибка: " + ex.Message });
        }
        NotifyDataChanged();
        return errors;
    }

    public async Task<List<ImportError>> ImportFromExcelAsync(string path)
    {
        var errors = new List<ImportError>();
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var package = new ExcelPackage(new FileInfo(path));

        bool hasAll = package.Workbook.Worksheets["Сотрудники"] != null
                   && package.Workbook.Worksheets["Договоры"] != null
                   && package.Workbook.Worksheets["Поставки"] != null
                   && package.Workbook.Worksheets["Организации"] != null;
        if (hasAll)
        {
            using var db = new KursContext();
            db.Organizations.RemoveRange(db.Organizations);
            db.Supplies.RemoveRange(db.Supplies);
            db.Contracts.RemoveRange(db.Contracts);
            db.Employees.RemoveRange(db.Employees);
            await db.SaveChangesAsync();
        }

        var empErrors = await ImportEmployeesFromExcelWorksheetAsync(package.Workbook.Worksheets["Сотрудники"]);
        var conErrors = await ImportContractsFromExcelWorksheetAsync(package.Workbook.Worksheets["Договоры"]);
        var supErrors = await ImportSuppliesFromExcelWorksheetAsync(package.Workbook.Worksheets["Поставки"]);
        var orgErrors = await ImportOrganizationsFromExcelWorksheetAsync(package.Workbook.Worksheets["Организации"]);

        errors.AddRange(empErrors);
        errors.AddRange(conErrors);
        errors.AddRange(supErrors);
        errors.AddRange(orgErrors);
        NotifyDataChanged();
        return errors;
    }

    private async Task<List<ImportError>> ImportEmployeesFromExcelWorksheetAsync(OfficeOpenXml.ExcelWorksheet? ws)
    {
        var errors = new List<ImportError>();
        if (ws == null) return errors;
        int rows = ws.Dimension.Rows;
        using var db = new KursContext();
        for (int r = 2; r <= rows; r++)
        {
            try
            {
                var id = int.Parse(ws.Cells[r, 1].Text);
                var dept = int.Parse(ws.Cells[r, 2].Text);
                var fio = ws.Cells[r, 3].Text;
                var pos = ws.Cells[r, 4].Text;
                var sal = decimal.Parse(ws.Cells[r, 5].Text);
                var bon = decimal.Parse(ws.Cells[r, 6].Text);
                var month = ws.Cells[r, 7].Text;
                if (sal <= 0) { errors.Add(new ImportError { Row = r, Message = "Оклад должен быть > 0" }); continue; }
                if (bon < 0) { errors.Add(new ImportError { Row = r, Message = "Премия должна быть >= 0" }); continue; }
                if (!db.Employees.Any(e => e.Id == id))
                    db.Employees.Add(new Employee { Id = id, DepartmentCode = dept, FullName = fio, Position = pos, Salary = sal, Bonus = bon, Month = month });
            }
            catch (Exception ex) { errors.Add(new ImportError { Row = r, Message = ex.Message }); }
        }
        await db.SaveChangesAsync();
        return errors;
    }

    private async Task<List<ImportError>> ImportContractsFromExcelWorksheetAsync(OfficeOpenXml.ExcelWorksheet? ws)
    {
        var errors = new List<ImportError>();
        if (ws == null) return errors;
        int rows = ws.Dimension.Rows;
        using var db = new KursContext();
        for (int r = 2; r <= rows; r++)
        {
            try
            {
                var num = ws.Cells[r, 1].Text;
                var empId = int.Parse(ws.Cells[r, 2].Text);
                var org = ws.Cells[r, 3].Text;
                var date = DateTime.Parse(ws.Cells[r, 4].Text);
                if (date > DateTime.Now) { errors.Add(new ImportError { Row = r, Message = "Дата в будущем" }); continue; }
                if (!db.Employees.Any(e => e.Id == empId)) { errors.Add(new ImportError { Row = r, Message = "Сотрудник не найден: " + empId }); continue; }
                if (!db.Contracts.Any(c => c.ContractNumber == num))
                    db.Contracts.Add(new Contract { ContractNumber = num, EmployeeId = empId, OrganizationName = org, Date = date });
            }
            catch (Exception ex) { errors.Add(new ImportError { Row = r, Message = ex.Message }); }
        }
        await db.SaveChangesAsync();
        return errors;
    }

    private async Task<List<ImportError>> ImportSuppliesFromExcelWorksheetAsync(OfficeOpenXml.ExcelWorksheet? ws)
    {
        var errors = new List<ImportError>();
        if (ws == null) return errors;
        int rows = ws.Dimension.Rows;
        using var db = new KursContext();
        for (int r = 2; r <= rows; r++)
        {
            try
            {
                var conNum = ws.Cells[r, 1].Text;
                var equip = ws.Cells[r, 2].Text;
                var comment = ws.Cells[r, 3].Text;
                var empId = int.Parse(ws.Cells[r, 4].Text);
                if (!db.Contracts.Any(c => c.ContractNumber == conNum)) { errors.Add(new ImportError { Row = r, Message = "Договор не найден: " + conNum }); continue; }
                if (db.Supplies.Any(s => s.ContractNumber == conNum)) { errors.Add(new ImportError { Row = r, Message = "Поставка уже существует: " + conNum }); continue; }
                if (!db.Employees.Any(e => e.Id == empId)) { errors.Add(new ImportError { Row = r, Message = "Сотрудник не найден: " + empId }); continue; }
                db.Supplies.Add(new Supply { ContractNumber = conNum, EquipmentType = equip, Comment = string.IsNullOrEmpty(comment) ? null : comment, EmployeeId = empId });
            }
            catch (Exception ex) { errors.Add(new ImportError { Row = r, Message = ex.Message }); }
        }
        await db.SaveChangesAsync();
        return errors;
    }

    private async Task<List<ImportError>> ImportOrganizationsFromExcelWorksheetAsync(OfficeOpenXml.ExcelWorksheet? ws)
    {
        var errors = new List<ImportError>();
        if (ws == null) return errors;
        int rows = ws.Dimension.Rows;
        using var db = new KursContext();
        for (int r = 2; r <= rows; r++)
        {
            try
            {
                var conNum = ws.Cells[r, 1].Text;
                var country = ws.Cells[r, 2].Text;
                var city = ws.Cells[r, 3].Text;
                var addr = ws.Cells[r, 4].Text;
                var phone = ws.Cells[r, 5].Text;
                var email = ws.Cells[r, 6].Text;
                var web = ws.Cells[r, 7].Text;
                if (!string.IsNullOrEmpty(email) && !email.Contains("@")) { errors.Add(new ImportError { Row = r, Message = "Неверный email: " + email }); continue; }
                if (!db.Contracts.Any(c => c.ContractNumber == conNum)) { errors.Add(new ImportError { Row = r, Message = "Договор не найден: " + conNum }); continue; }
                db.Organizations.Add(new Organization { ContractNumber = conNum, CountryCode = country, City = city, Address = addr, Phone = phone, Email = email, Website = string.IsNullOrEmpty(web) ? null : web });
            }
            catch (Exception ex) { errors.Add(new ImportError { Row = r, Message = ex.Message }); }
        }
        await db.SaveChangesAsync();
        return errors;
    }

    // ===================== EXPORT =====================

    public async Task<int> ExportAsync(string target, string format, string path)
    {
        return format.ToLower() switch
        {
            var f when f.Contains("excel") || f.Contains("xlsx") => await ExportToExcelAsync(target, path),
            var f when f.Contains("csv") => await ExportToCsvAsync(target, path),
            var f when f.Contains("pdf") => await ExportToPdfAsync(target, path),
            var f when f.Contains("json") => await ExportToJsonAsync(target, path),
            var f when f.Contains("xml") => await ExportToXmlAsync(target, path),
            _ => await ExportToExcelAsync(target, path)
        };
    }

    public async Task<int> ExportToCsvAsync(string target, string path)
    {
        using var db = new KursContext();
        using var writer = new StreamWriter(path, false, System.Text.Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        switch (NormalizeTarget(target))
        {
            case "contracts":
                var contracts = await db.Contracts.Include(c => c.Employee).Select(c => new
                {
                    Номер_договора = c.ContractNumber,
                    Код_сотрудника = c.EmployeeId,
                    Сотрудник = c.Employee != null ? c.Employee.FullName : "",
                    Организация = c.OrganizationName,
                    Дата = c.Date.ToString("yyyy-MM-dd")
                }).ToListAsync();
                csv.WriteRecords(contracts);
                return contracts.Count;

            case "supplies":
                var supplies = await db.Supplies.Include(s => s.Employee).Select(s => new
                {
                    Номер_договора = s.ContractNumber,
                    Тип_оборудования = s.EquipmentType,
                    Комментарий = s.Comment ?? "",
                    Код_сотрудника = s.EmployeeId,
                    Сотрудник = s.Employee != null ? s.Employee.FullName : ""
                }).ToListAsync();
                csv.WriteRecords(supplies);
                return supplies.Count;

            case "organizations":
                var orgs = await db.Organizations.Select(o => new
                {
                    ID = o.Id,
                    Номер_договора = o.ContractNumber,
                    Код_страны = o.CountryCode,
                    Город = o.City,
                    Адрес = o.Address,
                    Телефон = o.Phone,
                    Email = o.Email,
                    Веб_сайт = o.Website ?? ""
                }).ToListAsync();
                csv.WriteRecords(orgs);
                return orgs.Count;

            case "report_employees":
                var repEmp = await GetEmployeeReportDataAsync(db);
                csv.WriteRecords(repEmp);
                return repEmp.Count;

            case "report_supplies":
                var repSup = await GetSupplyReportDataAsync(db);
                csv.WriteRecords(repSup);
                return repSup.Count;

            case "report_contracts":
                var repCon = await GetContractReportDataAsync(db);
                csv.WriteRecords(repCon);
                return repCon.Count;

            case "report_inactive":
                var repInact = await GetInactiveEmployeesDataAsync(db);
                csv.WriteRecords(repInact);
                return repInact.Count;

            case "report_geo":
                var repGeo = await GetGeoReportDataAsync(db);
                csv.WriteRecords(repGeo);
                return repGeo.Count;

            case "employees":
            default:
                var emps = await db.Employees.Select(e => new
                {
                    Код = e.Id,
                    Отдел = e.DepartmentCode,
                    ФИО = e.FullName,
                    Должность = e.Position,
                    Оклад = e.Salary,
                    Премия = e.Bonus,
                    Месяц = e.Month
                }).ToListAsync();
                csv.WriteRecords(emps);
                return emps.Count;
        }
    }

    public async Task<int> ExportToExcelAsync(string target, string path)
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        using var db = new KursContext();
        int recordCount = 0;

        string norm = NormalizeTarget(target);

        if (norm == "all")
        {
            // All tables on separate sheets
            var emps = await db.Employees.ToListAsync();
            var cons = await db.Contracts.Include(c => c.Employee).ToListAsync();
            var sups = await db.Supplies.Include(s => s.Employee).ToListAsync();
            var orgs = await db.Organizations.ToListAsync();

            WriteExcelEmployeesSheet(package.Workbook.Worksheets.Add("Сотрудники"), emps);
            WriteExcelContractsSheet(package.Workbook.Worksheets.Add("Договоры"), cons);
            WriteExcelSuppliesSheet(package.Workbook.Worksheets.Add("Поставки"), sups);
            WriteExcelOrganizationsSheet(package.Workbook.Worksheets.Add("Организации"), orgs);

            recordCount = emps.Count + cons.Count + sups.Count + orgs.Count;
        }
        else if (norm == "contracts")
        {
            var cons = await db.Contracts.Include(c => c.Employee).ToListAsync();
            WriteExcelContractsSheet(package.Workbook.Worksheets.Add("Договоры"), cons);
            recordCount = cons.Count;
        }
        else if (norm == "supplies")
        {
            var sups = await db.Supplies.Include(s => s.Employee).ToListAsync();
            WriteExcelSuppliesSheet(package.Workbook.Worksheets.Add("Поставки"), sups);
            recordCount = sups.Count;
        }
        else if (norm == "organizations")
        {
            var orgs = await db.Organizations.ToListAsync();
            WriteExcelOrganizationsSheet(package.Workbook.Worksheets.Add("Организации"), orgs);
            recordCount = orgs.Count;
        }
        else if (norm == "report_employees")
        {
            var data = await GetEmployeeReportDataAsync(db);
            var ws = package.Workbook.Worksheets.Add("Сводка по сотрудникам");
            string[] h = { "Ф.И.О.", "Должность", "Оклад", "Премия", "Всего договоров", "Итого начислено" };
            StyleExcelHeader(ws, h);
            for (int i = 0; i < data.Count; i++)
            {
                var r = i + 2;
                ws.Cells[r, 1].Value = data[i].FullName;
                ws.Cells[r, 2].Value = data[i].Position;
                ws.Cells[r, 3].Value = data[i].Salary;
                ws.Cells[r, 3].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[r, 4].Value = data[i].Bonus;
                ws.Cells[r, 4].Style.Numberformat.Format = "#,##0.00";
                ws.Cells[r, 5].Value = data[i].TotalContracts;
                ws.Cells[r, 6].Value = data[i].TotalAmount;
                ws.Cells[r, 6].Style.Numberformat.Format = "#,##0.00";
            }
            ws.Cells.AutoFitColumns();
            recordCount = data.Count;
        }
        else if (norm == "report_supplies")
        {
            var data = await GetSupplyReportDataAsync(db);
            var ws = package.Workbook.Worksheets.Add("Сводка по поставкам");
            string[] h = { "Тип оборудования", "Количество договоров", "Количество организаций" };
            StyleExcelHeader(ws, h);
            for (int i = 0; i < data.Count; i++)
            {
                var r = i + 2;
                ws.Cells[r, 1].Value = data[i].EquipmentType;
                ws.Cells[r, 2].Value = data[i].ContractCount;
                ws.Cells[r, 3].Value = data[i].OrgCount;
            }
            ws.Cells.AutoFitColumns();
            recordCount = data.Count;
        }
        else if (norm == "report_contracts")
        {
            var data = await GetContractReportDataAsync(db);
            var ws = package.Workbook.Worksheets.Add("Сводка по договорам");
            string[] h = { "Номер договора", "Организация", "Дата заключения", "Сотрудник", "Тип оборудования", "Комментарий" };
            StyleExcelHeader(ws, h);
            for (int i = 0; i < data.Count; i++)
            {
                var r = i + 2;
                ws.Cells[r, 1].Value = data[i].ContractNumber;
                ws.Cells[r, 2].Value = data[i].OrganizationName;
                ws.Cells[r, 3].Value = data[i].Date.ToString("yyyy-MM-dd");
                ws.Cells[r, 4].Value = data[i].EmployeeName;
                ws.Cells[r, 5].Value = data[i].EquipmentType;
                ws.Cells[r, 6].Value = data[i].Comment;
            }
            ws.Cells.AutoFitColumns();
            recordCount = data.Count;
        }
        else if (norm == "report_inactive")
        {
            var data = await GetInactiveEmployeesDataAsync(db);
            var ws = package.Workbook.Worksheets.Add("Неактивные сотрудники");
            string[] h = { "Код", "Ф.И.О.", "Должность", "Код отдела" };
            StyleExcelHeader(ws, h);
            for (int i = 0; i < data.Count; i++)
            {
                var r = i + 2;
                ws.Cells[r, 1].Value = data[i].Id;
                ws.Cells[r, 2].Value = data[i].FullName;
                ws.Cells[r, 3].Value = data[i].Position;
                ws.Cells[r, 4].Value = data[i].DepartmentCode;
            }
            ws.Cells.AutoFitColumns();
            recordCount = data.Count;
        }
        else if (norm == "report_geo")
        {
            var data = await GetGeoReportDataAsync(db);
            var ws = package.Workbook.Worksheets.Add("География поставок");
            string[] h = { "Страна", "Город", "Количество договоров", "Количество организаций" };
            StyleExcelHeader(ws, h);
            for (int i = 0; i < data.Count; i++)
            {
                var r = i + 2;
                ws.Cells[r, 1].Value = data[i].Country;
                ws.Cells[r, 2].Value = data[i].City;
                ws.Cells[r, 3].Value = data[i].ContractCount;
                ws.Cells[r, 4].Value = data[i].OrgCount;
            }
            ws.Cells.AutoFitColumns();
            recordCount = data.Count;
        }
        else
        {
            // Employees
            var emps = await db.Employees.ToListAsync();
            WriteExcelEmployeesSheet(package.Workbook.Worksheets.Add("Сотрудники"), emps);
            recordCount = emps.Count;
        }

        await package.SaveAsAsync(new FileInfo(path));
        return recordCount;
    }

    public async Task<int> ExportToPdfAsync(string target, string path)
    {
        using var db = new KursContext();
        using var writer = new iText.Kernel.Pdf.PdfWriter(path);
        using var pdf = new iText.Kernel.Pdf.PdfDocument(writer);
        using var doc = new iText.Layout.Document(pdf);

        // Safe font initialization with Cyrillic support
        string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
        iText.Kernel.Font.PdfFont font = File.Exists(fontPath)
            ? iText.Kernel.Font.PdfFontFactory.CreateFont(fontPath, iText.IO.Font.PdfEncodings.IDENTITY_H)
            : iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);

        doc.SetFont(font);

        var norm = NormalizeTarget(target);
        string title = GetTargetDisplayName(norm);
        int recordCount = 0;

        // Header block
        var headerPara = new iText.Layout.Element.Paragraph(title)
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            .SetFontSize(18)
            .SimulateBold()
            .SetFontColor(new iText.Kernel.Colors.DeviceRgb(30, 41, 59))
            .SetMarginBottom(4);
        doc.Add(headerPara);

        var datePara = new iText.Layout.Element.Paragraph($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            .SetFontSize(10)
            .SetFontColor(new iText.Kernel.Colors.DeviceRgb(100, 116, 139))
            .SetMarginBottom(16);
        doc.Add(datePara);

        if (norm == "contracts")
        {
            var list = await db.Contracts.Include(c => c.Employee).ToListAsync();
            recordCount = list.Count;
            string[] headers = { "Номер договора", "Сотрудник", "Организация", "Дата заключения" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.ContractNumber, font));
                table.AddCell(CreatePdfCell(item.Employee?.FullName ?? "-", font));
                table.AddCell(CreatePdfCell(item.OrganizationName, font));
                table.AddCell(CreatePdfCell(item.Date.ToString("yyyy-MM-dd"), font));
            }
            doc.Add(table);
        }
        else if (norm == "supplies")
        {
            var list = await db.Supplies.Include(s => s.Employee).ToListAsync();
            recordCount = list.Count;
            string[] headers = { "Номер договора", "Тип оборудования", "Комментарий", "Сотрудник" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.ContractNumber, font));
                table.AddCell(CreatePdfCell(item.EquipmentType, font));
                table.AddCell(CreatePdfCell(item.Comment ?? "-", font));
                table.AddCell(CreatePdfCell(item.Employee?.FullName ?? "-", font));
            }
            doc.Add(table);
        }
        else if (norm == "organizations")
        {
            var list = await db.Organizations.ToListAsync();
            recordCount = list.Count;
            string[] headers = { "ID", "Договор", "Страна", "Город", "Адрес", "Телефон", "Email" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.Id.ToString(), font));
                table.AddCell(CreatePdfCell(item.ContractNumber, font));
                table.AddCell(CreatePdfCell(item.CountryCode, font));
                table.AddCell(CreatePdfCell(item.City, font));
                table.AddCell(CreatePdfCell(item.Address, font));
                table.AddCell(CreatePdfCell(item.Phone, font));
                table.AddCell(CreatePdfCell(item.Email, font));
            }
            doc.Add(table);
        }
        else if (norm == "report_employees")
        {
            var list = await GetEmployeeReportDataAsync(db);
            recordCount = list.Count;
            string[] headers = { "Ф.И.О.", "Должность", "Оклад", "Премия", "Договоров", "Итого" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.FullName, font));
                table.AddCell(CreatePdfCell(item.Position, font));
                table.AddCell(CreatePdfCell(item.Salary.ToString("N2"), font));
                table.AddCell(CreatePdfCell(item.Bonus.ToString("N2"), font));
                table.AddCell(CreatePdfCell(item.TotalContracts.ToString(), font));
                table.AddCell(CreatePdfCell(item.TotalAmount.ToString("N2"), font));
            }
            doc.Add(table);
        }
        else if (norm == "report_supplies")
        {
            var list = await GetSupplyReportDataAsync(db);
            recordCount = list.Count;
            string[] headers = { "Тип оборудования", "Количество договоров", "Организаций" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.EquipmentType, font));
                table.AddCell(CreatePdfCell(item.ContractCount.ToString(), font));
                table.AddCell(CreatePdfCell(item.OrgCount.ToString(), font));
            }
            doc.Add(table);
        }
        else if (norm == "report_contracts")
        {
            var list = await GetContractReportDataAsync(db);
            recordCount = list.Count;
            string[] headers = { "Номер", "Организация", "Дата", "Сотрудник", "Оборудование", "Комментарий" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.ContractNumber, font));
                table.AddCell(CreatePdfCell(item.OrganizationName, font));
                table.AddCell(CreatePdfCell(item.Date.ToString("yyyy-MM-dd"), font));
                table.AddCell(CreatePdfCell(item.EmployeeName, font));
                table.AddCell(CreatePdfCell(item.EquipmentType, font));
                table.AddCell(CreatePdfCell(item.Comment, font));
            }
            doc.Add(table);
        }
        else if (norm == "report_inactive")
        {
            var list = await GetInactiveEmployeesDataAsync(db);
            recordCount = list.Count;
            string[] headers = { "Код", "Ф.И.О.", "Должность", "Отдел" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.Id.ToString(), font));
                table.AddCell(CreatePdfCell(item.FullName, font));
                table.AddCell(CreatePdfCell(item.Position, font));
                table.AddCell(CreatePdfCell(item.DepartmentCode.ToString(), font));
            }
            doc.Add(table);
        }
        else if (norm == "report_geo")
        {
            var list = await GetGeoReportDataAsync(db);
            recordCount = list.Count;
            string[] headers = { "Страна", "Город", "Договоров", "Организаций" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.Country, font));
                table.AddCell(CreatePdfCell(item.City, font));
                table.AddCell(CreatePdfCell(item.ContractCount.ToString(), font));
                table.AddCell(CreatePdfCell(item.OrgCount.ToString(), font));
            }
            doc.Add(table);
        }
        else
        {
            // Employees (default)
            var list = await db.Employees.ToListAsync();
            recordCount = list.Count;
            string[] headers = { "Код", "Отдел", "Ф.И.О.", "Должность", "Оклад", "Премия", "Месяц" };
            var table = CreatePdfTable(headers, font);
            foreach (var item in list)
            {
                table.AddCell(CreatePdfCell(item.Id.ToString(), font));
                table.AddCell(CreatePdfCell(item.DepartmentCode.ToString(), font));
                table.AddCell(CreatePdfCell(item.FullName, font));
                table.AddCell(CreatePdfCell(item.Position, font));
                table.AddCell(CreatePdfCell(item.Salary.ToString("N2"), font));
                table.AddCell(CreatePdfCell(item.Bonus.ToString("N2"), font));
                table.AddCell(CreatePdfCell(item.Month, font));
            }
            doc.Add(table);
        }

        var totalPara = new iText.Layout.Element.Paragraph($"Всего записей в документе: {recordCount}")
            .SetFontSize(10)
            .SimulateItalic()
            .SetFontColor(new iText.Kernel.Colors.DeviceRgb(100, 116, 139))
            .SetMarginTop(12);
        doc.Add(totalPara);

        return recordCount;
    }

    public async Task<int> ExportToJsonAsync(string target, string path)
    {
        using var db = new KursContext();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string norm = NormalizeTarget(target);

        if (norm == "all")
        {
            var fullData = new FullData
            {
                сотрудники = await db.Employees.ToListAsync(),
                договоры = await db.Contracts.ToListAsync(),
                поставки = await db.Supplies.ToListAsync(),
                организации = await db.Organizations.ToListAsync()
            };
            await using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, fullData, options);
            return (fullData.сотрудники?.Count ?? 0) + (fullData.договоры?.Count ?? 0) +
                   (fullData.поставки?.Count ?? 0) + (fullData.организации?.Count ?? 0);
        }
        else if (norm == "contracts")
        {
            var list = await db.Contracts.ToListAsync();
            await using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, list, options);
            return list.Count;
        }
        else if (norm == "supplies")
        {
            var list = await db.Supplies.ToListAsync();
            await using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, list, options);
            return list.Count;
        }
        else if (norm == "organizations")
        {
            var list = await db.Organizations.ToListAsync();
            await using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, list, options);
            return list.Count;
        }
        else
        {
            var list = await db.Employees.ToListAsync();
            await using var fs = File.Create(path);
            await JsonSerializer.SerializeAsync(fs, list, options);
            return list.Count;
        }
    }

    public async Task<int> ExportToXmlAsync(string target, string path)
    {
        using var db = new KursContext();
        var fullData = new FullDataXml
        {
            Employees = await db.Employees.ToListAsync(),
            Contracts = await db.Contracts.ToListAsync(),
            Supplies = await db.Supplies.ToListAsync(),
            Organizations = await db.Organizations.ToListAsync()
        };
        await using var fs = File.Create(path);
        var serializer = new XmlSerializer(typeof(FullDataXml));
        serializer.Serialize(fs, fullData);
        return (fullData.Employees?.Count ?? 0) + (fullData.Contracts?.Count ?? 0) +
               (fullData.Supplies?.Count ?? 0) + (fullData.Organizations?.Count ?? 0);
    }

    // Backward-compatible wrappers
    public Task ExportEmployeesToCsvAsync(string path) => ExportToCsvAsync("Employees", path);
    public Task ExportEmployeesToExcelAsync(string path) => ExportToExcelAsync("Employees", path);
    public Task ExportEmployeesToPdfAsync(string path) => ExportToPdfAsync("Employees", path);

    // ===================== EXPORT HELPERS =====================

    private string NormalizeTarget(string target)
    {
        var t = target.Trim().ToLower();
        if (t.Contains("договор") && !t.Contains("отчёт") && !t.Contains("сводка")) return "contracts";
        if (t.Contains("поставк") && !t.Contains("отчёт") && !t.Contains("сводка")) return "supplies";
        if (t.Contains("организац")) return "organizations";
        if (t.Contains("все") || t.Contains("баз") || t.Contains("целиком")) return "all";
        if (t.Contains("сводка по сотрудникам") || t.Contains("отчёт: сводка по сотрудникам")) return "report_employees";
        if (t.Contains("сводка по поставкам") || t.Contains("отчёт: сводка по поставкам")) return "report_supplies";
        if (t.Contains("сводка по договорам") || t.Contains("отчёт: сводка по договорам")) return "report_contracts";
        if (t.Contains("неактивн") || t.Contains("отчёт: неактивные")) return "report_inactive";
        if (t.Contains("географ") || t.Contains("отчёт: география")) return "report_geo";
        return "employees";
    }

    private string GetTargetDisplayName(string normalizedTarget)
    {
        return normalizedTarget switch
        {
            "contracts" => "Договоры",
            "supplies" => "Поставки ПО",
            "organizations" => "Организации",
            "all" => "Полная база данных учёта поставок",
            "report_employees" => "Отчёт: Сводка по сотрудникам",
            "report_supplies" => "Отчёт: Сводка по поставкам",
            "report_contracts" => "Отчёт: Сводка по договорам",
            "report_inactive" => "Отчёт: Неактивные сотрудники",
            "report_geo" => "Отчёт: География поставок",
            _ => "Сотрудники"
        };
    }

    private void StyleExcelHeader(OfficeOpenXml.ExcelWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cells[1, i + 1];
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.Color.SetColor(System.Drawing.Color.White);
            cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(99, 102, 241)); // Tender Indigo
            cell.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
        }
        ws.Row(1).Height = 26;
    }

    private void WriteExcelEmployeesSheet(OfficeOpenXml.ExcelWorksheet ws, List<Employee> list)
    {
        string[] h = { "Код", "Отдел", "Ф.И.О.", "Должность", "Оклад", "Премия", "Месяц" };
        StyleExcelHeader(ws, h);
        for (int i = 0; i < list.Count; i++)
        {
            int r = i + 2;
            ws.Cells[r, 1].Value = list[i].Id;
            ws.Cells[r, 2].Value = list[i].DepartmentCode;
            ws.Cells[r, 3].Value = list[i].FullName;
            ws.Cells[r, 4].Value = list[i].Position;
            ws.Cells[r, 5].Value = list[i].Salary;
            ws.Cells[r, 5].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[r, 6].Value = list[i].Bonus;
            ws.Cells[r, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[r, 7].Value = list[i].Month;
        }
        ws.Cells.AutoFitColumns();
    }

    private void WriteExcelContractsSheet(OfficeOpenXml.ExcelWorksheet ws, List<Contract> list)
    {
        string[] h = { "Номер договора", "Код сотрудника", "Сотрудник", "Организация", "Дата заключения" };
        StyleExcelHeader(ws, h);
        for (int i = 0; i < list.Count; i++)
        {
            int r = i + 2;
            ws.Cells[r, 1].Value = list[i].ContractNumber;
            ws.Cells[r, 2].Value = list[i].EmployeeId;
            ws.Cells[r, 3].Value = list[i].Employee?.FullName ?? "-";
            ws.Cells[r, 4].Value = list[i].OrganizationName;
            ws.Cells[r, 5].Value = list[i].Date.ToString("yyyy-MM-dd");
        }
        ws.Cells.AutoFitColumns();
    }

    private void WriteExcelSuppliesSheet(OfficeOpenXml.ExcelWorksheet ws, List<Supply> list)
    {
        string[] h = { "Номер договора", "Тип оборудования", "Комментарий", "Код сотрудника", "Сотрудник" };
        StyleExcelHeader(ws, h);
        for (int i = 0; i < list.Count; i++)
        {
            int r = i + 2;
            ws.Cells[r, 1].Value = list[i].ContractNumber;
            ws.Cells[r, 2].Value = list[i].EquipmentType;
            ws.Cells[r, 3].Value = list[i].Comment ?? "-";
            ws.Cells[r, 4].Value = list[i].EmployeeId;
            ws.Cells[r, 5].Value = list[i].Employee?.FullName ?? "-";
        }
        ws.Cells.AutoFitColumns();
    }

    private void WriteExcelOrganizationsSheet(OfficeOpenXml.ExcelWorksheet ws, List<Organization> list)
    {
        string[] h = { "ID", "Номер договора", "Код страны", "Город", "Адрес", "Телефон", "Email", "Веб-сайт" };
        StyleExcelHeader(ws, h);
        for (int i = 0; i < list.Count; i++)
        {
            int r = i + 2;
            ws.Cells[r, 1].Value = list[i].Id;
            ws.Cells[r, 2].Value = list[i].ContractNumber;
            ws.Cells[r, 3].Value = list[i].CountryCode;
            ws.Cells[r, 4].Value = list[i].City;
            ws.Cells[r, 5].Value = list[i].Address;
            ws.Cells[r, 6].Value = list[i].Phone;
            ws.Cells[r, 7].Value = list[i].Email;
            ws.Cells[r, 8].Value = list[i].Website ?? "-";
        }
        ws.Cells.AutoFitColumns();
    }

    private iText.Layout.Element.Table CreatePdfTable(string[] headers, iText.Kernel.Font.PdfFont font)
    {
        var table = new iText.Layout.Element.Table(headers.Length).UseAllAvailableWidth();
        var headerBg = new iText.Kernel.Colors.DeviceRgb(99, 102, 241); // Soft Indigo

        foreach (var h in headers)
        {
            var cell = new iText.Layout.Element.Cell()
                .Add(new iText.Layout.Element.Paragraph(h)
                    .SetFont(font)
                    .SimulateBold()
                    .SetFontSize(10)
                    .SetFontColor(iText.Kernel.Colors.ColorConstants.WHITE))
                .SetBackgroundColor(headerBg)
                .SetPadding(6)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
            table.AddHeaderCell(cell);
        }
        return table;
    }

    private iText.Layout.Element.Cell CreatePdfCell(string text, iText.Kernel.Font.PdfFont font)
    {
        return new iText.Layout.Element.Cell()
            .Add(new iText.Layout.Element.Paragraph(text).SetFont(font).SetFontSize(9).SetFontColor(new iText.Kernel.Colors.DeviceRgb(30, 41, 59)))
            .SetPadding(5)
            .SetBorder(new iText.Layout.Borders.SolidBorder(new iText.Kernel.Colors.DeviceRgb(226, 232, 240), 0.5f));
    }

    // Reports data extractors
    public static async Task<List<EmployeeReportDto>> GetEmployeeReportDataAsync(KursContext db)
    {
        var empData = await db.Employees
            .Include(e => e.Supplies)
            .ThenInclude(s => s.Contract)
            .ToListAsync();

        return empData.Select(e => new EmployeeReportDto
        {
            FullName = e.FullName,
            Position = e.Position,
            Salary = e.Salary,
            Bonus = e.Bonus,
            TotalContracts = e.Supplies.Count,
            TotalAmount = e.Salary + e.Bonus
        }).ToList();
    }

    public static async Task<List<SupplyReportDto>> GetSupplyReportDataAsync(KursContext db)
    {
        var supData = await db.Supplies
            .Include(s => s.Organizations)
            .Include(s => s.Contract)
            .ToListAsync();

        return supData.GroupBy(s => s.EquipmentType)
            .Select(g => new SupplyReportDto
            {
                EquipmentType = g.Key,
                ContractCount = g.Count(),
                OrgCount = g.SelectMany(s => s.Organizations).Select(o => o.Id).Distinct().Count()
            }).ToList();
    }

    public static async Task<List<ContractReportDto>> GetContractReportDataAsync(KursContext db)
    {
        var conData = await db.Contracts
            .Include(c => c.Employee)
            .Include(c => c.Supply)
            .ThenInclude(s => s!.Organizations)
            .ToListAsync();

        return conData.Select(c => new ContractReportDto
        {
            ContractNumber = c.ContractNumber,
            OrganizationName = c.OrganizationName,
            Date = c.Date,
            EmployeeName = c.Employee?.FullName ?? "-",
            EquipmentType = c.Supply?.EquipmentType ?? "-",
            Comment = c.Supply?.Comment ?? "-"
        }).ToList();
    }

    public static async Task<List<Employee>> GetInactiveEmployeesDataAsync(KursContext db)
    {
        var empData = await db.Employees
            .Include(e => e.Supplies)
            .ToListAsync();
        return empData.Where(e => e.Supplies.Count == 0).ToList();
    }

    public static async Task<List<GeoReportDto>> GetGeoReportDataAsync(KursContext db)
    {
        var orgData = await db.Organizations.ToListAsync();
        return orgData.GroupBy(o => new { o.CountryCode, o.City })
            .Select(g => new GeoReportDto
            {
                Country = g.Key.CountryCode,
                City = g.Key.City,
                ContractCount = g.Count(),
                OrgCount = g.Count()
            }).ToList();
    }

    private List<ImportError> ValidateEmployee(Employee e, int row)
    {
        var errs = new List<ImportError>();
        if (e.Salary <= 0) errs.Add(new ImportError { Row = row, Message = "Оклад должен быть > 0" });
        if (e.Bonus < 0) errs.Add(new ImportError { Row = row, Message = "Премия должна быть >= 0" });
        return errs;
    }
}

public class ImportError
{
    public int Row { get; set; }
    public string Message { get; set; } = "";
    public override string ToString() => $"Строка {Row}: {Message}";
}

public class FullData
{
    public List<Employee>? сотрудники { get; set; }
    public List<Contract>? договоры { get; set; }
    public List<Supply>? поставки { get; set; }
    public List<Organization>? организации { get; set; }
}

[XmlRoot("данные")]
public class FullDataXml
{
    [XmlArray("сотрудники")]
    public List<Employee>? Employees { get; set; }

    [XmlArray("договоры")]
    public List<Contract>? Contracts { get; set; }

    [XmlArray("поставки")]
    public List<Supply>? Supplies { get; set; }

    [XmlArray("организации")]
    public List<Organization>? Organizations { get; set; }
}

public class EmployeeReportDto
{
    public string FullName { get; set; } = "";
    public string Position { get; set; } = "";
    public decimal Salary { get; set; }
    public decimal Bonus { get; set; }
    public int TotalContracts { get; set; }
    public decimal TotalAmount { get; set; }
}

public class SupplyReportDto
{
    public string EquipmentType { get; set; } = "";
    public int ContractCount { get; set; }
    public int OrgCount { get; set; }
}

public class ContractReportDto
{
    public string ContractNumber { get; set; } = "";
    public string OrganizationName { get; set; } = "";
    public DateTime Date { get; set; }
    public string EmployeeName { get; set; } = "";
    public string EquipmentType { get; set; } = "";
    public string Comment { get; set; } = "";
}

public class GeoReportDto
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public int ContractCount { get; set; }
    public int OrgCount { get; set; }
}