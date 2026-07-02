using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursMVVM.Services;

public class SupplyService : IDBService<Supply>
{
    public async Task<List<Supply>> GetAllAsync()
    {
        using var db = new KursContext();
        return await db.Supplies
            .Include(s => s.Employee)
            .Include(s => s.Contract)
            .ToListAsync();
    }

    public async Task<Supply?> GetByIdAsync(object id)
    {
        using var db = new KursContext();
        return await db.Supplies.FindAsync(id);
    }

    public async Task AddAsync(Supply entity)
    {
        using var db = new KursContext();
        db.Supplies.Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Supply entity)
    {
        using var db = new KursContext();
        db.Supplies.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(object id)
    {
        using var db = new KursContext();
        var entity = await db.Supplies.FindAsync(id);
        if (entity != null)
        {
            db.Supplies.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
