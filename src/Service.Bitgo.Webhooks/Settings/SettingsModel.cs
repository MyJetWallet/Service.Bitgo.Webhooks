using SimpleTrading.SettingsReader;

namespace Service.Bitgo.Webhooks.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("BitgoWebhooks.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }
    }
}