using Microsoft.EntityFrameworkCore;
using KursMVVM.API.Models.Abstractions;
using KursMVVM.API.Models.Data;

namespace KursMVVM.API.Models.Services;

public class OrganizationService : AbstractionService, ICommonService<Organization, int>
{
    private readonly int _pageSize = 10;
    private readonly KursContext _db;

    public OrganizationService(KursContext db)
    {
        _db = db;
    }

    public bool Create(Organization model)
    {
        return DoAction(() =>
        {
            _db.Organizations.Add(model);
            _db.SaveChanges();
        });
    }

    public bool Delete(int id)
    {
        return DoAction(() =>
        {
            var entity = _db.Organizations.FirstOrDefault(o => o.Id == id);
            if (entity == null) throw new Exception("Not found");
            _db.Organizations.Remove(entity);
            _db.SaveChanges();
        });
    }

    public async Task<Organization?> Get(int id)
    {
        return await _db.Organizations.FindAsync(id);
    }

    public async Task<IEnumerable<Organization>> GetAll()
    {
        return await _db.Organizations.ToListAsync();
    }

    public bool Update(int id, Organization model)
    {
        return DoAction(() =>
        {
            var entity = _db.Organizations.FirstOrDefault(o => o.Id == id);
            if (entity == null) throw new Exception("Not found");
            entity.ContractNumber = model.ContractNumber;
            entity.CountryCode = model.CountryCode;
            entity.City = model.City;
            entity.Address = model.Address;
            entity.Phone = model.Phone;
            entity.Email = model.Email;
            entity.Website = model.Website;
            _db.Organizations.Update(entity);
            _db.SaveChanges();
        });
    }

    public async Task<IEnumerable<Organization>> GetPage(int? page)
    {
        return await _db.Organizations
            .OrderBy(o => o.Id)
            .Skip(((page ?? 1) - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();
    }
}
