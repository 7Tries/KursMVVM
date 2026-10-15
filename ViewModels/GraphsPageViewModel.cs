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

    [ObservableProperty]
    private int _selectedViewModeIndex = 0; // 0 = Все 2x2, 1 = Договоры, 2 = Доля поставок (Pie), 3 = Динамика, 4 = Оклады

    [ObservableProperty]
    private int _totalEquipmentCount = 0;

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

    [RelayCommand]
    private void SetViewMode(string modeIndex)
    {
        if (int.TryParse(modeIndex, out int index))
        {
            SelectedViewModeIndex = index;
        }
    }

    private async Task LoadChartsAsync()
    {
        using var db = new KursContext();

        var empData = await db.Employees.ToListAsync();
        var conData = await db.Contracts.ToListAsync();
        var supData = await db.Supplies.ToListAsync();

        // 1. Договоры по сотрудникам (Столбчатая)
        var contractsByEmp = empData.Select(e => new
        {
            Name = e.FullName.Split(' ')[0],
            Count = conData.Count(c => c.EmployeeId == e.Id)
        }).ToList();

        ContractsByEmployeeSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Name = "Заключено договоров",
                Values = contractsByEmp.Select(x => x.Count).ToArray(),
                Fill = new SolidColorPaint(new SKColor(99, 102, 241)), // Tender Indigo
                Rx = 6,
                Ry = 6
            }
        };

        ContractsByEmployeeXAxes = new[]
        {
            new Axis
            {
                Labels = contractsByEmp.Select(x => x.Name).ToArray(),
                LabelsPaint = new SolidColorPaint(new SKColor(71, 85, 105))
            }
        };

        // 2. Доля поставок по типам оборудования (Круговая диаграмма)
        // ИСПРАВЛЕНИЕ: Не используем длинные внешние подписи (Outer), из-за которых диаграмма сжималась до точки.
        // Используем компактные внутренние подписи (Middle) и цветовую легенду с мягкой нежной палитрой.
        var equipTypes = supData.GroupBy(s => s.EquipmentType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        TotalEquipmentCount = supData.Count;

        var tenderColors = new[]
        {
            new SKColor(99, 102, 241),  // Tender Indigo
            new SKColor(16, 185, 129),  // Tender Emerald
            new SKColor(245, 158, 11),  // Tender Amber
            new SKColor(236, 72, 153),  // Tender Rose
            new SKColor(139, 92, 246),  // Tender Violet
            new SKColor(6, 182, 212),   // Tender Cyan
            new SKColor(249, 115, 22)   // Tender Coral
        };

        int colorIdx = 0;
        EquipmentTypeSeries = equipTypes.Select(e =>
        {
            var sliceColor = tenderColors[colorIdx++ % tenderColors.Length];
            return (ISeries)new PieSeries<int>
            {
                Name = e.Type,
                Values = new[] { e.Count },
                Fill = new SolidColorPaint(sliceColor),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsSize = 13,
                DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue} шт.",
                ToolTipLabelFormatter = point => $"{e.Type}: {point.Coordinate.PrimaryValue} шт. ({point.StackedValue!.Share:P1})",
                InnerRadius = 40, // Изящный Donut-эффект, делающий круг крупным и воздушным
                MaxRadialColumnWidth = 70
            };
        }).ToArray();

        // 3. Динамика договоров по месяцам (Линейный график)
        var dynamics = conData.GroupBy(c => c.Date.ToString("yyyy-MM"))
            .OrderBy(g => g.Key)
            .Select(g => new { Month = g.Key, Count = g.Count() })
            .ToList();

        XAxes = new[]
        {
            new Axis
            {
                Name = "Период",
                Labels = dynamics.Select(x => x.Month).ToArray(),
                LabelsPaint = new SolidColorPaint(new SKColor(71, 85, 105))
            }
        };

        YAxes = new[]
        {
            new Axis
            {
                Name = "Количество",
                LabelsPaint = new SolidColorPaint(new SKColor(71, 85, 105))
            }
        };

        ContractDynamicsSeries = new ISeries[]
        {
            new LineSeries<int>
            {
                Name = "Договоры",
                Values = dynamics.Select(x => x.Count).ToArray(),
                Stroke = new SolidColorPaint(new SKColor(16, 185, 129)) { StrokeThickness = 3 }, // Tender Emerald
                Fill = new SolidColorPaint(new SKColor(16, 185, 129, 40)),
                GeometryFill = new SolidColorPaint(new SKColor(16, 185, 129)),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                GeometrySize = 12
            }
        };

        // 4. Распределение окладов (Столбчатая)
        var salaries = empData.Select(e => (double)e.Salary).ToArray();
        SalaryDistributionSeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Оклады",
                Values = salaries,
                Fill = new SolidColorPaint(new SKColor(139, 92, 246)), // Tender Violet
                Rx = 6,
                Ry = 6
            }
        };
    }
}
