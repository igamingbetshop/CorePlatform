using IqSoft.CP.BLL.Interfaces;
using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using IqSoft.CP.DAL.Models;
using IqSoft.CP.DAL.Models.Affiliates;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IqSoft.CP.BLL.Services
{
    public class AffiliateService : PermissionBll, IAffiliateService
    {
        public AffiliateService(SessionIdentity identity, ILog log)
            : base(identity, log)
        {

        }

        public AffiliateService(BaseBll baseBl)
            : base(baseBl)
        {

        }

        public List<ClientActivityModel> GetClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, long tDate)
        {
            var affiliateClientActivies = new List<ClientActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            foreach (var client in affClients)
            {
                var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                               (x.Status == (int)PaymentRequestStates.Approved ||
                                                                x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                x.Date >= fDate && x.Date < tDate)
                                                    .GroupBy(x => x.Type)
                                                    .Select(x => new
                                                    {
                                                        Type = x.Key,
                                                        Amount = x.Sum(y => y.Amount),
                                                        Count = x.Count()
                                                    }).ToList();
                var dateString = fromDate.ToString("yyyy-MM-dd");
                if (paymentData.Count != 0)
                {
                    affiliateClientActivies.Add(new ClientActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        ActivityDate = dateString,
                        BrandId = brandId,
                        Deposits = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                        Withdrawals = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                        PaymentTransactions = paymentData.Sum(x => x.Count),
                        CurrencyId = client.CurrencyId
                    });
                }

                var activies = Db.Bets.Where(x => x.ClientId == client.ClientId &&
                                                  x.State != (int)BetDocumentStates.Deleted &&
                                                  x.State != (int)BetDocumentStates.Uncalculated &&
                                                  x.BetDate >= fDate && x.BetDate < tDate)
                                      .GroupBy(x => x.ProductId == Constants.SportsbookProductId ? 1 : 2)
                                      .Select(x => new
                                      {
                                          ProductId = x.Key,
                                          BetAmount = x.Where(y => y.BonusId != null && y.BonusId != 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                          WinAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                          BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                          BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                          Count = x.Count()
                                      });

                if (activies.Count() != 0)
                {
                    var existItem = affiliateClientActivies.FirstOrDefault(x => x.CustomerId == client.ClientId);
                    if (existItem != null)
                    {
                        existItem.SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        existItem.SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        existItem.SportTotalWinAmount =  activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoTotalWinAmount =  activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.TotalTransactions += activies.Select(y => y.Count).DefaultIfEmpty(0).Sum();
                    }
                    else
                        affiliateClientActivies.Add(new ClientActivityModel
                        {
                            CustomerId = client.ClientId,
                            CurrencyId = client.CurrencyId,
                            BTag = client.ClickId,
                            ActivityDate = dateString,
                            BrandId = brandId,
                            SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum(),
                            CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(y => y.BetAmount - y.WinAmount).DefaultIfEmpty(0).Sum(),
                            SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                            CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                            SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                            CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                            SportTotalWinAmount =  activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                            CasinoTotalWinAmount =  activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                            TotalTransactions = activies.Select(y => y.Count).DefaultIfEmpty(0).Sum()
                        });
                }

            }
            return affiliateClientActivies;
        }

        public List<DIMClientActivityModel> GetDIMClientActivity(List<AffiliatePlatformModel> affClients, int brandId, DateTime fromDate, long tDate)
        {
            var affiliateClientActivies = new List<DIMClientActivityModel>();
            var fDate = fromDate.Year * (long)1000000 + fromDate.Month * 10000 + fromDate.Day * 100 + fromDate.Hour;
            var bonusWinAmounts = Db.Documents.Where(x => x.OperationTypeId == (int)OperationTypes.BonusWin && 
                                                          x.Date > fDate && x.Date <= tDate)
                                              .GroupBy(x => x.ClientId)
                                              .Select(x => new { ClientId = x.Key, Amount = x.Select(y => y.Amount).DefaultIfEmpty(0).Sum() }).ToList();
            var dateString = fromDate.ToString("yyyy-MM-dd");
            foreach (var client in affClients)
            {
                var paymentData = Db.PaymentRequests.Where(x => x.ClientId == client.ClientId &&
                                                               (x.Status == (int)PaymentRequestStates.Approved ||
                                                                x.Status == (int)PaymentRequestStates.ApprovedManually) &&
                                                                x.Date >= fDate && x.Date < tDate).GroupBy(x => x.Type).Select(x => new
                                                                {
                                                                    Type = x.Key,
                                                                    Amount = x.Sum(y => y.Amount),
                                                                    Count = x.Count()
                                                                }).ToList();

                if (paymentData.Count != 0)
                {
                    affiliateClientActivies.Add(new DIMClientActivityModel
                    {
                        CustomerId = client.ClientId,
                        BTag = client.ClickId,
                        ActivityDate = dateString,
                        BrandId = brandId,
                        Deposits = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Deposit).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                        Withdrawals = paymentData.Where(x => x.Type == (int)PaymentRequestTypes.Withdraw).Select(x => x.Amount).DefaultIfEmpty(0).Sum(),
                        PaymentTransactions = paymentData.Sum(x => x.Count),
                        CurrencyId = client.CurrencyId
                    });
                }
                var bonusWinAmount = bonusWinAmounts.FirstOrDefault(x => x.ClientId == client.ClientId);
                var activies = Db.Bets.Where(x => x.ClientId == client.ClientId &&
                                                  x.State != (int)BetDocumentStates.Deleted &&
                                                  x.State != (int)BetDocumentStates.Uncalculated &&
                                                  x.BetDate >= fDate && x.BetDate < tDate)
                                      .GroupBy(x => x.ProductId == Constants.SportsbookProductId ? 1 :
                                                    x.ProductId == Constants.PokerProductId ? 3 :
                                                    x.ProductId == Constants.MahjongProductId ? 4 : 2)
                                      .Select(x => new
                                      {
                                          ProductId = x.Key,
                                          BetAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                          WinAmount = x.Where(y => !y.BonusId.HasValue || y.BonusId == 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                          BonusBetAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.BetAmount).DefaultIfEmpty(0).Sum(),
                                          BonusWinAmount = x.Where(y => y.BonusId.HasValue && y.BonusId.Value > 0).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                                          GGR = x.Select(y => y.Rake >= 0 ? y.Rake : (y.BetAmount - (y.BonusAmount == null ? y.WinAmount : y.BonusAmount))).DefaultIfEmpty(0).Sum(),
                                          Count = x.Count()
                                      });

                if (activies.Count() != 0)
                {
                    var existItem = affiliateClientActivies.FirstOrDefault(x => x.CustomerId == client.ClientId);
                    if (existItem != null)
                    {
                        existItem.SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(x => x.GGR).DefaultIfEmpty(0).Sum();
                        existItem.CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(x => x.GGR).DefaultIfEmpty(0).Sum();
                        existItem.PokerGrossRevenue = activies.Where(y => y.ProductId == 3).Select(x => x.GGR).DefaultIfEmpty(0).Sum();
                        existItem.MahjongGrossRevenue = activies.Where(y => y.ProductId == 4).Select(x => x.GGR).DefaultIfEmpty(0).Sum();

                        existItem.SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        existItem.PokerBonusBetsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();
                        existItem.MahjongBonusBetsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum();

                        existItem.SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        existItem.PokerBonusWinsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();
                        existItem.MahjongBonusWinsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum();

                        existItem.SportTotalWinAmount = activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.CasinoTotalWinAmount = activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.PokerTotalWinAmount = activies.Where(y => y.ProductId == 3).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();
                        existItem.MahjongTotalWinAmount = activies.Where(y => y.ProductId == 4).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum();

                        existItem.TotalTransactions += activies.Select(y => y.Count).DefaultIfEmpty(0).Sum();
                        existItem.TotalConvertedBonusAmount = bonusWinAmount?.Amount ?? 0;
                    }
                    else
                        affiliateClientActivies.Add(new DIMClientActivityModel
                        {
                            CustomerId = client.ClientId,
                            CurrencyId = client.CurrencyId,
                            BTag = client.ClickId,
                            ActivityDate = dateString,
                            BrandId = brandId,
                            SportGrossRevenue = activies.Where(y => y.ProductId == 1).Select(x => x.GGR).DefaultIfEmpty(0).Sum(),
                            CasinoGrossRevenue = activies.Where(y => y.ProductId == 2).Select(x => x.GGR).DefaultIfEmpty(0).Sum(),
                            PokerGrossRevenue = activies.Where(y => y.ProductId == 3).Select(x => x.GGR).DefaultIfEmpty(0).Sum(),
                            MahjongGrossRevenue = activies.Where(y => y.ProductId == 4).Select(x => x.GGR).DefaultIfEmpty(0).Sum(),

                            SportBonusBetsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                            CasinoBonusBetsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                            PokerBonusBetsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),
                            MahjongBonusBetsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusBetAmount).DefaultIfEmpty(0).Sum(),

                            SportBonusWinsAmount = activies.Where(y => y.ProductId == 1).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                            CasinoBonusWinsAmount = activies.Where(y => y.ProductId == 2).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                            PokerBonusWinsAmount = activies.Where(y => y.ProductId == 3).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),
                            MahjongBonusWinsAmount = activies.Where(y => y.ProductId == 4).Select(y => y.BonusWinAmount).DefaultIfEmpty(0).Sum(),

                            SportTotalWinAmount = activies.Where(y => y.ProductId == 1).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                            CasinoTotalWinAmount = activies.Where(y => y.ProductId == 2).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                            PokerTotalWinAmount = activies.Where(y => y.ProductId == 3).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),
                            MahjongTotalWinAmount = activies.Where(y => y.ProductId == 4).Select(y => y.WinAmount).DefaultIfEmpty(0).Sum(),

                            TotalTransactions = activies.Select(y => y.Count).DefaultIfEmpty(0).Sum(),
                            TotalConvertedBonusAmount = bonusWinAmount?.Amount ?? 0
                        });
                }
            }
            return affiliateClientActivies;
        }
    }
}