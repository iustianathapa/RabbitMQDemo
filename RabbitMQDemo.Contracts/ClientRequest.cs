using System.Collections.Generic;

namespace RabbitMQDemo.Contracts
{
    public class ClientRequest
    {
        public string? ClientId { get; set; } // optional
        public string Method { get; set; } = "Print";
        public Dictionary<string, string> Payload { get; set; } = new();
    }
}