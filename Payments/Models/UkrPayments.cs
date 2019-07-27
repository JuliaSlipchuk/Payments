using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Payments.Models
{
    [Table("UkrPayments")]
    public class UkrPayments
    {
        [Key]
        public int UrkrId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "The length should be within 5 - 50")]
        public string PayeeName { get; set; }
        [Required]
        public decimal Sum { get; set; }
        [Required]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$", ErrorMessage = "It should be code of a card")]
        public string PurposePayment { get; set; }
        [Required]
        [RegularExpression(@"^\w{8}$", ErrorMessage = "It should be EDRPOUcode")]
        public string EDRPOUcode { get; set; }
        [Required]
        public int MFO { get; set; }
        [Required]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$", ErrorMessage = "It should be code of a card")]
        public string UkrCardsCurrentAccount { get; set; }
        public UkrCards UkrCards { get; set; } 
        [Required]
        public string Email { get; set; }
        [Required]
        public DateTime DateTime { get; set; }
    }
}