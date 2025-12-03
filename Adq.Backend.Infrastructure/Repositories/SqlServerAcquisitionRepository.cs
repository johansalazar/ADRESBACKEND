using Adq.Backend.Domain.Models;
using Adq.Backend.Domain.Ports;
using Adq.Backend.Infrastructure.DbContexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Adq.Backend.Infrastructure.Repositories
{
    public class SqlServerAcquisitionRepository : IAcquisitionRepository
    {
        private readonly AcquisitionDbContext _db;

        public SqlServerAcquisitionRepository(AcquisitionDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Acquisition>> GetAllAsync()
            => await _db.Acquisitions.AsNoTracking().ToListAsync();

        public async Task<Acquisition?> GetByIdAsync(Guid id)
            => await _db.Acquisitions.FindAsync(id);

        public async Task<IEnumerable<Acquisition>> QueryAsync(Func<Acquisition, bool> predicate)
            => _db.Acquisitions.AsNoTracking().Where(predicate).ToList();

        public async Task SaveAsync(Acquisition a)
        {
            var exists = await _db.Acquisitions.AnyAsync(x => x.Id == a.Id);

            if (!exists)
                _db.Acquisitions.Add(a);
            else
                _db.Acquisitions.Update(a);

            await _db.SaveChangesAsync();
        }

        public async Task DeactivateAsync(Guid id, string reason)
        {
            var a = await _db.Acquisitions.FindAsync(id);
            if (a == null) return;

            a.Active = false;

            _db.History.Add(new HistoryEntry
            {
                AcquisitionId = id,
                Action = "Deactivate",
                Payload = reason
            });

            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<HistoryEntry>> GetHistoryAsync(Guid id)
        => await _db.History.Where(h => h.AcquisitionId == id).ToListAsync();

        public async Task AddHistoryAsync(HistoryEntry entry)
        {
            var exists = await _db.History.AnyAsync(x => x.Id == entry.Id);

            if (!exists)
                _db.History.Add(entry);
            else
                _db.History.Update(entry);

            await _db.SaveChangesAsync();
           
        }
    }
}
