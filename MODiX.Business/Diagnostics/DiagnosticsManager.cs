using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Modix.Business.Diagnostics
{
    public interface IDiagnosticsManager
    {
        IObservable<TimeSpan> HeartbeatInterval { get; }
    }

    internal class DiagnosticsManager
        : DisposableBase,
            IDiagnosticsManager
    {
        public DiagnosticsManager()
        {
            _heartbeatIntervalSource = new();
            _heartbeatInterval = _heartbeatIntervalSource
                .Replay(1);
        }

        public IObservable<TimeSpan> HeartbeatInterval
            => _heartbeatInterval;

        public void OnHeartbeatIntervalChanged(TimeSpan heartbeatInterval)
            => _heartbeatIntervalSource.OnNext(heartbeatInterval);

        private readonly IObservable<TimeSpan> _heartbeatInterval;
        private readonly Subject<TimeSpan> _heartbeatIntervalSource;
    }
}
