using System.Runtime.Serialization;

namespace RabbitClient
{
    public enum ExchangeType
    {
        [EnumMember(Value = "fanout")]
        Fanout,
        [EnumMember(Value = "direct")]
        Direct,
        [EnumMember(Value = "topic")]
        Topic
    }
}