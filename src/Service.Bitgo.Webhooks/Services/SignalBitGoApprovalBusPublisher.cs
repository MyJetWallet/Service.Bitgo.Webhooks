using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Domain.ServiceBus.Serializers;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Domain.Models;

namespace Service.Bitgo.Webhooks.Services
{
    public class SignalBitGoApprovalBusPublisher : IPublisher<SignalBitGoPendingApproval>
    {
        private readonly MyServiceBusTcpClient _client;

        public SignalBitGoApprovalBusPublisher(MyServiceBusTcpClient client)
        {
            _client = client;
            _client.CreateTopicIfNotExists(SignalBitGoPendingApproval.ServiceBusTopicName);
        }

        public async ValueTask PublishAsync(SignalBitGoPendingApproval valueToPublish)
        {
            var bytesToSend = valueToPublish.ServiceBusContractToByteArray();
            await _client.PublishAsync(SignalBitGoPendingApproval.ServiceBusTopicName, bytesToSend, true);
        }
    }
}