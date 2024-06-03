using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
	public class OrderTransaction
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		public int OrderId { get; set; }	
		public Order Order { get; set; }
		public string Reference { get; set; }
		public int Amount { get; set; }
		public string AccountNumber { get; set; } // stk ao tuong ung voi moi don hang, yea, con tk that la nam o counterAccountNumber
		public string TransactionDateTime { get; set; }
		public string? VirtualAccountName { get; set; }	
		public string? VirtualAccountNumber { get; set; }	
		public string? CounterAccountBankId { get; set; }	
		public string? CounterAccountBankName { get; set; }
		public string? CounterAccountName { get; set; }	
		public string? CounterAccountNumber { get; set; }
	}
	//string reference, int amount, string accountNumber, string description, string transactionDateTime, string? virtualAccountName, string? virtualAccountNumber, string? counterAccountBankId, string? counterAccountBankName, string? counterAccountName, string? counterAccountNumber);
}
