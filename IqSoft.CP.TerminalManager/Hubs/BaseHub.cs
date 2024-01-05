using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Serilog;
using IqSoft.CP.TerminalManager.Models;
using IqSoft.CP.Common.Helpers;
using System.Net.Mail;

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
                MacAddress = Program.AppSetting.HDDSerialNumber
            };
            var encryption = new RijndaelEncrypt(Program.AppSetting.Password, Program.AppSetting.Salt, $"{Program.AppSetting.Password}{Program.AppSetting.Salt}");
            configurationModel.Hash = encryption.Encrypt("{\"MacAddress\":\"" + Program.AppSetting.HDDSerialNumber + "\"}");
            return configurationModel;
        }
    }
}