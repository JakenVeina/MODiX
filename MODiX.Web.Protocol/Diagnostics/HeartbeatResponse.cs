using ProtoBuf;

namespace Modix.Web.Protocol.Diagnostics
{
    [ProtoContract]
    [ProtoInclude(1, typeof(HeartbeatInitialization))]
    [ProtoInclude(2, typeof(HeartbeatTick))]
    public abstract class HeartbeatResponse { }
}
