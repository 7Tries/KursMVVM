using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using KursMVVM.Messages;
using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KursMVVM.ViewModels;

public partial class GraphsPageViewModel : ViewModelBase, IRecipient<DataChangedMessage>
{
    [ObservableProperty]
    private ISeries[] _contractsByEmployeeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _contractsByEmployeeXAxes = new[] { new Axis() };

    [ObservableProperty]
    private ISeries[] _equipmentTypeSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _contractDynamicsSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _salaryDistributionSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = new[] { new Axis { Name = "Месяц" } };

    [ObservableProperty]
    private Axis[] _yAxes = new[] { new Axis { Name = "Количество" } };

    public GraphsPageViewModel()
    {
        _ = LoadChartsAsync();
        Messenger.Register<DataChangedMessage>(this, (r, m) => _ = LoadChartsAsync());
    }

    public void Receive(DataChangedMessage message)
    {
        _ = LoadChartsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadChartsAsync();
    }

    private async Task LoadChartsAsync()
    {
        using var db = new KursContext();

        var empData = await db.Employees.ToListAsync();
        var conData = await db.Contracts.ToListAsync();

        var contractsByEmp = empData.Select(e => new
        {
            Name = e.FullName.Split(' ')[0],
            Count = conData.Count(c => c.EmployeeId == e.Id)
        }).ToList();

        ContractsByEmployeeSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Договоры",
                Values = contractsByEmp.Select(x => x.Count).ToArray(),
                Fill = new SolidColorPaint(new SKColor(194, 154, 107))
            }
        };

        ContractsByEmployeeXAxes = new[] { new Axis { Labels = contractsByEmp.Select(x => x.Name).ToArray() } };

        var supData = await db.Supplies.ToListAsync();
        var equipTypes = supData.GroupBy(s => s.EquipmentType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToList();

        EquipmentTypeSeries = equipTypes.Select(e => new PieSeries<int>
        {
            Name = e.Type,
            Values = new[] { e.Count },
            DataLabelsPaint = new SolidColorPaint(SKColors.Black),
            DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
            DataLabelsFormatter = point => $"{e.Type}: {point.Coordinate.PrimaryValue}"
        }).ToArray<ISeries>();

        var dynamics = conData.GroupBy(c => c.Date.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToList();

        XAxes = new[] { new Axis { Name = "Месяц", Labels = dynamics.Select(x => x.Month).ToArray() } };

        ContractDynamicsSeries = new ISeries[]
        {
            new LineSeries<int>
            {
                Name = "Договоры",
                Values = dynamics.Select(x => x.Count).ToArray(),
                Stroke = new SolidColorPaint(new SKColor(139, 188, 143)) { StrokeThickness = 3 },
                Fill = new SolidColorPaint(new SKColor(139, 188, 143, 50)),
                GeometryFill = new SolidColorPaint(new SKColor(139, 188, 143)),
                GeometrySize = 10
            }
        };

        var salaries = empData.Select(e => (double)e.Salary).ToArray();
        SalaryDistributionSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Оклады",
                Values = salaries,
                Fill = new SolidColorPaint(new SKColor(210, 180, 140))
            }
        };
    }
}
