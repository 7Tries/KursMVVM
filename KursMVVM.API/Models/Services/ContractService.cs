using Microsoft.EntityFrameworkCore;
using KursMVVM.API.Models.Abstractions;
using KursMVVM.API.Models.Data;

namespace KursMVVM.API.Models.Services;

public class ContractService : AbstractionService, ICommonService<Contract, string>
{
    private readonly int _pageSize = 10;
    private readonly KursContext _db;

    public ContractService(KursContext db)
    {
        _db = db;
    }

    public bool Create(Contract model)
    {
        return DoAction(() =>
        {
            _db.Contracts.Add(model);
            _db.SaveChanges();
        });
    }

    public bool Delete(string id)
    {
        return DoAction(() =>
        {
            var entity = _db.Contracts.FirstOrDefault(c => c.ContractNumber == id);
            if (entity == null) throw new Exception("Not found");
            _db.Contracts.Remove(entity);
            _db.SaveChanges();
        });
    }

    public async Task<Contract?> Get(string id)
    {
        return await _db.Contracts.FindAsync(id);
    }

    public async Task<IEnumerable<Contract>> GetAll()
    {
        return await _db.Contracts.ToListAsync();
    }

    public bool Update(string id, Contract model)
    {
        return DoAction(() =>
        {
            var entity = _db.Contracts.FirstOrDefault(c => c.ContractNumber == id);
            if (entity == null) throw new Exception("Not found");
            entity.EmployeeId = model.EmployeeId;
            entity.OrganizationName = model.OrganizationName;
            entity.Date = model.Date;
            _db.Contracts.Update(entity);
            _db.SaveChanges();
        });
    }

    public async Task<IEnumerable<Contract>> GetPage(int? page)
    {
        return await _db.Contracts
            .OrderBy(c => c.ContractNumber)
            .Skip(((page ?? 1) - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();
    }
}
