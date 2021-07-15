using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.BitGo;
using MyJetWallet.BitGo.Settings.NoSql;
using MyJetWallet.Sdk.Service.Tools;
using MyNoSqlServer.Abstractions;

// ReSharper disable InconsistentLogPropertyNaming

namespace Service.Bitgo.Webhooks.Jobs
{
    public class WebHooksCheckerJob : IDisposable
    {
        private readonly ILogger<WebHooksCheckerJob> _logger;
        private readonly MyTaskTimer _timer;
        private readonly IMyNoSqlServerDataReader<BitgoAssetMapEntity> _myNoSqlReadRepository;
        private readonly IBitGoClient _bitGoClient;

        public WebHooksCheckerJob(ILogger<WebHooksCheckerJob> logger,
            IMyNoSqlServerDataReader<BitgoAssetMapEntity> myNoSqlReadRepository, IBitGoClient bitGoClient)
        {
            _logger = logger;
            _myNoSqlReadRepository = myNoSqlReadRepository;
            _bitGoClient = bitGoClient;
            _timer = new MyTaskTimer(typeof(WebHooksCheckerJob),
                TimeSpan.FromMilliseconds(Program.Settings.WebHooksCheckerIntervalMSec),
                logger, DoTime);
        }

        private async Task DoTime()
        {
            var webhookUrl = Program.ReloadedSettings(e => e.WebhooksUrl).Invoke();
            var defaultConfirmations = Program.ReloadedSettings(e => e.DefaultWebhookConfirmations).Invoke();
            var allTokenEnabledCoins = Program.ReloadedSettings(e => e.AllTokenEnabledCoins).Invoke().Split(";").ToList();

            var bitgoAssets = _myNoSqlReadRepository.Get();
            foreach (var bitgoAssetMapEntity in bitgoAssets)
            {
                foreach (var wallet in bitgoAssetMapEntity.EnabledBitgoWalletIds.Split(";"))
                {
                    var webhooks = await _bitGoClient.ListWebhooksAsync(bitgoAssetMapEntity.BitgoCoin, wallet);
                    if (!webhooks.Success)
                    {
                        _logger.LogInformation("Unable to get info about webhooks for {coin} and wallet {wallet}",
                            bitgoAssetMapEntity.BitgoCoin, wallet);
                        continue;
                    }

                    var correctWebHook = false;
                    foreach (var dataWebhook in webhooks.Data.Webhooks)
                    {
                        if (dataWebhook.Url.StartsWith(webhookUrl))
                        {
                            correctWebHook = true;
                            continue;
                        }

                        _logger.LogInformation("Remove invalid webhook url {url} for {coin} and wallet {wallet}",
                            dataWebhook.Url, bitgoAssetMapEntity.BitgoCoin, wallet);
                        var remove = await _bitGoClient.RemoveWebhookAsync(dataWebhook.Coin, dataWebhook.WalletId,
                            dataWebhook.Type, dataWebhook.Url, dataWebhook.Id);
                        if (remove.Success)
                        {
                            _logger.LogInformation(
                                "Removed invalid webhook url {url} for {coin} and wallet {wallet}", dataWebhook.Url,
                                bitgoAssetMapEntity.BitgoCoin, wallet);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Unable to remove invalid webhook url {url} for {coin} and wallet {wallet}. Reason: {reason}",
                                dataWebhook.Url, bitgoAssetMapEntity.BitgoCoin, wallet,
                                remove.Error);
                        }
                    }

                    if (correctWebHook) continue;

                    var url = $"{webhookUrl}/{bitgoAssetMapEntity.BitgoCoin}";
                    _logger.LogInformation("Adding new webhook url {url} for {coin} and wallet {wallet}",
                        url, bitgoAssetMapEntity.BitgoCoin, wallet);
                    var add = await _bitGoClient.AddWebhookAsync(bitgoAssetMapEntity.BitgoCoin, wallet,
                        "transfer", allTokenEnabledCoins.Contains(bitgoAssetMapEntity.BitgoCoin), url,
                        $"Webhook.{bitgoAssetMapEntity.BitgoCoin}", defaultConfirmations, false);
                    if (add.Success)
                    {
                        _logger.LogInformation(
                            "Added new webhook url {url} for {coin} and wallet {wallet}", url,
                            bitgoAssetMapEntity.BitgoCoin, wallet);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Unable to add new webhook url {url} for {coin} and wallet {wallet}. Reason: {reason}",
                            url, bitgoAssetMapEntity.BitgoCoin, wallet,
                            add.Error);
                    }
                }
            }
        }

        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}