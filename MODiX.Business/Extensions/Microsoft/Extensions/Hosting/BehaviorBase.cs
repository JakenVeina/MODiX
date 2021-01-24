using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public abstract class BehaviorBase
        : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _stopToken = Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _stopToken?.Dispose();
            _stopToken = null;
            return Task.CompletedTask;
        }

        protected abstract IDisposable Start();

        private IDisposable? _stopToken;
    }
}
