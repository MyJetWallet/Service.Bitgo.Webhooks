using System.Runtime.Serialization;

namespace Service.Bitgo.Webhooks.Domain.Models
{
    [DataContract]
    public class SignalBitGoPendingApproval
    {
        public const string ServiceBusTopicName = "bitgo-pending-approval-signal";

        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public string PendingApprovalId { get; set; }
    }
}