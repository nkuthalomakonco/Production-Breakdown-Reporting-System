using BreakdownManager.Domain.Entities;

namespace BreakdownManager.Services.Interfaces;

public interface IMachineService
{
    Task<List<Machine>> GetActiveMachinesAsync();
    Task<Machine?> GetByIdAsync(int id);
    Task<Machine> AddMachineAsync(Machine machine);
}
