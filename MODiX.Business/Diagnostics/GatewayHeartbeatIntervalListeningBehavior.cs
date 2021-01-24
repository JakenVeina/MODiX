using System;
using System.Reactive.Linq;

using Microsoft.Extensions.Hosting;

using Remora.Discord.API.Abstractions.Gateway.Events;

namespace Modix.Business.Diagnostics
{
    internal class GatewayHeartbeatIntervalListeningBehavior
        : BehaviorBase
    {
        public GatewayHeartbeatIntervalListeningBehavior(
            DiagnosticsManager diagnosticsManager,
            IObservable<IHello?> helloReceived)
        {
            _diagnosticsManager = diagnosticsManager;
            _helloReceived = helloReceived;
        }

        protected override IDisposable Start()
            => _helloReceived
                .WhereNotNull()
                .Select(@event => @event.HeartbeatInterval)
                .DistinctUntilChanged()
                .Subscribe(heartbeatInterval => _diagnosticsManager.OnHeartbeatIntervalChanged(heartbeatInterval));

        private readonly DiagnosticsManager _diagnosticsManager;
        private readonly IObservable<IHello?> _helloReceived;
    }
}
