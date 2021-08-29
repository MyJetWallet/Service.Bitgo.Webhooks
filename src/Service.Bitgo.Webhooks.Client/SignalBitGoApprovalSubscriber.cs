using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Domain.Models;

namespace Service.Bitgo.Webhooks.Client
{
    public class SignalBitGoApprovalSubscriber : ISubscriber<SignalBitGoPendingApproval>
    {
        private readonly List<Func<SignalBitGoPendingApproval, ValueTask>> _list = new List<Func<SignalBitGoPendingApproval, ValueTask>>();

        public SignalBitGoApprovalSubscriber(
            MyServiceBusTcpClient client,
            string queueName,
            TopicQueueType queryType)
        {
            client.Subscribe(SignalBitGoPendingApproval.ServiceBusTopicName, queueName, queryType, Handler);
        }

        private async ValueTask Handler(IMyServiceBusMessage data)
        {
            var item = Deserializer(data.Data);

            if (!_list.Any())
            {
                throw new Exception("Cannot handle event. No subscribers");
            }

            foreach (var callback in _list)
            {
                await callback.Invoke(item);
            }
        }


        public void Subscribe(Func<SignalBitGoPendingApproval, ValueTask> callback)
        {
            this._list.Add(callback);
        }

        private SignalBitGoPendingApproval Deserializer(ReadOnlyMemory<byte> data) => data.ByteArrayToServiceBusContract<SignalBitGoPendingApproval>();
    }
}