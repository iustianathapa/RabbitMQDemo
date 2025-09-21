using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using RabbitMQDemo.Client.Models;

namespace RabbitMQDemo.Client.Services
{
	public class PrintManager
	{
		private string billContent;
		private readonly string printerName;
		private const int MaxPrintedItems = 20;
		private static readonly Dictionary<string, DateTime> _printedDataCache = new();

		public PrintManager(string printerName)
		{
			this.printerName = printerName;
		}

		public ServiceResult<bool> PrintBill(PrintVM printData)
		{
			Console.WriteLine($"{DateTime.Now} Print Service called. Printer: {printerName}. Serial No: {printData.Master.SerialNo}");

			//// Optional: check printer exists
			//if (!IsPrinterReady(printerName))
			//{
			//	Console.WriteLine($"{DateTime.Now} Printer: {printerName} not found");
			//	return new ServiceResult<bool>
			//	{
			//		Data = false,
			//		Status = ResultStatus.processError,
			//		Message = $"Printer '{printerName}' not found."
			//	};
			//}

			// Prepare bill content
			StringBuilder contentBuilder = new StringBuilder();
			int waiterNameMaxLength = 11;

			if (printData.IsCancellationBill)
				contentBuilder.AppendLine("##BOLD##-------------CANCELLATION-----------------");

			contentBuilder.AppendLine($"Bill No: {printData.Master.BillNo.PadRight(10)}  Date: {printData.Master.Date}");
			contentBuilder.AppendLine($"Table No: {printData.Master.TableNo}       Time: {printData.Master.Time}");

			string waiterName = printData.Master.Waiter ?? "";
			string waiterNameSubString = "";
			if (waiterName.Length > waiterNameMaxLength)
			{
				contentBuilder.AppendLine($"Waiter: {waiterName.Substring(0, waiterNameMaxLength)}    {printData.Master.Type}: {printData.Master.SerialNo}");
				waiterNameSubString = waiterName.Substring(waiterNameMaxLength);
			}
			else
			{
				contentBuilder.AppendLine($"Waiter: {waiterName.PadRight(waiterNameMaxLength)}  {printData.Master.Type}: {printData.Master.SerialNo}");
			}
			if (waiterNameSubString.Length > 0)
				contentBuilder.AppendLine($"        {waiterNameSubString}");

			contentBuilder.AppendLine("-----------------------------------------------");
			contentBuilder.AppendLine("Item Name                Qty  Rem/Time");
			contentBuilder.AppendLine("-----------------------------------------------");

			foreach (var detail in printData.Details)
			{
				string itemName = detail.ItemName ?? "";
				string remark = detail.Remarks ?? "";

				int lineLength = 24;
				int remarkBlockWidth = 9;

				string itemFirstPart = itemName.Length > lineLength
					? itemName.Substring(0, lineLength)
					: itemName.PadRight(lineLength);

				string remarkFirstPart = remark.Length > remarkBlockWidth
					? remark.Substring(0, remarkBlockWidth)
					: remark.PadRight(remarkBlockWidth);

				contentBuilder.AppendLine($"##BOLD##{itemFirstPart,-24}{detail.Quantity,3}   {remarkFirstPart}");

				int itemStartIndex = lineLength;
				int remarkStartIndex = remarkBlockWidth;

				while (itemStartIndex < itemName.Length || remarkStartIndex < remark.Length)
				{
					string itemPart = "";
					if (itemStartIndex < itemName.Length)
					{
						int len = Math.Min(lineLength, itemName.Length - itemStartIndex);
						itemPart = itemName.Substring(itemStartIndex, len);
						itemStartIndex += len;
					}
					string itemPartPadded = itemPart.PadRight(lineLength);

					string remarkPart = "";
					if (remarkStartIndex < remark.Length)
					{
						int len = Math.Min(remarkBlockWidth, remark.Length - remarkStartIndex);
						remarkPart = remark.Substring(remarkStartIndex, len);
						remarkStartIndex += len;
					}

					contentBuilder.AppendLine($"##BOLD##{itemPartPadded,-24}      {remarkPart}");
				}
			}

			contentBuilder.AppendLine("-----------------------------------------------");
			contentBuilder.AppendLine($"Printed By: {printData.Master.PrintedBy}");

			// Store bill content
			billContent = contentBuilder.ToString();

			// Create PrintDocument for Windows printing
			PrintDocument printDocument = new PrintDocument();
			printDocument.PrinterSettings.PrinterName = printerName;
			printDocument.PrintPage += new PrintPageEventHandler(PrintPage);

			try
			{
				if (_printedDataCache.ContainsKey($"{printData.Master.SerialNo}{printData.IsCancellationBill}"))
				{
					Console.WriteLine($"{DateTime.Now} {printData.Master.SerialNo} has already been printed");
					return new ServiceResult<bool>
					{
						Data = true,
						Status = ResultStatus.Ok,
						Message = "This data has already been printed."
					};
				}

				if (printData.IsCancellationBill)
					printData.Master.NoOfKotBotToBePrinted = 1;

				for (int i = 1; i <= printData.Master.NoOfKotBotToBePrinted; i++)
				{
					Console.WriteLine(billContent);  // optional preview
					//printDocument.Print();           // actual print
					Console.WriteLine($"{DateTime.Now} {printData.Master.SerialNo} printed");
				}

				// Maintain printed cache
				if (_printedDataCache.Count >= MaxPrintedItems)
				{
					var oldestKey = _printedDataCache.OrderBy(kvp => kvp.Value).First().Key;
					_printedDataCache.Remove(oldestKey);
				}

				_printedDataCache[$"{printData.Master.SerialNo}{printData.IsCancellationBill}"] = DateTime.UtcNow;
				Console.WriteLine($"{DateTime.Now} {printData.Master.SerialNo} saved to dictionary");

				return new ServiceResult<bool>
				{
					Data = true,
					Status = ResultStatus.Ok,
					Message = "Printed Successfully"
				};
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Print error: {ex}");
				return new ServiceResult<bool>
				{
					Data = false,
					Status = ResultStatus.processError,
					Message = "Failed to print"
				};
			}
		}



		private bool PrintViaSystemCopy(string content, string printer)
		{
			try
			{
				// Write content to temp file
				string tempFile = System.IO.Path.GetTempFileName();
				System.IO.File.WriteAllText(tempFile, content, Encoding.UTF8);

				// Try lp first (common on many Linux distros), then lpr
				var lpResult = RunProcess("lp", $"-d {EscapeArg(printer)} \"{tempFile}\"");
				if (lpResult.Success)
				{
					TryDeleteTemp(tempFile);
					return true;
				}

				var lprResult = RunProcess("lpr", $"-P {EscapeArg(printer)} \"{tempFile}\"");
				if (lprResult.Success)
				{
					TryDeleteTemp(tempFile);
					return true;
				}

				// As a last resort keep the temp file so operator can print manually
				Console.WriteLine($"No system print command succeeded. Temp file: {tempFile}");
				return false;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"PrintViaSystemCopy error: {ex}");
				return false;
			}
		}

		private (bool Success, int ExitCode, string Output, string Error) RunProcess(string fileName, string arguments)
		{
			try
			{
				var psi = new ProcessStartInfo
				{
					FileName = fileName,
					Arguments = arguments,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (var proc = Process.Start(psi))
				{
					if (proc == null) return (false, -1, "", "Process start failed");
					string stdout = proc.StandardOutput.ReadToEnd();
					string stderr = proc.StandardError.ReadToEnd();
					proc.WaitForExit(10000);
					return (proc.ExitCode == 0, proc.ExitCode, stdout, stderr);
				}
			}
			catch (Exception ex)
			{
				return (false, -1, "", ex.Message);
			}
		}

		private string EscapeArg(string arg)
		{
			if (string.IsNullOrEmpty(arg)) return "";
			return arg.Replace("\"", "\\\"");
		}

		private void TryDeleteTemp(string path)
		{
			try { System.IO.File.Delete(path); } catch { /* ignore */ }
		}

		private bool IsPrinterReady(string printerName)
		{
			foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
			{
				if (installedPrinter.Equals(printerName, StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						using (ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_Printer WHERE Name = '{printerName}'"))
						{
							foreach (ManagementObject printer in searcher.Get())
							{
								string printerStatus = printer["WorkOffline"] != null && (bool)printer["WorkOffline"] ? "Offline" : "Online";
								return printerStatus == "Online";
							}
						}
					}
					catch (Exception)
					{
						return false;
					}
				}
			}
			return false;
		}

		private void PrintPage(object sender, PrintPageEventArgs e)
		{
			// Create a font for printing
			Font font = new Font("Courier New", 8);
			Font boldFont = new Font("Courier New", 8, FontStyle.Bold);

			float lineHeight = font.GetHeight(e.Graphics) + 5;

			string[] lines = billContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
			float yPos = 10;

			foreach (var line in lines)
			{
				if (line.StartsWith("##BOLD##"))
				{
					string cleanLine = line.Replace("##BOLD##", "");
					e.Graphics.DrawString(cleanLine, boldFont, Brushes.Black, new PointF(10, yPos));
				}
				else
				{
					e.Graphics.DrawString(line, font, Brushes.Black, new PointF(10, yPos));
				}
				yPos += lineHeight;
			}
		}
	}
}