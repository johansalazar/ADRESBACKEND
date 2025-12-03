using Adq.Backend.Domain.Models;
using Adq.Backend.Domain.Ports;
using System.Text.Json;

namespace Adq.Backend.Infrastructure.Repositories
{
    public class FileAcquisitionRepository : IAcquisitionRepository
    {
        private readonly string _filePath;
        private static readonly SemaphoreSlim _locker = new(1, 1);

        public FileAcquisitionRepository(string filePath)
        {
            _filePath = filePath;
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            if (!File.Exists(_filePath)) File.WriteAllText(_filePath, "");
        }

        public async Task SaveAsync(Acquisition a)
        {
            var entry = new HistoryEntry
            {
                AcquisitionId = a.Id,
                Action = "upsert",
                Timestamp = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(a)
            };
            var line = JsonSerializer.Serialize(entry);
            await _locker.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_filePath, line + Environment.NewLine);
            }
            finally { _locker.Release(); }
        }

        public async Task DeactivateAsync(Guid id, string reason)
        {
            var entry = new HistoryEntry
            {
                AcquisitionId = id,
                Action = "deactivate",
                Timestamp = DateTime.UtcNow,
                Payload = reason
            };
            var line = JsonSerializer.Serialize(entry);
            await _locker.WaitAsync();
            try { await File.AppendAllTextAsync(_filePath, line + Environment.NewLine); }
            finally { _locker.Release(); }
        }

        public async Task<IEnumerable<HistoryEntry>> GetHistoryAsync(Guid id)
        {
            var list = new List<HistoryEntry>();
            await foreach (var e in ReadEntriesAsync())
            {
                if (e.AcquisitionId == id) list.Add(e);
            }
            return list.OrderBy(x => x.Timestamp);
        }

        public async Task<IEnumerable<Acquisition>> GetAllAsync()
        {
            var dict = await RebuildAllAsync();
            return dict.Values;
        }

        public async Task<Acquisition?> GetByIdAsync(Guid id)
        {
            var dict = await RebuildAllAsync();
            return dict.TryGetValue(id, out var a) ? a : null;
        }

        public async Task<IEnumerable<Acquisition>> QueryAsync(Func<Acquisition, bool> predicate)
        {
            var all = await GetAllAsync();
            return all.Where(predicate);
        }

        private async IAsyncEnumerable<HistoryEntry> ReadEntriesAsync()
        {
            using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var sr = new StreamReader(fs);
            string? line;
            while ((line = await sr.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                HistoryEntry? he = null;
                try { he = JsonSerializer.Deserialize<HistoryEntry>(line); } catch { }
                if (he != null) yield return he;
            }
        }

        private async Task<Dictionary<Guid, Acquisition>> RebuildAllAsync()
        {
            var dict = new Dictionary<Guid, Acquisition>();
            await foreach (var entry in ReadEntriesAsync())
            {
                if (entry.Action == "upsert")
                {
                    try
                    {
                        var a = JsonSerializer.Deserialize<Acquisition>(entry.Payload);
                        if (a != null) dict[a.Id] = a;
                    }
                    catch { /* ignore malformed */ }
                }
                else if (entry.Action == "deactivate")
                {
                    if (dict.TryGetValue(entry.AcquisitionId, out var a))
                    {
                        a.Active = false;
                    }
                }
            }
            return dict;
        }

        public async Task AddHistoryAsync(HistoryEntry a)
        {
            var entry = new HistoryEntry
            {
                AcquisitionId = a.Id,
                Action = "upsert",
                Timestamp = DateTime.UtcNow,
                Payload = JsonSerializer.Serialize(a)
            };
            var line = JsonSerializer.Serialize(entry);
            await _locker.WaitAsync();
            try
            {
                await File.AppendAllTextAsync(_filePath, line + Environment.NewLine);
            }
            finally { _locker.Release(); 
            }
        }
    }
}