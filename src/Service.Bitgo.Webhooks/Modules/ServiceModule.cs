using Autofac;
using DotNetCoreDecorators;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.BitGo.Settings.Ioc;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Domain.Models;
using Service.Bitgo.Webhooks.Jobs;
using Service.Bitgo.Webhooks.Services;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Service.Bitgo.Webhooks.Modules
{
    public class ServiceModule: Module
    {
        private MyNoSqlTcpClient _myNoSqlClient;
        public static ILogger ServiceBusLogger { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterMyNoSqlTcpClient(builder);
            ServiceBusLogger = Program.LogFactory.CreateLogger(nameof(MyServiceBusTcpClient));

            var serviceBusClient = new MyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");
            serviceBusClient.Log.AddLogException(ex => ServiceBusLogger.LogInformation(ex, "Exception in MyServiceBusTcpClient"));
            serviceBusClient.Log.AddLogInfo(info => ServiceBusLogger.LogDebug($"MyServiceBusTcpClient[info]: {info}"));
            serviceBusClient.SocketLogs.AddLogInfo((context, msg) => ServiceBusLogger.LogInformation($"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Info] {msg}"));
            serviceBusClient.SocketLogs.AddLogException((context, exception) => ServiceBusLogger.LogInformation(exception, $"MyServiceBusTcpClient[Socket {context?.Id}|{context?.ContextName}|{context?.Inited}][Exception] {exception.Message}"));
            builder.RegisterInstance(serviceBusClient).AsSelf().SingleInstance();

            builder
                .RegisterInstance(new SignalBitGoTransferBusPublisher(serviceBusClient))
                .As<IPublisher<SignalBitGoTransfer>>()
                .AutoActivate()
                .SingleInstance();
            
            builder
                .RegisterInstance(new SignalBitGoApprovalBusPublisher(serviceBusClient))
                .As<IPublisher<SignalBitGoPendingApproval>>()
                .AutoActivate()
                .SingleInstance();

            var bitgoClient = new BitGoClient(Program.Settings.BitgoAccessToken, Program.Settings.BitgoExpressUrl)
            {
                ThrowThenErrorResponse = false
            };

            builder
                .RegisterInstance(bitgoClient)
                .As<IBitGoClient>()
                .SingleInstance();
            
            builder.RegisterBitgoSettingsReader(_myNoSqlClient);

            builder
                .RegisterType<WebHooksCheckerJob>()
                .AsSelf()
                .SingleInstance();
        }
        
        private void RegisterMyNoSqlTcpClient(ContainerBuilder builder)
        {
            _myNoSqlClient = new MyNoSqlTcpClient(Program.ReloadedSettings(e => e.MyNoSqlReaderHostPort),
                ApplicationEnvironment.HostName ?? $"{ApplicationEnvironment.AppName}:{ApplicationEnvironment.AppVersion}");

            builder
                .RegisterInstance(_myNoSqlClient)
                .AsSelf()
                .SingleInstance();
        }
    }
}