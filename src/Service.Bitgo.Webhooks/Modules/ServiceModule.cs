using Autofac;
using Autofac.Core;
using Autofac.Core.Registration;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Settings;
using SimpleTrading.SettingsReader;

namespace Service.Bitgo.Webhooks.Modules
{
    public class ServiceModule: Module
    {
        public static ILogger ServiceBusLogger { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            ServiceBusLogger = Program.LogFactory.CreateLogger(nameof(MyServiceBusTcpClient));

            var serviceBusClient = new MyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");
            serviceBusClient.Log.AddLogException(ex => ServiceBusLogger.LogInformation(ex, "Exception in MyServiceBusTcpClient"));
            serviceBusClient.Log.AddLogInfo(info => ServiceBusLogger.LogDebug($"MyServiceBusTcpClient[info]: {info}"));
            serviceBusClient.SocketLogs.AddLogInfo((context, msg) => ServiceBusLogger.LogInformation($"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Info] {msg}"));
            serviceBusClient.SocketLogs.AddLogException((context, exception) => ServiceBusLogger.LogInformation(exception, $"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Exception] {exception.Message}"));
            builder.RegisterInstance(serviceBusClient).AsSelf().SingleInstance();

            DepositDetector.Client.AutofacHelper.RegisterClientRegistrationPublisher(builder, serviceBusClient);
        }

        
    }
}