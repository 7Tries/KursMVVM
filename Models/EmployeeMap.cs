using CsvHelper.Configuration;

namespace KursMVVM.Models;

public class EmployeeMap : ClassMap<Employee>
{
    public EmployeeMap()
    {
        Map(m => m.Id).Name("код_сотрудника");
        Map(m => m.DepartmentCode).Name("код_отдела");
        Map(m => m.FullName).Name("фио");
        Map(m => m.Position).Name("должность");
        Map(m => m.Salary).Name("оклад");
        Map(m => m.Bonus).Name("премия");
        Map(m => m.Month).Name("месяц");
    }
}
