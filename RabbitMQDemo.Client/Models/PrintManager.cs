using RabbitMQDemo.Client.Models;
using System;

namespace RabbitMQDemo.Client.Services
{
    public class PrintManager
    {
        private readonly string _printerName;

        public PrintManager(string printerName)
        {
            _printerName = printerName;
        }

        public void PrintBill(PrintVM bill)
        {
            Console.WriteLine($"Printing KOT to printer: {_printerName}");
            Console.WriteLine($"BillNo: {bill.Master.BillNo}, TableNo: {bill.Master.TableNo}, Waiter: {bill.Master.Waiter}");
            Console.WriteLine("Items:");
            foreach (var item in bill.Details)
            {
                Console.WriteLine($" - {item.ItemName} x {item.Qty}");
            }
            Console.WriteLine("----- End of KOT -----");
        }
    }
}