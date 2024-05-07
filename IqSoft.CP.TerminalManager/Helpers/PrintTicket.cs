using System.Drawing;
using System.Drawing.Printing;
using BarcodeLib;
using IqSoft.CP.TerminalManager.Enum;
using IqSoft.CP.TerminalManager.Models;
using Color = System.Drawing.Color;

namespace IqSoft.CP.TerminalManager.Helpers
{
    #pragma warning disable CA1416 // Validate platform compatibility
    public class PrintTicket
    {
        BetReceiptModel BetReceiptItem = new BetReceiptModel();
        WithdrawReceiptModel WithdrawReceiptItem = new WithdrawReceiptModel();
        private readonly Image LogoImg = Image.FromFile("logo.png");
        private static readonly string Duplicate = "DUPLICATE";
        private TicketTypes PrintType;

        private static readonly Font headingFont = new Font("Calibri", 16, FontStyle.Bold);

        private static readonly Font boldFont = new Font("Calibri", 13, FontStyle.Bold);
        private static readonly Font normalFont = new Font("Calibri", 11, FontStyle.Regular);
        private static readonly Font middleFont = new Font("Calibri", 10, FontStyle.Regular);
        private static readonly Font smallFont = new Font("Calibri", 9, FontStyle.Regular);
        private StringFormat DefaultStringFormat = new StringFormat();

        public PrintTicket(object receiptItem, TicketTypes ticketType)
        {
            PrintType = ticketType;
            switch (ticketType)
            {
                case TicketTypes.Bet:
                    BetReceiptItem = (BetReceiptModel)receiptItem;
                    break;
                case TicketTypes.Withdraw:
                    WithdrawReceiptItem = (WithdrawReceiptModel)receiptItem;
                    break;
                default:
                    break;
            }
        }

        public void PrintReceipt(string printerName)
        {
            PrintDocument pd = new PrintDocument();
            switch (PrintType)
            {
                case TicketTypes.Bet:
                    pd.PrintPage += new PrintPageEventHandler(PrintBetTicket);
                    break;
                case TicketTypes.Withdraw:
                    pd.PrintPage += new PrintPageEventHandler(PrintWithdrawTicket);
                    break;
                default:
                    break;
            }
            if (!string.IsNullOrEmpty(printerName))
                pd.PrinterSettings.PrinterName = printerName;
            pd.Print();
        }

        private void PrintBetTicket(object sender, PrintPageEventArgs ev)
        {
            SizeF valWidth;
            float height = 40;
            string line = "------------------------------------------------------";
            var lineSz = ev.Graphics.MeasureString(line, normalFont);
            Rectangle logoRect = new Rectangle(30, 10, 250, 60);
            ev.Graphics.DrawImage(LogoImg, logoRect, 0, 0, LogoImg.Width, LogoImg.Height, GraphicsUnit.Pixel);
            height += 30;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.ShopAddress, smallFont);
            ev.Graphics.DrawString(BetReceiptItem.ShopAddress, smallFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.Title, headingFont);
            ev.Graphics.DrawString(BetReceiptItem.Title, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);          

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.TicketNumber, headingFont);
            ev.Graphics.DrawString(BetReceiptItem.TicketNumber, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            if (BetReceiptItem.IsDuplicate)
            {
                height += valWidth.Height;
                valWidth = ev.Graphics.MeasureString(Duplicate, headingFont);
                var rect = new RectangleF(5 + (lineSz.Width - valWidth.Width) / 2, height + 2, valWidth.Width + 10, 22);
                ev.Graphics.FillRectangle(Brushes.Black, rect);
                ev.Graphics.DrawString(Duplicate, headingFont, Brushes.White, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            }
            height += valWidth.Height;
            var date = BetReceiptItem.PrintDate;
            valWidth = ev.Graphics.MeasureString(date, smallFont);
            ev.Graphics.DrawString(date, smallFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            height += valWidth.Height;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += lineSz.Height;

            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetType, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetType, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += valWidth.Height;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += lineSz.Height;

            //---------print selections----------------
            if (BetReceiptItem.BetDetails.Selections != null && BetReceiptItem.BetDetails.Selections.Any())
            {
                foreach (var selection in BetReceiptItem.BetDetails.Selections)
                {                    
                    if (!string.IsNullOrEmpty(selection.Team1))
                    {
                        ev.Graphics.DrawString(selection.Id, smallFont, Brushes.Black, 10, height, DefaultStringFormat);
                        valWidth = ev.Graphics.MeasureString(selection.CurrentTime, smallFont);
                        ev.Graphics.DrawString(selection.CurrentTime, smallFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                        height += valWidth.Height==0 ? 15 : valWidth.Height;

                        ev.Graphics.DrawString($"{selection.MatchDate}  {selection.Team1}", smallFont, Brushes.Black, 10, height, DefaultStringFormat);
                        valWidth = ev.Graphics.MeasureString(selection.Score, smallFont);
                        ev.Graphics.DrawString(selection.Score, smallFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                        height += valWidth.Height==0 ? 15 : valWidth.Height;
                        ev.Graphics.DrawString($"{selection.MatchTime}  {selection.Team2}", smallFont, Brushes.Black, 10, height, DefaultStringFormat);
                        height += valWidth.Height==0 ? 15 : valWidth.Height;
                        ev.Graphics.DrawString(selection.MatchName, smallFont, Brushes.Black, 10, height, DefaultStringFormat);
                        height += valWidth.Height==0 ? 15 : valWidth.Height;
                        ev.Graphics.DrawString(selection.SelectionName, smallFont, Brushes.Black, 10, height, DefaultStringFormat);
                        valWidth = ev.Graphics.MeasureString(selection.Coefficient, smallFont);
                        ev.Graphics.DrawString(selection.Coefficient, smallFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);                                             
                        height += valWidth.Height==0 ? 15 : valWidth.Height+5;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(selection.Id) && selection.Id!= "0")
                            ev.Graphics.DrawString(selection.Id, middleFont, Brushes.Black, 10, height, DefaultStringFormat);
                        valWidth = ev.Graphics.MeasureString(selection.CurrentTime, middleFont);
                        ev.Graphics.DrawString(selection.CurrentTime, middleFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                        height += valWidth.Height==0 ? 15 : valWidth.Height;
                        if (!string.IsNullOrEmpty(selection.Id) && selection.Id!= "0")
                        {
                            ev.Graphics.DrawString(selection.Id, middleFont, Brushes.Black, 10, height, DefaultStringFormat);
                            valWidth = ev.Graphics.MeasureString(selection.Id, normalFont);
                            height += valWidth.Height;
                        }
                        valWidth = ev.Graphics.MeasureString(selection.SelectionName, normalFont);
                        if (!string.IsNullOrEmpty(selection.SelectionName))
                        {
                            ev.Graphics.DrawString(selection.SelectionName, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                            height += valWidth.Height;
                        }
                        valWidth = ev.Graphics.MeasureString(selection.EventInfoLabel, normalFont);
                        ev.Graphics.DrawString(selection.EventInfoLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                        var valSz = ev.Graphics.MeasureString(selection.EventInfo, normalFont);
                        if (valWidth.Width + valSz.Width + 10 > lineSz.Width)
                        {
                            height += valWidth.Height;
                            var rowsCount = (int)Math.Ceiling(valSz.Width / lineSz.Width);
                            var rect = new RectangleF(10, height, 252, rowsCount * valSz.Height);
                            ev.Graphics.DrawString(selection.EventInfo, normalFont, Brushes.Black, rect);
                            height += rect.Height;
                        }
                        else
                        {
                            ev.Graphics.DrawString(selection.EventInfo, normalFont, Brushes.Black, lineSz.Width - valSz.Width, height, DefaultStringFormat);
                            height += valSz.Height;
                        }
                        valWidth = ev.Graphics.MeasureString(selection.RoundId, normalFont);
                        ev.Graphics.DrawString(selection.RoundIdLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                        ev.Graphics.DrawString(selection.RoundId, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                        height += valWidth.Height;

                    }
                }
                ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                height += lineSz.Height;
            }
            //-----------------------------------------
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.BetAmount, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetAmountLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetAmount, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.FeeValue, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.FeeLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.FeeValue, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.BetsNumber.ToString(), normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetsNumberLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetsNumber.ToString(), normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.AmountPerBet, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.AmountPerBetLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.AmountPerBet, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.TotalAmount, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.TotalAmountLabel, boldFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.TotalAmount, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.PossibleWin, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.PossibleWinLabel, boldFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.PossibleWin, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += valWidth.Height;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += valWidth.Height;

            var barcode = new Barcode();
            int imageWidth = 250;
            int imageHeight = 90;

            // Generate the barcode with your settings
            Image barcodeImage = barcode.Encode(TYPE.CODE39, BetReceiptItem.Barcode, Color.Black, Color.Transparent, imageWidth, imageHeight);
            logoRect = new Rectangle(10, (int)height, 250, 70);
            ev.Graphics.DrawImage(barcodeImage, logoRect, 0, 0, 250, 90, GraphicsUnit.Pixel);
            height += 70;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.Barcode, smallFont);
            ev.Graphics.DrawString(BetReceiptItem.Barcode, smallFont, Brushes.Black, 10 + (barcodeImage.Width - valWidth.Width) / 2, height, DefaultStringFormat);

            ev.HasMorePages = false;
        }

        private void PrintWithdrawTicket(object sender, PrintPageEventArgs ev)
        {
            SizeF valWidth;
            float height = 40;
            string line = "------------------------------------------------------";
            var lineSz = ev.Graphics.MeasureString(line, normalFont);
            Rectangle logoRect = new Rectangle(30, 10, 250, 60);
            ev.Graphics.DrawImage(LogoImg, logoRect, 0, 0, LogoImg.Width, LogoImg.Height, GraphicsUnit.Pixel);
            height += 30;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Title, headingFont);
            ev.Graphics.DrawString(WithdrawReceiptItem.Title, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            
            height += valWidth.Height;
            if (!string.IsNullOrEmpty(WithdrawReceiptItem.ShopAddress))
            {
                valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.ShopAddress, headingFont);
                ev.Graphics.DrawString(WithdrawReceiptItem.ShopAddress, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            }
            else
            {
                var deviceInfo = $"{WithdrawReceiptItem.DeviceIdLabel}: {WithdrawReceiptItem.DeviceId}";
                valWidth = ev.Graphics.MeasureString(deviceInfo, headingFont);
                ev.Graphics.DrawString(deviceInfo, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);

                height += valWidth.Height;
                var branchInfo = $"{WithdrawReceiptItem.BranchIdLabel}: {WithdrawReceiptItem.BranchId}";
                valWidth = ev.Graphics.MeasureString(branchInfo, headingFont);
                ev.Graphics.DrawString(branchInfo, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            }
            height += valWidth.Height;
            var printDate = $"{WithdrawReceiptItem.PrintDateLabel}: {WithdrawReceiptItem.PrintDate}";
            valWidth = ev.Graphics.MeasureString(printDate, normalFont);
            ev.Graphics.DrawString(printDate, normalFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);

            height += valWidth.Height;
            var withdraw = $"{WithdrawReceiptItem.WithdrawIdLabel}: {WithdrawReceiptItem.WithdrawId}";
            valWidth = ev.Graphics.MeasureString(withdraw, normalFont);
            ev.Graphics.DrawString(withdraw, normalFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            height += valWidth.Height;

            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Amount, normalFont);
            ev.Graphics.DrawString(WithdrawReceiptItem.AmountLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(WithdrawReceiptItem.Amount, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
            height += valWidth.Height;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Description, normalFont);
            RectangleF drawRect = new RectangleF(10, height, lineSz.Width, (float)Math.Ceiling(valWidth.Width/(lineSz.Width-20))*valWidth.Height);
            ev.Graphics.DrawString(WithdrawReceiptItem.Description, normalFont, Brushes.Black, drawRect, DefaultStringFormat);
            height += drawRect.Height + 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);

            height += valWidth.Height;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.AttentionMessage, normalFont);
            drawRect = new RectangleF(10, height, lineSz.Width, (float)Math.Ceiling(valWidth.Width/(lineSz.Width-20))*valWidth.Height);
            ev.Graphics.DrawString(WithdrawReceiptItem.AttentionMessage, normalFont, Brushes.Black, drawRect, DefaultStringFormat);
            height += drawRect.Height + 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);

            height += valWidth.Height;
            var barcode = new Barcode();
            int imageWidth = 250;
            int imageHeight = 90;

            // Generate the barcode with your settings
            Image barcodeImage = barcode.Encode(TYPE.CODE39, WithdrawReceiptItem.Barcode, Color.Black, Color.Transparent, imageWidth, imageHeight);
            logoRect = new Rectangle(10, (int)height, 250, 70);
            ev.Graphics.DrawImage(barcodeImage, logoRect, 0, 0, 250, 90, GraphicsUnit.Pixel);
            height += 70;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Barcode, smallFont);

            ev.Graphics.DrawString(WithdrawReceiptItem.Barcode, smallFont, Brushes.Black, 10 + (barcodeImage.Width - valWidth.Width) / 2, height, DefaultStringFormat);
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
