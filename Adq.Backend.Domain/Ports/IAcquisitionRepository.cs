using Adq.Backend.Domain.Models;

namespace Adq.Backend.Domain.Ports
{
    public interface IAcquisitionRepository
    {
        Task<Acquisition?> GetByIdAsync(Guid id);
        Task<IEnumerable<Acquisition>> GetAllAsync();
        Task<IEnumerable<Acquisition>> QueryAsync(Func<Acquisition, bool> predicate);
        Task SaveAsync(Acquisition a); //crear o actualizar
        Task DeactivateAsync(Guid id, string reason);
        Task<IEnumerable<HistoryEntry>> GetHistoryAsync(Guid id);
        Task AddHistoryAsync(HistoryEntry entry);
    }
}
