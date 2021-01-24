using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;

using Modix.Business.Diagnostics;
using Modix.Web.Protocol.Diagnostics;

using Remora.Discord.API.Abstractions.Gateway.Bidirectional;

namespace Modix.Web.Server.Diagnostics
{
    public class DiagnosticsContract
        : IDiagnosticsContract
    {
        public DiagnosticsContract(
            IDiagnosticsManager diagnosticsManager,
            IDiagnosticsService diagnosticsService,
            IObservable<IHeartbeatAcknowledge?> heartbeatAcknowledged,
            ISystemClock systemClock)
        {
            _diagnosticsManager = diagnosticsManager;
            _diagnosticsService = diagnosticsService;
            _heartbeatAcknowledged = heartbeatAcknowledged;
            _systemClock = systemClock;
        }

        public IAsyncEnumerable<HeartbeatResponse> ObserveHeartbeat()
            => Observable.Merge(
                    _diagnosticsManager.HeartbeatInterval
                        .Select(heartbeatInterval => new HeartbeatInitialization()
                        {
                            HeartbeatInterval = heartbeatInterval
                        })
                        .Cast<HeartbeatResponse>(),
                    _heartbeatAcknowledged
                        .Select(_ => new HeartbeatTick()
                        {
                            Received = _systemClock.UtcNow
                        })
                        .Cast<HeartbeatResponse>())
                .ToAsyncEnumerable();

        public IAsyncEnumerable<PingTestResponse> PerformPingTest()
            => AsyncEnumerable.Empty<PingTestResponse>()
                .Append(new PingTestDefinitions()
                {
                    EndpointNames = _diagnosticsService.PingTestEndpointNames
                })
                .Concat(_diagnosticsService.PerformPingTest());

        private readonly IDiagnosticsManager _diagnosticsManager;
        private readonly IDiagnosticsService _diagnosticsService;
        private readonly IObservable<IHeartbeatAcknowledge?> _heartbeatAcknowledged;
        private readonly ISystemClock _systemClock;
    }
}
