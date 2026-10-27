using Microsoft.EntityFrameworkCore;
using KursMVVM.API.Models.Abstractions;
using KursMVVM.API.Models.Data;

namespace KursMVVM.API.Models.Services;

public class EmployeeService : AbstractionService, ICommonService<Employee, int>
{
    private readonly int _pageSize = 10;
    private readonly KursContext _db;

    public EmployeeService(KursContext db)
    {
        _db = db;
    }

    public bool Create(Employee model)
    {
        return DoAction(() =>
        {
            _db.Employees.Add(model);
            _db.SaveChanges();
        });
    }

    public bool Delete(int id)
    {
        return DoAction(() =>
        {
            var entity = _db.Employees.FirstOrDefault(e => e.Id == id);
            if (entity == null) throw new Exception("Not found");
            _db.Employees.Remove(entity);
            _db.SaveChanges();
        });
    }

    public async Task<Employee?> Get(int id)
    {
        return await _db.Employees.FindAsync(id);
    }

    public async Task<IEnumerable<Employee>> GetAll()
    {
        return await _db.Employees.ToListAsync();
    }

    public bool Update(int id, Employee model)
    {
        return DoAction(() =>
        {
            var entity = _db.Employees.FirstOrDefault(e => e.Id == id);
            if (entity == null) throw new Exception("Not found");
            entity.DepartmentCode = model.DepartmentCode;
            entity.FullName = model.FullName;
            entity.Position = model.Position;
            entity.Salary = model.Salary;
            entity.Bonus = model.Bonus;
            entity.Month = model.Month;
            _db.Employees.Update(entity);
            _db.SaveChanges();
        });
    }

    public async Task<IEnumerable<Employee>> GetPage(int? page)
    {
        return await _db.Employees
            .OrderBy(e => e.Id)
            .Skip(((page ?? 1) - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();
    }
}
