using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Diagnost.Misc
{
    public class BrowserCookieHandler : DelegatingHandler
    {
        public BrowserCookieHandler()
        {
            InnerHandler = new HttpClientHandler();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Магия для WASM: включаем отправку кук (Credentials)
            if (OperatingSystem.IsBrowser())
            {
                request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
