using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.PlatformServices;
using System.Reactive.Subjects;

using Modix.Web.Protocol.Diagnostics;

namespace Modix.Web.Client.Diagnostics
{
    public class DiagnosticsViewModel
    {
        public DiagnosticsViewModel(
            IDiagnosticsContract diagnosticsContract,
            ISystemClock systemClock)
        {
            _pingTestStartRequested = new Subject<Unit>();

            var heartbeatState = Observable.CombineLatest(
                    diagnosticsContract
                        .ObserveHeartbeat()
                        .ToObservable()
                        .Scan(
                            (
                                lastTickReceived:   default(DateTimeOffset?),
                                targetInterval:     default(TimeSpan?)
                            ),
                            (state, response) => response switch
                            {
                                HeartbeatInitialization initialization  =>
                                (
                                    lastTickReceived:   state.lastTickReceived,
                                    targetInterval:     initialization.HeartbeatInterval
                                ),
                                HeartbeatTick lastTick =>
                                (
                                    lastTickReceived:   lastTick.Received,
                                    targetInterval:     state.targetInterval
                                ),
                                _ => state
                            }),
                    Observable.Timer(
                            dueTime:    TimeSpan.Zero,
                            period:     TimeSpan.FromMilliseconds(100))
                        .Select(_ => systemClock.UtcNow.TruncateMilliseconds())
                        .DistinctUntilChanged(),
                    (state, now) =>
                    (
                        tickInterval:   now - state.lastTickReceived,
                        targetInterval: state.targetInterval
                    ))
                .ShareReplay(1);

            _hasHeartbeatIntervalElapsed = heartbeatState
                .Select(state => state.tickInterval > state.targetInterval);

            _heartbeatTickInterval = heartbeatState
                .Select(state => state.tickInterval);

            _pingTestStates = _pingTestStartRequested
                .Select(_ => diagnosticsContract.PerformPingTest()
                    .ToObservable())
                .Switch()
                .Scan(ImmutableList<PingTestState>.Empty, (states, response) => response switch
                {
                    PingTestDefinitions definitions => definitions.EndpointNames
                        .Select(endpointName => new PingTestState(
                            endpointName: endpointName,
                            hasCompleted: false,
                            latency: null,
                            status: EndpointStatus.Unknown))
                        .ToImmutableList(),
                    PingTestOutcome outcome => states.SetItem(
                        index: states.FindIndex(0, state => state.EndpointName == outcome.EndpointName),
                        value: new PingTestState(
                            endpointName: outcome.EndpointName,
                            hasCompleted: true,
                            latency: outcome.Latency,
                            status: outcome.Status)),
                    _ => states
                })
                .Share();

            _isPingTestRunning = _pingTestStates
                .Select(states => states.Any(state => !state.HasCompleted));
        }

        public IObservable<bool> HasHeartbeatIntervalElapsed
            => _hasHeartbeatIntervalElapsed;

        public IObservable<TimeSpan?> HeartbeatTickInterval
            => _heartbeatTickInterval;

        public IObservable<bool> IsPingTestRunning
            => _isPingTestRunning;

        public IObservable<ImmutableList<PingTestState>> PingTestStates
            => _pingTestStates;

        public void StartPingTest()
            => _pingTestStartRequested.OnNext(Unit.Default);

        private readonly IObservable<bool> _hasHeartbeatIntervalElapsed;
        private readonly IObservable<bool> _isPingTestRunning;
        private readonly IObservable<ImmutableList<PingTestState>> _pingTestStates;
        private readonly Subject<Unit> _pingTestStartRequested;
        private readonly IObservable<TimeSpan?> _heartbeatTickInterval;
    }
}
