using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursMVVM.Services;

public class ContractService : IDBService<Contract>
{
    public async Task<List<Contract>> GetAllAsync()
    {
        using var db = new KursContext();
        return await db.Contracts
            .Include(c => c.Employee)
            .Include(c => c.Supply)
            .ToListAsync();
    }

    public async Task<Contract?> GetByIdAsync(object id)
    {
        using var db = new KursContext();
        return await db.Contracts.FindAsync(id);
    }

    public async Task AddAsync(Contract entity)
    {
        using var db = new KursContext();
        db.Contracts.Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Contract entity)
    {
        using var db = new KursContext();
        db.Contracts.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(object id)
    {
        using var db = new KursContext();
        var entity = await db.Contracts.FindAsync(id);
        if (entity != null)
        {
            db.Contracts.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
