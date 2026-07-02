using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KursMVVM.Messages;
using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace KursMVVM.ViewModels;

public partial class ReportsPageViewModel : ViewModelBase, IRecipient<DataChangedMessage>
{
    [ObservableProperty]
    private ObservableCollection<EmployeeReport> _employeeReports = new();

    [ObservableProperty]
    private ObservableCollection<SupplyReport> _supplyReports = new();

    [ObservableProperty]
    private ObservableCollection<ContractReport> _contractReports = new();

    [ObservableProperty]
    private ObservableCollection<Employee> _inactiveEmployees = new();

    [ObservableProperty]
    private ObservableCollection<GeoReport> _geoReports = new();

    [ObservableProperty]
    private int _selectedTabIndex;

    public ReportsPageViewModel()
    {
        _ = LoadAllAsync();
        Messenger.Register<DataChangedMessage>(this, (r, m) => _ = LoadAllAsync());
    }

    public void Receive(DataChangedMessage message)
    {
        _ = LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        using var db = new KursContext();

        var empData = await db.Employees
            .Include(e => e.Supplies)
            .ThenInclude(s => s.Contract)
            .ToListAsync();

        EmployeeReports = new ObservableCollection<EmployeeReport>(
            empData.Select(e => new EmployeeReport
            {
                FullName = e.FullName,
                Position = e.Position,
                Salary = e.Salary,
                Bonus = e.Bonus,
                TotalContracts = e.Supplies.Count,
                TotalAmount = e.Salary + e.Bonus
            }));

        var supData = await db.Supplies
            .Include(s => s.Organizations)
            .Include(s => s.Contract)
            .ToListAsync();

        SupplyReports = new ObservableCollection<SupplyReport>(
            supData.GroupBy(s => s.EquipmentType)
                .Select(g => new SupplyReport
                {
                    EquipmentType = g.Key,
                    ContractCount = g.Count(),
                    OrgCount = g.SelectMany(s => s.Organizations).Select(o => o.Id).Distinct().Count()
                }));

        var conData = await db.Contracts
            .Include(c => c.Employee)
            .Include(c => c.Supply)
            .ThenInclude(s => s!.Organizations)
            .ToListAsync();

        ContractReports = new ObservableCollection<ContractReport>(
            conData.Select(c => new ContractReport
            {
                ContractNumber = c.ContractNumber,
                OrganizationName = c.OrganizationName,
                Date = c.Date,
                EmployeeName = c.Employee?.FullName ?? "-",
                EquipmentType = c.Supply?.EquipmentType ?? "-",
                Comment = c.Supply?.Comment ?? "-"
            }));

        InactiveEmployees = new ObservableCollection<Employee>(
            empData.Where(e => e.Supplies.Count == 0));

        var orgData = await db.Organizations.ToListAsync();
        GeoReports = new ObservableCollection<GeoReport>(
            orgData.GroupBy(o => new { o.CountryCode, o.City })
                .Select(g => new GeoReport
                {
                    Country = g.Key.CountryCode,
                    City = g.Key.City,
                    ContractCount = g.Count(),
                    OrgCount = g.Count()
                }));
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadAllAsync();
    }
}

public class EmployeeReport
{
    public string FullName { get; set; } = "";
    public string Position { get; set; } = "";
    public decimal Salary { get; set; }
    public decimal Bonus { get; set; }
    public int TotalContracts { get; set; }
    public decimal TotalAmount { get; set; }
}

public class SupplyReport
{
    public string EquipmentType { get; set; } = "";
    public int ContractCount { get; set; }
    public int OrgCount { get; set; }
}

public class ContractReport
{
    public string ContractNumber { get; set; } = "";
    public string OrganizationName { get; set; } = "";
    public DateTime Date { get; set; }
    public string EmployeeName { get; set; } = "";
    public string EquipmentType { get; set; } = "";
    public string Comment { get; set; } = "";
}

public class GeoReport
{
    public string Country { get; set; } = "";
    public string City { get; set; } = "";
    public int ContractCount { get; set; }
    public int OrgCount { get; set; }
}
