using System;

namespace Service.Bitgo.Webhooks.Domain.Models
{
    public interface IHelloMessage
    {
        string Message { get; set; }
    }
}
