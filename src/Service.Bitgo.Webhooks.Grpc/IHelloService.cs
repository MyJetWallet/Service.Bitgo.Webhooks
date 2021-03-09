using System.ServiceModel;
using System.Threading.Tasks;
using Service.Bitgo.Webhooks.Grpc.Models;

namespace Service.Bitgo.Webhooks.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}