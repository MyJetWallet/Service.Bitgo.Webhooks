using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using ProtoBuf.Grpc.Client;
using Service.Bitgo.Webhooks.Grpc;

namespace Service.Bitgo.Webhooks.Client
{
    [UsedImplicitly]
    public class Bitgo.WebhooksClientFactory
    {
        private readonly CallInvoker _channel;

        public Bitgo.WebhooksClientFactory(string assetsDictionaryGrpcServiceUrl)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(assetsDictionaryGrpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IHelloService GetHelloService() => _channel.CreateGrpcService<IHelloService>();
    }
}
