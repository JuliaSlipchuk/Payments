﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Payments.Models
{
    [Table("UkrCards")]
    public class UkrCards
    {
        [Key]
        [RegularExpression(@"^\d{4}-\d{4}-\d{4}-\d{4}$", ErrorMessage = "It should be code of a card")]
        public string CurrentAccount { get; set; }
        [Required]
        public decimal Sum { get; set; }
        public ICollection<UkrPayments> UkrPayments;
        public UkrCards()
        {
            UkrPayments = new List<UkrPayments>();
        }
    }
}