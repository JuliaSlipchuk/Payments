using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using Payments.Models;
using System.Net;
using System.IO;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;

namespace Payments.Controllers
{
    public class HomeController : Controller
    {
        PaymentContext db = new PaymentContext();
        static Regex regex = new Regex(@"^((http)|(https))://localhost:\d{1,5}/$");
        static Regex reg1 = new Regex(@"^((http)|(https))://localhost:\d{1,5}/Home/Payments\?type=((Ukr)|(Swift))\&Email=([a-z0-9_-]+\.)*[a-z0-9_-]+%40[a-z0-9_-]+(\.[a-z0-9_-]+)*\.[a-z]{2,6}$");
        static Regex reg2 = new Regex(@"^((http)|(https))://localhost:\d{1,5}/Home/Verificat$");
        
        static Regex regToIndex1 = new Regex(@"^((http)|(https))://localhost:\d{1,5}/Home/Archive\?((CurrAccUkr=(\d{4}-){3}\d{4})|(CurrAccSwift=(\d{4}-){3}\d{4})|(CurrAccUkr=(\d{4}-){3}\d{4}\&CurrAccSwift=(\d{4}-){3}\d{4}))$");
        static Regex regToIndex2 = new Regex(@"^((http)|(https))://localhost:\d{1,5}/Home/Verificat$");
        static Regex regToIndex3 = new Regex(@"^((http)|(https))://localhost:\d{1,5}/Home/NewPayment\?type=((Ukr)|(Swift))$");

        [HttpGet]
        public ActionResult Index()
        {
            if (Request.UrlReferrer == null || regToIndex1.IsMatch(Request.UrlReferrer.OriginalString) || regToIndex2.IsMatch(Request.UrlReferrer.OriginalString) || regToIndex3.IsMatch(Request.UrlReferrer.OriginalString))
            {
                return View();
            }
            return Content("<h2 style='color: red;'>You can not go from this page to the specified url</h2>");
        }
        [HttpPost]
        public ActionResult Index(string buttVal, string Email, string Payments, string CurrAccUkr, string CurrAccSwift, string AdminLogin, string AdminPassword, string PaymentsAddOrDel)
        {
            if (buttVal == "Archive")
            {
                return RedirectToRoute(new { controller = "Home", action = "Archive", CurrAccUkr = CurrAccUkr, CurrAccSwift = CurrAccSwift});
            }
            else if (buttVal == "CreateNew")
            {
                return RedirectToRoute(new { controller = "Home", action = "Payments", type = Payments, Email = Email });
            }
            else
            {
                if (AdminLogin == "Administrator" && AdminPassword == "HelloWorld")
                {
                    return RedirectToRoute(new { controller = "Home", action = "AddOrDeleteCard", act = buttVal, type = PaymentsAddOrDel});
                }
                else
                {
                    return Content("<h2 style='color: red;'>You entered wrong password</h2>");
                }
            }
        }

        public ActionResult Archive(string CurrAccUkr, string CurrAccSwift)
        {
            if (Request?.UrlReferrer == null || !regex.IsMatch(Request.UrlReferrer.OriginalString))
            {
                return Content("<h2 style='color: red;'>You can not go from this page to the specified url</h2>");
            }
            List<UkrPayments> thisUkrPayments = new List<UkrPayments>();
            List<SwiftPayments> thisSwiftPaymetns = new List<SwiftPayments>();
            if (CurrAccUkr != null)
            {
                thisUkrPayments.AddRange(db.UkrPayments.Select(o => o).Where(o => (o.UkrCardsCurrentAccount == CurrAccUkr)).ToList());
            }
            if (CurrAccSwift != null)
            {
                thisSwiftPaymetns.AddRange(db.SwiftPayments.Select(o => o).Where(o => (o.SwiftCardsCurrentAccount == CurrAccSwift)).ToList());
            }
            ViewBag.ukrPayments = thisUkrPayments;
            ViewBag.swiftPayments = thisSwiftPaymetns;
            return View("Archive");
        }

        public ActionResult Payments(string type, string Email)
        {
            if (Request?.UrlReferrer == null || !regex.IsMatch(Request.UrlReferrer.OriginalString))
            {
                return Content("<h2 style='color: red;'>You can not go from this page to the specified url</h2>");
            }
            if (type == "Ukr" || type == "Swift")
            {
                return View(type);
            }
            else
            {
                return Content("<h2 style='color: red;'>You should choose 'Ukr' or 'Swift'</h2>");
            }
        }

        private static string code;
        private static UkrPayments newUkr;
        private static SwiftPayments newSwift;
        [HttpPost]
        public ActionResult Verificat(UkrPayments ukrPaym, SwiftPayments swiftPaym, HttpPostedFileBase uploadImage, string buttVal)
        {
            if (Request?.UrlReferrer == null || !reg1.IsMatch(Request.UrlReferrer.OriginalString))
            {
                return Content("<h2 style='color: red;'>You can not go from this page to the specified url</h2>");
            }
            code = GetOneTimePass();
            if (buttVal == "Ukr")
            {
                newUkr = ukrPaym;
                SendEmail(ukrPaym.Email, code);
            }
            else if (buttVal == "Swift")
            {
                newSwift = swiftPaym;
                ForImg(newSwift, uploadImage);
                SendEmail(swiftPaym.Email, code);
            }
            ViewBag.Type = buttVal;
            return View("Verification");
        }
        [HttpPost]
        public ActionResult Verification(string buttVal, string password, string type)
        {
            if (Request?.UrlReferrer == null || !reg2.IsMatch(Request.UrlReferrer.OriginalString))
            {
                return Content("<h2 style='color: red;'>You can not go from this page to the specified url</h2>");
            }
            if (password == code)
            {
                if (buttVal == "submit")
                {
                    if (type == "Ukr" && newUkr != null)
                    {
                        UkrCards thisCard;
                        try
                        {
                            thisCard = db.UkrCards.Select(o => o).Where(o => (o.CurrentAccount == newUkr.UkrCardsCurrentAccount)).ToList()[0];
                        }
                        catch
                        {
                            return Content("<h3 style='color: red'>Such as card does not exists</h3>");
                        }
                        if (newUkr.Sum >= 100 && newUkr.Sum <= 999999999999 && Regex.IsMatch(newUkr.MFO.ToString(), @"^\d{6}$"))
                        {
                            if (thisCard.Sum < (newUkr.Sum + 10 + (decimal)0.1 * newUkr.Sum))
                            {
                                return Content("<h3 style='color: red'>The amount is not enough to withdraw</h3>");
                            }
                            thisCard.Sum -= newUkr.Sum + 10 + (decimal)0.1 * newUkr.Sum;
                            db.Entry(thisCard).State = EntityState.Modified;
                            db.SaveChanges();
                            newUkr.DateTime = DateTime.Now;
                            db.UkrPayments.Add(newUkr);
                        }
                        else
                        {
                            return Content("<h3 style='color: red'>Your record is not valid</h3>");
                        }
                    }
                    else if (type == "Swift" && newSwift != null)
                    {
                        SwiftCards thisCard;
                        try
                        {
                            thisCard = db.SwiftCards.Select(o => o).Where(o => o.CurrentAccount == newSwift.SwiftCardsCurrentAccount).ToList()[0];
                        }
                        catch (Exception e)
                        {
                            return Content("<h3 style='color: red'>Such as card does not exists</h3>");
                        }
                        if (newSwift.Sum >= 100 && newSwift.Sum <= 99999999999999)
                        {
                            if (thisCard.Sum < ((decimal)1.1 * Conversion(thisCard, newSwift) + 1))
                            {
                                return Content("<h3 style='color: red'>The amount is not enough to withdraw</h3>");
                            }
                            thisCard.Sum -= (decimal)1.1 * Conversion(thisCard, newSwift) + 1;
                            db.Entry(thisCard).State = EntityState.Modified;
                            db.SaveChanges();
                            newSwift.DateTime = DateTime.Now;
                            db.SwiftPayments.Add(newSwift);
                        }
                        else
                        {
                            return Content("<h2 style='color: red'>Your record is not valid</h2>");
                        }
                    }
                    else
                    {
                        return Content("<h2 style='color: red'>Type should be 'Ukr' or 'Swift'</h2>");
                    }
                    try
                    {
                        db.SaveChanges();
                        newUkr = null;
                        newSwift = null;
                    }
                    catch (Exception e)
                    {
                        return Content($"<h3 style='color: red'>It is error to add to database your record</h3>" +
                            $"<h3>{e.Message}</h3>");
                    }
                    return RedirectToRoute(new { controller = "Home", action = "NewPayment", type = type });
                }
                else if (buttVal == "exit")
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Content("<h2 style='color: red'>You entered something unknown</h2>");
                }
            }
            else
            {
                return Content("<h2 style='color: red'>You entered invalid password</h2>");
            }
        }

        [HttpGet]
        public ActionResult NewPayment(string type)
        {
            int id;
            if (type == "Ukr")
            {
                id = db.UkrPayments.Select(o => o.UrkrId).Max();
                UkrPayments newUkrPaym = db.UkrPayments.Select(o => o).Where(o => (o.UrkrId == db.UkrPayments.Select(d => d.UrkrId).Max())).ToList()[0];
                ViewBag.ukrPaym = newUkrPaym;
            }
            else
            {
                id = db.SwiftPayments.Select(o => o.SwiftId).Max();
                SwiftPayments newSwiftPaym = db.SwiftPayments.Select(o => o).Where(o => (o.SwiftId == db.SwiftPayments.Select(d => d.SwiftId).Max())).ToList()[0];
                ViewBag.swiftPaym = newSwiftPaym;
            }
            return View();
        }

        [HttpGet]
        public ActionResult AddOrDeleteCard(string act, string type)
        {
            if (Request.UrlReferrer != null && regex.IsMatch(Request.UrlReferrer.OriginalString))
            {
                ViewBag.Type = type;
                return View(act);
            }
            return Content("<h2>You cannot get to this page</h2>");
        }
        [HttpPost]
        public ActionResult AddOrDeleteCard(string buttVal, string type, string CurrentAccount, string Sum)
        {
            if (buttVal == "Create")
            {
                if (Convert.ToInt32(Sum) >= 100 && Convert.ToInt64(Sum) <= 999999999999 && Regex.IsMatch(CurrentAccount, @"^\d{4}-\d{4}-\d{4}-\d{4}$"))
                {
                    if (type == "Ukr")
                    {
                        if (db.UkrCards.Find(CurrentAccount) == null)
                        {
                            db.UkrCards.Add(new UkrCards { CurrentAccount = CurrentAccount, Sum = Convert.ToInt64(Sum) });
                        }
                        else
                        {
                            return Content($"<h2 style='color:red;'>Ukr card {CurrentAccount} already exist</h2>");
                        }
                    }
                    else
                    {
                        if (db.SwiftCards.Find(CurrentAccount) == null)
                        {
                            db.SwiftCards.Add(new SwiftCards { CurrentAccount = CurrentAccount, Sum = Convert.ToInt64(Sum) });
                        }
                        else
                        {
                            return Content($"<h2 style='color:red;'>Swift card {CurrentAccount} already exist</h2>");
                        }
                    }
                    db.SaveChanges();
                    return Content($"<h2>Your {type} card was successfully added to database</h2>");
                }
                else
                {
                    return Content("<h2 style='color:red;'>You entered invalid data</h2>");
                }
            }
            else if (buttVal == "Exit")
            {
                if (Regex.IsMatch(CurrentAccount, @"^\d{4}-\d{4}-\d{4}-\d{4}$"))
                {
                    try
                    {
                        if (type == "Ukr")
                        {
                            UkrCards acc = db.UkrCards.Find(CurrentAccount);
                            db.UkrCards.Remove(acc);
                        }
                        else
                        {
                            SwiftCards acc = db.SwiftCards.Find(CurrentAccount);
                            db.SwiftCards.Remove(acc);
                        }
                        db.SaveChanges();
                        return Content($"<h2>Your {type} card was successfully removed from database</h2>");
                    }
                    catch
                    {
                        return Content($"<h2 style='color:red;'>Card {CurrentAccount} does not exist</h2>");
                    }
                }
                else
                {
                    return Content("<h2 style='color:red;'>You entered invalid account</h2>");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public string GetOneTimePass()
        {
            string result = "";
            Random rand = new Random();
            for (int i = 0; i < 10; i++)
            {
                System.Threading.Thread.Sleep(20);
                result += rand.Next(0, 9);
            }
            return result;
        }
        public void SendEmail(string email, string code)
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com");
            client.Port = 587;
            client.Credentials = new NetworkCredential("julia2015olex@gmail.com", "grandrvua");
            client.EnableSsl = true;
            string body = "DateTime: " + DateTime.Now + "\nCode: " + code;
            client.Send(new MailMessage("julia2015olex@gmail.com", email, "One-Time code", body));
        }
        public void ForImg(SwiftPayments swift, HttpPostedFileBase uploadImage)
        {
            if (uploadImage != null)
            {
                int start = uploadImage.FileName.LastIndexOf('.');
                swift.Extension = uploadImage.FileName.Substring(start + 1).ToLower();
                if (swift.Extension == "jpg" || swift.Extension == "png" || swift.Extension == "gif" || swift.Extension == "jpeg")
                {
                    using (BinaryReader reader = new BinaryReader(uploadImage.InputStream))
                    {
                        swift.PayerPhoto = reader.ReadBytes(uploadImage.ContentLength);
                    }
                }
            }
        }
        public decimal Conversion(SwiftCards thisCard, SwiftPayments newSwift)
        {
            switch(newSwift.RecipientCurrency)
            {
                case "USD":
                    return newSwift.Sum;
                case "EUR":
                    return (newSwift.Sum * 31) / 28;
                case "PLN":
                    return (newSwift.Sum * 7) / 28;
                case "GBP":
                    return (newSwift.Sum * 36) / 28;
                default:
                    throw new Exception("You select non-existent currency");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}