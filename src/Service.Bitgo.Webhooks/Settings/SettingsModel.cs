using MyYamlParser;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Service.Bitgo.Webhooks.Settings
{
    public class SettingsModel
    {
        [YamlProperty("BitgoWebhooks.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("BitgoWebhooks.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("BitgoWebhooks.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("BitgoWebhooks.MyNoSqlReaderHostPort")]
        public string MyNoSqlReaderHostPort { get; set; }

        [YamlProperty("BitgoWebhooks.BitgoAccessToken")]
        public string BitgoAccessToken { get; set; }

        [YamlProperty("BitgoWebhooks.BitgoExpressUrl")]
        public string BitgoExpressUrl { get; set; }

        [YamlProperty("BitgoWebhooks.WebHooksCheckerIntervalMSec")]
        public int WebHooksCheckerIntervalMSec { get; set; }

        [YamlProperty("BitgoWebhooks.WebhooksUrl")]
        public string WebhooksUrl { get; set; }

        [YamlProperty("BitgoWebhooks.DefaultWebhookConfirmations")]
        public int DefaultWebhookConfirmations { get; set; }

        [YamlProperty("BitgoWebhooks.AllTokenEnabledCoins")]
        public string AllTokenEnabledCoins { get; set; }
    }
}