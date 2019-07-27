using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Payments.Models
{
    public class PaymentContext : DbContext
    {
        public DbSet<UkrPayments> UkrPayments { get; set; }
        public DbSet<SwiftPayments> SwiftPayments { get; set; }
        public DbSet<UkrCards> UkrCards { get; set; }
        public DbSet<SwiftCards> SwiftCards { get; set; }
    }
}