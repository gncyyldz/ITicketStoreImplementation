using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

namespace ITicketStoreImplementation.Options
{
    public class ConfigureCookieAuthenticationOptions(ITicketStore ticketStore) : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        public void PostConfigure(string? name, CookieAuthenticationOptions options)
            => options.SessionStore = ticketStore;
    }
}
