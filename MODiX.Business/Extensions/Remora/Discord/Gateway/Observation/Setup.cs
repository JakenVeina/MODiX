using System;
using System.Reactive.Subjects;

using Microsoft.Extensions.DependencyInjection;

using Remora.Discord.API.Abstractions.Gateway.Bidirectional;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.Gateway.Extensions;

namespace Remora.Discord.Gateway.Observation
{
    public static class Setup
    {
        public static IServiceCollection AddGatewayObservation(this IServiceCollection services)
            => services
                .AddGatewayObservation<IHeartbeatAcknowledge>()
                .AddGatewayObservation<IHello>()
                .AddGatewayObservation<IChannelDelete>()
                .AddGatewayObservation<IGuildDelete>()
                .AddGatewayObservation<IMessageDelete>()
                .AddGatewayObservation<IMessageReactionAdd>()
                .AddGatewayObservation<IMessageReactionRemove>();

        public static IServiceCollection AddGatewayObservation<TGatewayEvent>(this IServiceCollection services)
                where TGatewayEvent : IGatewayEvent
            => services
                .AddSingleton<Subject<TGatewayEvent>>()
                .AddSingleton<IObserver<TGatewayEvent?>, Subject<TGatewayEvent?>>(serviceProvider => serviceProvider.GetRequiredService<Subject<TGatewayEvent?>>())
                .AddSingleton<IObservable<TGatewayEvent?>, Subject<TGatewayEvent?>>(serviceProvider => serviceProvider.GetRequiredService<Subject<TGatewayEvent?>>())
                .AddResponder<ObservingResponder<TGatewayEvent>>();
    }
}
