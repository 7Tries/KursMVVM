using Microsoft.EntityFrameworkCore;
using KursMVVM.API.Models.Abstractions;
using KursMVVM.API.Models.Data;

namespace KursMVVM.API.Models.Services;

public class SupplyService : AbstractionService, ICommonService<Supply, string>
{
    private readonly int _pageSize = 10;
    private readonly KursContext _db;

    public SupplyService(KursContext db)
    {
        _db = db;
    }

    public bool Create(Supply model)
    {
        return DoAction(() =>
        {
            _db.Supplies.Add(model);
            _db.SaveChanges();
        });
    }

    public bool Delete(string id)
    {
        return DoAction(() =>
        {
            var entity = _db.Supplies.FirstOrDefault(s => s.ContractNumber == id);
            if (entity == null) throw new Exception("Not found");
            _db.Supplies.Remove(entity);
            _db.SaveChanges();
        });
    }

    public async Task<Supply?> Get(string id)
    {
        return await _db.Supplies.FindAsync(id);
    }

    public async Task<IEnumerable<Supply>> GetAll()
    {
        return await _db.Supplies.ToListAsync();
    }

    public bool Update(string id, Supply model)
    {
        return DoAction(() =>
        {
            var entity = _db.Supplies.FirstOrDefault(s => s.ContractNumber == id);
            if (entity == null) throw new Exception("Not found");
            entity.EquipmentType = model.EquipmentType;
            entity.Comment = model.Comment;
            entity.EmployeeId = model.EmployeeId;
            _db.Supplies.Update(entity);
            _db.SaveChanges();
        });
    }

    public async Task<IEnumerable<Supply>> GetPage(int? page)
    {
        return await _db.Supplies
            .OrderBy(s => s.ContractNumber)
            .Skip(((page ?? 1) - 1) * _pageSize)
            .Take(_pageSize)
            .ToListAsync();
    }
}
