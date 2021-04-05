using SimpleTrading.SettingsReader;

namespace Service.Bitgo.Webhooks.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("BitgoWebhooks.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("BitgoWebhooks.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("BitgoWebhooks.ZipkinUrl")]
        public string ZipkinUrl { get; set; }
    }
}