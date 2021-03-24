using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Domain.Models;

namespace Service.Bitgo.Webhooks.Services
{
    public class SignalBitGoTransferBusPublisher : IPublisher<SignalBitGoTransfer>
    {
        private readonly MyServiceBusTcpClient _client;

        public SignalBitGoTransferBusPublisher(MyServiceBusTcpClient client)
        {
            _client = client;
            _client.CreateTopicIfNotExists(SignalBitGoTransfer.ServiceBusTopicName);
        }

        public async ValueTask PublishAsync(SignalBitGoTransfer valueToPublish)
        {
            var bytesToSend = valueToPublish.ServiceBusContractToByteArray();
            await _client.PublishAsync(SignalBitGoTransfer.ServiceBusTopicName, bytesToSend, true);
        }
    }
}