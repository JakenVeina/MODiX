using System;

using ProtoBuf;

namespace Modix.Web.Protocol.Diagnostics
{
    [ProtoContract]
    public class HeartbeatInitialization
        : HeartbeatResponse
    {
        [ProtoMember(1, DataFormat = DataFormat.WellKnown)]
        public TimeSpan HeartbeatInterval { get; init; }
    }
}
