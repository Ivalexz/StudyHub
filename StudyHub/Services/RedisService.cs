using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StudyHub.Data;
using StudyHub.Models;
using StudyHub.ViewModels;

namespace StudyHub.Services
{
    public class RedisService
    {
        private readonly StudyHubContext _db;
        private readonly IDistributedCache _cache;

        public RedisService(StudyHubContext db, IDistributedCache cache)
        {
            _db = db;
            _cache = cache;
        }
        
        private async Task SetCacheAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
            };

            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, json, options);
        }


        private async Task<T?> GetCacheAsync<T>(string key)
        {
            var cached = await _cache.GetStringAsync(key);
            if (cached == null)
                return default;

            return JsonSerializer.Deserialize<T>(cached);
        }

        // 1. Затверджені нотатки
        public async Task<List<NoteViewModel>> GetApprovedNotesAsync()
        {
            var cacheKey = "notes_feed";
            var notes = await GetCacheAsync<List<NoteViewModel>>(cacheKey);

            if (notes == null)
            {
                notes = await _db.Notes
                    .Where(n => n.Status == NoteStatus.Approved)
                    .Include(n => n.Subject)
                    .Select(n => new NoteViewModel
                    {
                        Title = n.Title,
                        Content = n.Content,
                        SubjectName = n.Subject.Name,
                        AuthorId = n.AuthorId,
                        AuthorName = n.AuthorName
                    })
                    .ToListAsync();

                await SetCacheAsync(cacheKey, notes, TimeSpan.FromMinutes(10));
            }

            return notes;
        }

        // 2. Топ‑автори
        public async Task<List<TopAuthorViewModel>> GetTopAuthorsAsync()
        {
            var cacheKey = "top_authors";
            var authors = await GetCacheAsync<List<TopAuthorViewModel>>(cacheKey);

            if (authors == null)
            {
                authors = await _db.Notes
                    .Where(n => n.Status == NoteStatus.Approved)
                    .GroupBy(n => new { n.AuthorId, n.AuthorName })
                    .Select(g => new TopAuthorViewModel
                    {
                        AuthorId = g.Key.AuthorId,
                        AuthorName = g.Key.AuthorName,
                        NoteCount = g.Count()
                    })
                    .OrderByDescending(a => a.NoteCount)
                    .Take(3)
                    .ToListAsync();

                await SetCacheAsync(cacheKey, authors, TimeSpan.FromMinutes(10));
            }

            return authors;
        }

        // 3. Pending‑нотатки
        public async Task<int> GetPendingCountAsync()
        {
            var cacheKey = "pending_count";
            var count = await GetCacheAsync<int>(cacheKey);

            if (count == 0)
            {
                count = await _db.Notes.CountAsync(n => n.Status == NoteStatus.Pending);
                await SetCacheAsync(cacheKey, count, TimeSpan.FromMinutes(2));
            }

            return count;
        }


        public async Task ClearCacheAsync()
        {
            await _cache.RemoveAsync("notes_feed");
            await _cache.RemoveAsync("top_authors");
            await _cache.RemoveAsync("pending_count");
        }
    }
}
