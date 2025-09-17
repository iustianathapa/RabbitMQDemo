using System.Collections.Generic;

namespace RabbitMQDemo.Client.Models
{
    public class PrintVM
    {
        public MasterVM Master { get; set; } = new();
        public List<DetailVM> Details { get; set; } = new();
    }
}