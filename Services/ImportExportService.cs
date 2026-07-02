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

    public async Task ExportEmployeesToCsvAsync(string path)
    {
        using var db = new KursContext();
        var list = await db.Employees.ToListAsync();
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(list);
    }

    public async Task ExportEmployeesToExcelAsync(string path)
    {
        ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
        using var db = new KursContext();
        var list = await db.Employees.ToListAsync();
        using var package = new ExcelPackage();
        var ws = package.Workbook.Worksheets.Add("Сотрудники");
        ws.Cells[1, 1].Value = "Код"; ws.Cells[1, 2].Value = "Отдел";
        ws.Cells[1, 3].Value = "Ф.И.О."; ws.Cells[1, 4].Value = "Должность";
        ws.Cells[1, 5].Value = "Оклад"; ws.Cells[1, 6].Value = "Премия"; ws.Cells[1, 7].Value = "Месяц";
        for (int i = 0; i < list.Count; i++)
        {
            ws.Cells[i + 2, 1].Value = list[i].Id;
            ws.Cells[i + 2, 2].Value = list[i].DepartmentCode;
            ws.Cells[i + 2, 3].Value = list[i].FullName;
            ws.Cells[i + 2, 4].Value = list[i].Position;
            ws.Cells[i + 2, 5].Value = list[i].Salary;
            ws.Cells[i + 2, 5].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[i + 2, 6].Value = list[i].Bonus;
            ws.Cells[i + 2, 6].Style.Numberformat.Format = "#,##0.00";
            ws.Cells[i + 2, 7].Value = list[i].Month;
        }
        ws.Cells.AutoFitColumns();
        await package.SaveAsAsync(new FileInfo(path));
    }

    public async Task ExportEmployeesToPdfAsync(string path)
    {
        using var db = new KursContext();
        var list = await db.Employees.ToListAsync();
        using var pdf = new iText.Kernel.Pdf.PdfDocument(new iText.Kernel.Pdf.PdfWriter(path));
        using var doc = new iText.Layout.Document(pdf);
        var font = iText.Kernel.Font.PdfFontFactory.CreateFont(iText.IO.Font.Constants.StandardFonts.HELVETICA);
        doc.SetFont(font);
        doc.Add(new iText.Layout.Element.Paragraph("Отчёт по сотрудникам")
            .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
            .SetFontSize(18));
        var table = new iText.Layout.Element.Table(7).UseAllAvailableWidth();
        string[] headers = { "Код", "Отдел", "Ф.И.О.", "Должность", "Оклад", "Премия", "Месяц" };
        foreach (var h in headers) table.AddHeaderCell(h);
        foreach (var e in list)
        {
            table.AddCell(e.Id.ToString());
            table.AddCell(e.DepartmentCode.ToString());
            table.AddCell(e.FullName);
            table.AddCell(e.Position);
            table.AddCell(e.Salary.ToString("F2"));
            table.AddCell(e.Bonus.ToString("F2"));
            table.AddCell(e.Month);
        }
        doc.Add(table);
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