using System;

using ProtoBuf;

namespace Modix.Web.Protocol.Diagnostics
{
    [ProtoContract]
    public class HeartbeatTick
        : HeartbeatResponse
    {
        [ProtoMember(1)]
        public DateTimeOffset Received { get; init; }
    }
}
