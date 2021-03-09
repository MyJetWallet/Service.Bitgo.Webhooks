using System.Runtime.Serialization;
using Service.Bitgo.Webhooks.Domain.Models;

namespace Service.Bitgo.Webhooks.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}