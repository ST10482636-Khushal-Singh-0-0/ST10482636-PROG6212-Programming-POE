using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Tests
{
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _sessionStorage = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear() => _sessionStorage.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _sessionStorage.Remove(key);
        public void Set(string key, byte[] value) => _sessionStorage[key] = value;

        public bool TryGetValue(string key, [NotNullWhen(true)] out byte[]? value)
        {
            return _sessionStorage.TryGetValue(key, out value);
        }
    }
}