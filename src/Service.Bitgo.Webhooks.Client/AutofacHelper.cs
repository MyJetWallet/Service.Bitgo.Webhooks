using Autofac;
using DotNetCoreDecorators;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Domain.Models;
// ReSharper disable UnusedMember.Global

namespace Service.Bitgo.Webhooks.Client
{
    public static class AutofacHelper
    {
        public static void RegisterSignalBitGoTransferSubscriber(this ContainerBuilder builder,
            MyServiceBusTcpClient client,
            string queueName,
            TopicQueueType queryType)
        {
            var subs = new SignalBitGoTransferSubscriber(client, queueName, queryType);

            builder
                .RegisterInstance(subs)
                .As<ISubscriber<SignalBitGoTransfer>>()
                .SingleInstance();
        }
    }
}