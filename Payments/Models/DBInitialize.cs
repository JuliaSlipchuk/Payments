using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace Payments.Models
{
    public class DBInitialize : DropCreateDatabaseAlways<PaymentContext>
    {
        protected override void Seed(PaymentContext db)
        {
            try
            {
                db.UkrCards.Add(new UkrCards { CurrentAccount = "1111-2222-3333-4444", Sum = 10000 });
                db.UkrCards.Add(new UkrCards { CurrentAccount = "4444-3333-2222-1111", Sum = 5000 });
                db.SwiftCards.Add(new SwiftCards { CurrentAccount = "9999-8888-7777-6666", Sum = 1000 });
                db.SwiftCards.Add(new SwiftCards { CurrentAccount = "6666-7777-8888-9999", Sum = 25000 });
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    string str1 = eve.Entry.Entity.GetType().Name;
                    string str2 = eve.Entry.State.ToString();
                    List<string> list = new List<string>();
                    foreach (var ve in eve.ValidationErrors)
                    {
                        list.Add(ve.PropertyName);
                        list.Add(ve.ErrorMessage);
                    }
                }
                throw;
            }
            base.Seed(db);
        }
    }
}