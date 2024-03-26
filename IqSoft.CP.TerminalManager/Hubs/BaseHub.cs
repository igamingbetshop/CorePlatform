using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Serilog;
using IqSoft.CP.TerminalManager.Models;
using IqSoft.CP.TerminalManager.Helpers;

namespace IqSoft.CP.TerminalManager.Hubs
{
    public class BaseHub : Hub
    {
        public static IHubContext<BaseHub> CurrentContext;

        public static ConcurrentDictionary<string, object> ConnectedClients = new ConcurrentDictionary<string, object>();

        public BaseHub(IHubContext<BaseHub> hubContext) : base()
        {
            CurrentContext = hubContext;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e.Message);
                await base.OnConnectedAsync();
            }
        }

        public ConfigurationModel GetAppSetting()
        {
            var configurationModel = new ConfigurationModel
            {
                PartnerId = Program.AppSetting.PartnerId,
                CashDeskId = Program.AppSetting.CashDeskId,
                MacAddress = Program.AppSetting.SerialNumber
            };
            var encryption = new RijndaelEncrypt(Program.AppSetting.Password, Program.AppSetting.Salt, $"{Program.AppSetting.Password}{Program.AppSetting.Salt}");
            configurationModel.Hash = encryption.Encrypt("{\"MacAddress\":\"" + Program.AppSetting.SerialNumber + "\"}");
            return configurationModel;
        }

        public string GetMacAddress()
        {
            return CommonHelpers.GetMotherBoardID();
        }

        public void PrintBetReceipt(BetReceiptModel receiptObject)
        {
            try
            {
                var printReceip = new PrintTicket(receiptObject, Enum.TicketTypes.Bet);
                printReceip.PrintReceipt();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public void PrintWithdrawReceipt(WithdrawReceiptModel receiptObject)
        {
            try
            {
                var printReceip = new PrintTicket(receiptObject, Enum.TicketTypes.Withdraw);
                printReceip.PrintReceipt();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }
}