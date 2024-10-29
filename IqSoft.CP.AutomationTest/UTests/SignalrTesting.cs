using Microsoft.AspNetCore.SignalR.Client;
using NUnit.Framework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System;

namespace IqSoft.CP.AutomationTest.UTests
{
    internal class SignalrTesting
    {
        [Test]
        public void ConnectionTesting()
        {

            var connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:57460/apisignalr/basehub")
                .Build();
            connection.StartAsync().Wait();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
        }
    }
}
