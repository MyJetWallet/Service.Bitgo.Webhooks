using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.DataReader;
using MyServiceBus.TcpClient;
using Service.Bitgo.Webhooks.Jobs;

namespace Service.Bitgo.Webhooks
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly MyServiceBusTcpClient _busTcpClient;
        private readonly MyNoSqlTcpClient _myNoSqlClient;
        private readonly WebHooksCheckerJob _webHooksCheckerJob;

        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime,
            ILogger<ApplicationLifetimeManager> logger, MyServiceBusTcpClient busTcpClient, MyNoSqlTcpClient myNoSqlClient,
            WebHooksCheckerJob webHooksCheckerJob)
            : base(appLifetime)
        {
            _logger = logger;
            _busTcpClient = busTcpClient;
            _myNoSqlClient = myNoSqlClient;
            _webHooksCheckerJob = webHooksCheckerJob;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called");
            _busTcpClient.Start();
            _logger.LogInformation("MyServiceBusTcpClient is started");
            _myNoSqlClient.Start();
            _logger.LogInformation("MyNoSqlTcpClient is started");
            _webHooksCheckerJob.Start();
            _logger.LogInformation("WebHooksCheckerJob is started");
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called");
            _busTcpClient.Stop();
            _logger.LogInformation("MyServiceBusTcpClient is stop");
            _myNoSqlClient.Stop();
            _logger.LogInformation("MyNoSqlTcpClient is stop");
            _webHooksCheckerJob.Stop();
            _logger.LogInformation("WebHooksCheckerJob is stopped");
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called");
        }
    }
}