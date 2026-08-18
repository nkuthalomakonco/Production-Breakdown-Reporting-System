using BreakdownManager.Data;
using BreakdownManager.Domain.Entities;
using BreakdownManager.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BreakdownManager.Services;

public class MachineService : IMachineService
{
    private readonly BreakdownManagerDbContext _db;

    public MachineService(BreakdownManagerDbContext db)
    {
        _db = db;
    }

    public async Task<List<Machine>> GetActiveMachinesAsync()
    {
        return await _db.Machines
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Machine?> GetByIdAsync(int id)
    {
        return await _db.Machines.FindAsync(id);
    }

    public async Task<Machine> AddMachineAsync(Machine machine)
    {
        _db.Machines.Add(machine);
        await _db.SaveChangesAsync();
        return machine;
    }
}
