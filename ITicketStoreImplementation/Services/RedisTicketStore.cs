using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;
using System.Security.Cryptography;

namespace ITicketStoreImplementation.Services
{
    public class RedisTicketStore : ITicketStore
    {
        // Redis bağlantısı için IDatabase instance'ını tutar.
        private readonly IDatabase _database;
        // Ticket'ın default geçerlilik süresi 8 saat olarak ayarlanmıştır. Eğer ticket'ın Properties.ExpiresUtc değeri null ise, bu default süre kullanılır.
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(8);

        public RedisTicketStore(IConnectionMultiplexer connectionMultiplexer)
            => _database = connectionMultiplexer.GetDatabase();

        // Verilen key'e ait ticket'ı Redis'ten siler.
        public async Task RemoveAsync(string key)
            => await _database.KeyDeleteAsync(key);

        // Ticket'ı günceller/yeniler ve ticket'ın bitiş zamanını hesaplar, kalan süreyi bulur. Süre dolmuşsa kaydı siler; dolmamışsa bileti kalan süre kadar bir TTL (yaşam süresi) ile Redis'e yazar.
        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var serializedTicket = TicketSerializer.Default.Serialize(ticket);
            var expiration = ticket.Properties.ExpiresUtc ?? DateTimeOffset.UtcNow.Add(_defaultExpiration);
            var remainingTime = expiration - DateTimeOffset.UtcNow;

            if (remainingTime <= TimeSpan.Zero)
            {
                await _database.KeyDeleteAsync(key);
                return;
            }

            await _database.StringSetAsync(key, serializedTicket, remainingTime);
        }
        // Verilen key ile Redis'ten ticket'ı okur, boşsa null döner, varsa deserialize edip AuthenticationTicket olarak geri verir.
        public async Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            var value = await _database.StringGetAsync(key);

            if (value.IsNullOrEmpty)
                return null;

            return TicketSerializer.Default.Deserialize(value!);
        }

        // RandomNumberGenerator ile kriptografik olarak güvenli, rastgele 32 byte'lık bir key üretip, bu key eşliğinde AuthenticationTicket'ı Redis'e kaydeder ve key'i döndürür.
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            await RenewAsync(key, ticket);

            return key;
        }
    }
}
