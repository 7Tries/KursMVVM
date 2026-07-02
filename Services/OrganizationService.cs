using KursMVVM.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KursMVVM.Services;

public class OrganizationService : IDBService<Organization>
{
    public async Task<List<Organization>> GetAllAsync()
    {
        using var db = new KursContext();
        return await db.Organizations.ToListAsync();
    }

    public async Task<Organization?> GetByIdAsync(object id)
    {
        using var db = new KursContext();
        return await db.Organizations.FindAsync(id);
    }

    public async Task AddAsync(Organization entity)
    {
        using var db = new KursContext();
        db.Organizations.Add(entity);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Organization entity)
    {
        using var db = new KursContext();
        db.Organizations.Update(entity);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(object id)
    {
        using var db = new KursContext();
        var entity = await db.Organizations.FindAsync(id);
        if (entity != null)
        {
            db.Organizations.Remove(entity);
            await db.SaveChangesAsync();
        }
    }
}
