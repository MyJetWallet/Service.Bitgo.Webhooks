using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.Service;
using Newtonsoft.Json;
using Service.Bitgo.Webhooks.Domain.Models;


// ReSharper disable UnusedMember.Global

namespace Service.Bitgo.Webhooks.Services
{
    public class WebhookMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebhookMiddleware> _logger;
        private readonly IPublisher<SignalBitGoTransfer> _publisher;

        public string TransferPath = "/webhook/transfer";

        /// <summary>
        /// Middleware that handles all unhandled exceptions and logs them as errors.
        /// </summary>
        public WebhookMiddleware(RequestDelegate next, ILogger<WebhookMiddleware> logger, IPublisher<SignalBitGoTransfer> publisher)
        {
            _next = next;
            _logger = logger;
            _publisher = publisher;
        }

        /// <summary>
        /// Invokes the middleware
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            if (!context.Request.Path.StartsWithSegments("/webhook", StringComparison.OrdinalIgnoreCase))
            {
                await _next.Invoke(context);
                return;
            }

            var path = context.Request.Path;
            var method = context.Request.Method;

            var body = "--none--";

            if (method == "POST")
            {
                await using var buffer = new MemoryStream();
                
                await context.Request.Body.CopyToAsync(buffer);

                buffer.Position = 0L;

                using var reader = new StreamReader(buffer);

                body = await reader.ReadToEndAsync();
            }

            var query = context.Request.QueryString;

            _logger.LogInformation($"'{path}' | {query} | {method}\n{body}");


            if (path.StartsWithSegments(TransferPath) && method == "POST")
            {
                using var activity = MyTelemetry.StartActivity($"Receive transfer webhook");


                var dto = JsonConvert.DeserializeObject<TransferDto>(body);

                path.ToString().AddToActivityAsTag("webhook-path");
                body.AddToActivityAsTag("webhook-body");

                await _publisher.PublishAsync(new SignalBitGoTransfer()
                {
                    Coin = dto.Coin,
                    TransferId = dto.TransferId,
                    WalletId = dto.WalletId
                });
            }

            context.Response.StatusCode = 200;
        }
    }

    public class TransferDto
    {
        [JsonProperty("coin")]
        public string Coin { get; set; }

        [JsonProperty("wallet")]
        public string WalletId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("transfer")]
        public string TransferId { get; set; }
    }
}