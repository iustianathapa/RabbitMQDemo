using System.Collections.Generic;

namespace RabbitMQDemo.Contracts
{
    public class ClientRequest
    {
        public string ClientId { get; set; }
        public string Method { get; set; }
        public Dictionary<string, string> Payload { get; set; } = new Dictionary<string, string>();
    }
}