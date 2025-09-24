


using System.Collections.Generic;

namespace RabbitMQDemo.Client.Models
{
public class KotBotPayload
	{
		public string  ClientId { get; set; }
		public string  ApiToken { get; set; }
		public PrintVM payload { get; set; }
	}
	public class PrintVM
	{
		public bool IsCancellationBill { get; set; }
		public PrintMasteVM Master { get; set; }
		public List<PrintDetailVM> Details { get; set; }
	}
	public class PrintMasteVM
	{
		public string BillNo { get; set; }
		public string Date { get; set; }
		public string TableNo { get; set; }
		public string Waiter { get; set; }
		public string Time { get; set; }
		public string Type { get; set; }
		public string SerialNo { get; set; }
		public string PrinterName { get; set; }
		public string PrintedBy { get; set; }
		public int NoOfKotBotToBePrinted { get; set; }
	}
	public class PrintDetailVM
	{
		public string ItemName { get; set; }
		public int Quantity { get; set; }
		public string Remarks { get; set; }
	}
	public class ServiceResult<t>
	{
		public ResultStatus Status { get; set; }
		public string? Message { get; set; }
		public t? Data { get; set; }
	}
	public class InvoicePrintVM
	{
		public InvoicePrintMasterVM Master { get; set; }
		public List<InvoicePrintDetailVM> Details { get; set; }
		public List<ModeOfPayment> ModesOfPayment { get; set; }

	}
	public class InvoicePrintMasterVM
	{
		public bool IsVatBill { get; set; }
		public bool IsThisAbbreviatedBill { get; set; }
		public string Tel { get; set; }
		public string VATNo { get; set; }
		public string CompanyName { get; set; }
		public string CompanyAddress { get; set; }
		public string InvoiceNo { get; set; }
		public string DateBS { get; set; }
		public string DateAD { get; set; }
		public string Time { get; set; }
		public decimal TotalAmount { get; set; }
		public double DiscountPercentage { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal AmtAfterDisc { get; set; }
		public decimal TaxableAmount { get; set; }
		public decimal VATAmount { get; set; }
		public decimal PayableAmount { get; set; }
		public string PayableAmountInWords { get; set; }
		public string Cashier { get; set; }
		public string PrinterName { get; set; }
		public string SpecialRemarks { get; set; }
		public int PrintedTimes { get; set; }
	}

	public class InvoicePrintDetailVM
	{
		public string Item { get; set; }
		public int Quantity { get; set; }
		public decimal Rate { get; set; }
		public decimal Amount { get; set; }
	}
	public class ModeOfPayment
	{
		public string PaymentMode { get; set; }
		public decimal Amount { get; set; }
	}

	public class CreditNotePrintVM
	{
		public CreditNotePrintMasterVM Master { get; set; }
		public List<CreditNotePrintDetailVM> Details { get; set; }
		public List<ModeOfPayment> ModesOfPayment { get; set; }

	}
	public class CreditNotePrintMasterVM
	{
		public string Tel { get; set; }
		public string VATNo { get; set; }
		public string CompanyName { get; set; }
		public string CompanyAddress { get; set; }
		public string InvoiceNo { get; set; }
		public string CreditNoteNo { get; set; }
		public string DateBS { get; set; }
		public string DateAD { get; set; }
		public string Time { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal TaxableAmount { get; set; }
		public decimal VATAmount { get; set; }
		public decimal PayableAmount { get; set; }
		public string PayableAmountInWords { get; set; }
		public string Cashier { get; set; }
		public string PrinterName { get; set; }
		public string CreditNoteBy { get; set; }
		public string ReasonForCreditNote { get; set; }
		public string CnNumber { get; set; }
		public string CnDate { get; set; }
		public string BuyersName { get; set; }
		public string BuyersAddress { get; set; }
	}

	public class CreditNotePrintDetailVM
	{
		public string Item { get; set; }
		public int Quantity { get; set; }
		public decimal Rate { get; set; }
		public decimal Amount { get; set; }
	}
	public enum ResultStatus
	{
		processError,
		dataBaseError,
		ComError,
		unHandeledError,
		Ok,
		InvalidToken
	}
}