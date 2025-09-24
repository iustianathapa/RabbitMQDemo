using RabbitMQDemo.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RabbitMQDemo.Client.Services
{
    public class PrintManager
    {
        private readonly string printerName;
        private static readonly Dictionary<string, DateTime> _printedDataCache = new();
        private const int MaxPrintedItems = 100;

        public PrintManager(string printerName)
        {
            this.printerName = printerName;
        }

        public ServiceResult<bool> PrintBill(PrintVM printData)
        {
            Console.WriteLine($"{DateTime.Now} Print Service called. Printer: {printerName}. Serial No: {printData.Master.SerialNo}");

            // Prevent duplicate printing
            string cacheKey = $"{printData.Master.SerialNo}{printData.IsCancellationBill}";
            if (_printedDataCache.ContainsKey(cacheKey))
            {
                return new ServiceResult<bool>
                {
                    Data = true,
                    Status = ResultStatus.Ok,
                    Message = "This data has already been printed."
                };
            }

            try
            {
                // Build bill content
                StringBuilder sb = new StringBuilder();
                int waiterNameMaxLength = 11;

                if (printData.IsCancellationBill)
                    sb.AppendLine("##BOLD##-------------CANCELLATION-----------------");

                sb.AppendLine($"Bill No: {printData.Master.BillNo.PadRight(10)}  Date: {printData.Master.Date}");
                sb.AppendLine($"Table No: {printData.Master.TableNo}       Time: {printData.Master.Time}");

                string waiterName = printData.Master.Waiter ?? "";
                string waiterNameSub = "";

                if (waiterName.Length > waiterNameMaxLength)
                {
                    sb.AppendLine($"Waiter: {waiterName.Substring(0, waiterNameMaxLength)}    {printData.Master.Type}: {printData.Master.SerialNo}");
                    waiterNameSub = waiterName.Substring(waiterNameMaxLength);
                }
                else
                {
                    sb.AppendLine($"Waiter: {waiterName.PadRight(waiterNameMaxLength)}  {printData.Master.Type}: {printData.Master.SerialNo}");
                }

                if (!string.IsNullOrEmpty(waiterNameSub))
                    sb.AppendLine($"        {waiterNameSub}");

                sb.AppendLine("-----------------------------------------------");
                sb.AppendLine("Item Name                Qty  Rem/Time");
                sb.AppendLine("-----------------------------------------------");

                foreach (var detail in printData.Details)
                {
                    string itemName = detail.ItemName ?? "";
                    string remark = detail.Remarks ?? "";

                    int lineLength = 24;
                    int remarkBlockWidth = 9;

                    // First line
                    string itemPart = itemName.Length > lineLength ? itemName.Substring(0, lineLength) : itemName.PadRight(lineLength);
                    string remarkPart = remark.Length > remarkBlockWidth ? remark.Substring(0, remarkBlockWidth) : remark.PadRight(remarkBlockWidth);

                    sb.AppendLine($"##BOLD##{itemPart,-24}{detail.Quantity,3}   {remarkPart}");

                    int itemIndex = lineLength;
                    int remarkIndex = remarkBlockWidth;

                    // Wrap remaining text
                    while (itemIndex < itemName.Length || remarkIndex < remark.Length)
                    {
                        string itemLine = itemIndex < itemName.Length
                            ? itemName.Substring(itemIndex, Math.Min(lineLength, itemName.Length - itemIndex))
                            : "".PadRight(lineLength);
                        itemIndex += lineLength;

                        string remarkLine = remarkIndex < remark.Length
                            ? remark.Substring(remarkIndex, Math.Min(remarkBlockWidth, remark.Length - remarkIndex))
                            : "".PadRight(remarkBlockWidth);
                        remarkIndex += remarkBlockWidth;

                        sb.AppendLine($"##BOLD##{itemLine,-24}      {remarkLine}");
                    }
                }

                sb.AppendLine("-----------------------------------------------");
                sb.AppendLine($"Printed By: {printData.Master.PrintedBy}");

                // Output bill to console (simulate printing)
                Console.WriteLine(sb.ToString());

                // Update cache
                if (_printedDataCache.Count >= MaxPrintedItems)
                {
                    var oldestKey = _printedDataCache.OrderBy(kvp => kvp.Value).First().Key;
                    _printedDataCache.Remove(oldestKey);
                }
                _printedDataCache[cacheKey] = DateTime.UtcNow;

                return new ServiceResult<bool>
                {
                    Data = true,
                    Status = ResultStatus.Ok,
                    Message = "Printed successfully (simulated)"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool>
                {
                    Data = false,
                    Status = ResultStatus.processError,
                    Message = $"Failed to print: {ex.Message}"
                };
            }
        }
    }
}
