using System;
using System.Collections.Generic;
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
            var transferUrl = $"{webhookUrl}/transfer";
            var approvalUrl = $"{webhookUrl}/approval";
            var defaultConfirmations = Program.ReloadedSettings(e => e.DefaultWebhookConfirmations).Invoke();
            var allTokenEnabledCoins =
                Program.ReloadedSettings(e => e.AllTokenEnabledCoins).Invoke().Split(";").ToList();

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

                    var correctTransferWebHook = false;
                    var correctApprovalWebHook = false;
                    foreach (var dataWebhook in webhooks.Data.Webhooks)
                    {
                        if (dataWebhook.Url.StartsWith(transferUrl))
                        {
                            correctTransferWebHook = true;
                            continue;
                        }

                        if (dataWebhook.Url.StartsWith(approvalUrl))
                        {
                            correctApprovalWebHook = true;
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

                    if (!correctTransferWebHook)
                    {
                        await AddTransferWebhook(transferUrl, bitgoAssetMapEntity.BitgoCoin, wallet,
                            allTokenEnabledCoins, defaultConfirmations);
                    }

                    if (!correctApprovalWebHook)
                    {
                        await AddApprovalWebhook(approvalUrl, bitgoAssetMapEntity.BitgoCoin, wallet);
                    }
                }
            }
        }

        private async Task AddTransferWebhook(string webhookUrl, string coin, string wallet,
            List<string> allTokenEnabledCoins, int defaultConfirmations)
        {
            var url = $"{webhookUrl}/{coin}";
            _logger.LogInformation("Adding new transfer webhook url {url} for {coin} and wallet {wallet}",
                url, coin, wallet);
            var add = await _bitGoClient.AddWebhookAsync(coin, wallet,
                "transfer", allTokenEnabledCoins.Contains(coin), url,
                $"Webhook.{coin}", defaultConfirmations, false);
            if (add.Success)
            {
                _logger.LogInformation(
                    "Added new transfer webhook url {url} for {coin} and wallet {wallet}", url,
                    coin, wallet);
            }
            else
            {
                _logger.LogInformation(
                    "Unable to add new transfer webhook url {url} for {coin} and wallet {wallet}. Reason: {reason}",
                    url, coin, wallet,
                    add.Error);
            }
        }

        private async Task AddApprovalWebhook(string webhookUrl, string coin, string wallet)
        {
            var url = $"{webhookUrl}/{coin}";
            _logger.LogInformation("Adding new approval webhook url {url} for {coin} and wallet {wallet}",
                url, coin, wallet);
            var add = await _bitGoClient.AddWebhookAsync(coin, wallet,
                "pendingapproval", false, url,
                $"Webhook.{coin}", 0, false);
            if (add.Success)
            {
                _logger.LogInformation(
                    "Added new approval webhook url {url} for {coin} and wallet {wallet}", url,
                    coin, wallet);
            }
            else
            {
                _logger.LogInformation(
                    "Unable to add new approval webhook url {url} for {coin} and wallet {wallet}. Reason: {reason}",
                    url, coin, wallet,
                    add.Error);
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