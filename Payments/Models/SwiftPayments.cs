using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Payments.Models
{
    [Table("SwiftPayments")]
    public class SwiftPayments
    {
        [Key]
        [Required]
        public int SwiftId { get; set; }
        [Required]
        public byte[] PayerPhoto { get; set; }
        [Required]
        [RegularExpression(@"^jpg|jpeg|png$", ErrorMessage = "It should be extension for image")]
        public string Extension { get; set; }
        [Required]
        public string RecipientCurrency { get; set; }
        [Required]
        public decimal Sum { get; set; }
        [Required]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "It should be PaymentReference")]
        public string PaymentReference { get; set; }
        [Required]
        [RegularExpression(@"^[A-Z]{6}\w{2,5}$", ErrorMessage = "It should be BIC")]
        public string BIC { get; set; }
        [Required]
        [RegularExpression(@"^[A-Z]{2}\d{20}$", ErrorMessage = "It should be IBAN")]
        public string IBAN { get; set; }
        [Required]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$", ErrorMessage = "It should be code of a card")]
        public string SwiftCardsCurrentAccount { get; set; }
        public SwiftCards SwiftCards { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
    }
    public enum Currency
    {
        EUR,
        USD,
        PLN,
        GBP
    }
}