using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursMVVM.Services;

public class EmployeeService : IDBService<Employee>
{
    public async Task<List<Employee>> GetAllAsync()
    {
        using var db = new KursContext();
        return await db.Employees.ToListAsync();
    }

    public async Task<Employee?> GetByIdAsync(object id)
    {
        using var db = new KursContext();
        return await db.Employees.FindAsync(id);
    }

    public async Task AddAsync(Employee entity)
    {
        using var db = new KursContext();
        db.Employees.Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Employee entity)
    {
        using var db = new KursContext();
        db.Employees.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(object id)
    {
        using var db = new KursContext();
        var entity = await db.Employees.FindAsync(id);
        if (entity != null)
        {
            db.Employees.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
