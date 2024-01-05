using System;
using IqSoft.CP.BLL.Caching;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.BLL.Services;
using NUnit.Framework;
using log4net;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.Integration.Payments.Helpers;
using IqSoft.CP.DAL;
using IqSoft.CP.Common;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace IqSoft.CP.AutomationTest.UTests
{
    public class BusinesLayerMethods
    {
        public void Func1()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var cc = db.Clients.ToList();
                var translations = db.WebSiteMenuItems.Include(x => x.WebSiteSubMenuItems).Where(x => x.Menu.PartnerId == 1 && x.Menu.Type == Constants.WebSiteConfiguration.Translations &&
                     x.Type != "promotion" && x.Type != "news" && x.Type != "page").AsEnumerable().Select(x => new
                     {
                         x.Title,
                         SubMenu = x.WebSiteSubMenuItems.Where(y => !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower())
                        ).Select(z => new
                         {
                             Title = !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower()) ?
                             z.Title : "Content",
                             Value = db.fn_Translation("en").Where(o => o.TranslationId == z.TranslationId).FirstOrDefault()
                         })
                     }).AsEnumerable().ToDictionary(y => y.Title, y => y.SubMenu.ToDictionary(z => z.Title, (z => z.Value == null ? string.Empty : z.Value.Text)));


            }
        }

        public void Func2()
        {
            using (var db = new IqSoftCorePlatformEntities())
            {
                var cc = db.Clients.ToList();
                var translations = db.WebSiteMenuItems.Include(x => x.WebSiteSubMenuItems).Where(x =>  x.Menu.Type == Constants.WebSiteConfiguration.Translations &&
                     x.Type != "promotion" && x.Type != "news" && x.Type != "page").AsEnumerable().Select(x => new
                     {
                         x.Title,
                         SubMenu = x.WebSiteSubMenuItems.Where(y => !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower())
                        ).Select(z => new
                        {
                            Title = !x.Title.Replace("_", "").ToLower().Contains(Constants.WebSiteConfiguration.TermsConditions.ToLower()) ?
                             z.Title : "Content",
                            Value = db.fn_Translation("en").Where(o => o.TranslationId == z.TranslationId).FirstOrDefault()
                        })
                     }).AsEnumerable().ToDictionary(y => y.Title, y => y.SubMenu.ToDictionary(z => z.Title, (z => z.Value == null ? string.Empty : z.Value.Text)));


            }
        }

        [Test]
        public void MethodTest()
        {
            try {
                Func2();

            }
            catch { }
            Func1();

            //CacheManager.GetMatchId();

            ILog log = LogManager.GetLogger("ADONetAppender");
            int partnerId = 1;
            int paymentSystemId = 86;
            var session = new SessionIdentity();
            session.LanguageId = "en";
            session.PartnerId = 1;
            session.LoginIp = "109.75.47.208";
            session.Domain = "craftbet.com";
            using (var db = new IqSoftCorePlatformEntities()) 
            {
                var cc = db.Clients.ToList();
            }
            using (var paymentSystemBl = new PaymentSystemBll(session, log))
            {
                using (var clientBl = new ClientBll(paymentSystemBl))
                {
                    var client = CacheManager.GetClientById(10022);
                    var partnerPaymentSetting = CacheManager.GetPartnerPaymentSettings(partnerId, paymentSystemId, client.CurrencyId, (int)PaymentRequestTypes.Deposit);

                    if (partnerPaymentSetting == null)
                        throw BaseBll.CreateException(string.Empty, Constants.Errors.PartnerPaymentSettingNotFound);
                    var paymentRequest = new PaymentRequest
                    {
                        Amount = 5,
                        ClientId = client.Id,
                        CurrencyId = client.CurrencyId,
                        //Info = "{\"Amount\":\"5000\",\"BankId\":\"\",\"WithdrawCode\":\"\",\"CardType\":1,\"Name\":\"\",\"CardNumber\":\"\",\"ExpDate\":\"\",\"ExpDateMM\":\"\",\"ExpDateYY\":\"\",\"Info\":\"\"}",
                        Info = "{\"BankId\":\"1416\",\"WalletNumber\":\"201557642978\",}",
                        PaymentSystemId = partnerPaymentSetting.PaymentSystemId,
                        PartnerPaymentSettingId = partnerPaymentSetting.Id,

                    };
                    var request = clientBl.CreateDepositFromPaymentSystem(paymentRequest);
                    //request.ExternalTransactionId = request.Id.ToString();
                    //paymentSystemBl.ChangePaymentRequestDetails(request);
                    //clientBl.ApproveDepositFromPaymentSystem(request, false);
                    var response = PaymentHelpers.SendPaymentDepositRequest(request, partnerId, session, log);
                }
            }
        }
    }
}
