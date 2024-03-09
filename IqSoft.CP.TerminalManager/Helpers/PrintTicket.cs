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

        public void PrintReceipt()
        {
            PrintDocument pd = new PrintDocument();
            PrinterSettings ps = new PrinterSettings();
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
            pd.Print();
        }

        private void PrintBetTicket(object sender, PrintPageEventArgs ev)
        {
            SizeF valWidth;
            float height = 40;
            string line = "------------------------------------------------------";
            var lineSz = ev.Graphics.MeasureString(line, normalFont);
            height += 30;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.ShopAddress, smallFont);
            ev.Graphics.DrawString(BetReceiptItem.ShopAddress, smallFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            height += 15;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.Title, headingFont);
            ev.Graphics.DrawString(BetReceiptItem.Title, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            Rectangle logoRect = new Rectangle(30, 10, 250, 60);
            ev.Graphics.DrawImage(LogoImg, logoRect, 0, 0, LogoImg.Width, LogoImg.Height, GraphicsUnit.Pixel);

            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.TicketNumber, headingFont);
            ev.Graphics.DrawString(BetReceiptItem.TicketNumber, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            if (BetReceiptItem.IsDuplicate)
            {
                height += 20;
                valWidth = ev.Graphics.MeasureString(Duplicate, headingFont);
                var rect = new RectangleF(5 + (lineSz.Width - valWidth.Width) / 2, height + 2, valWidth.Width + 10, 22);
                ev.Graphics.FillRectangle(Brushes.Black, rect);
                ev.Graphics.DrawString(Duplicate, headingFont, Brushes.White, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            }
            height += 25;
            var date = BetReceiptItem.PrintDate.ToString("yyyy-MM-ddTHH:mm:ss.sss");
            valWidth = ev.Graphics.MeasureString(date, smallFont);
            ev.Graphics.DrawString(date, smallFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            height += 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 15;
            ev.Graphics.DrawString(BetReceiptItem.BetType, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 10;

            //---------print selections----------------
            if (BetReceiptItem.BetDetails.Selections != null && BetReceiptItem.BetDetails.Selections.Any())
                foreach (var selection in BetReceiptItem.BetDetails.Selections)
                {
                    ev.Graphics.DrawString(selection.Id, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                    valWidth = ev.Graphics.MeasureString(selection.CurrentTime, normalFont);
                    ev.Graphics.DrawString(selection.CurrentTime, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                    height += 15;
                    ev.Graphics.DrawString($"{selection.MatchDate}  {selection.Team1}", normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                    valWidth = ev.Graphics.MeasureString(selection.Score, normalFont);
                    ev.Graphics.DrawString(selection.Score, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                    height += 15;
                    ev.Graphics.DrawString(selection.MatchName, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
                    valWidth = ev.Graphics.MeasureString(selection.Coefficient, normalFont);
                    ev.Graphics.DrawString(selection.Coefficient, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
                    height += 20;
                }
            else
                height += 20;

            //-----------------------------------------

            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.BetAmount, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetAmountLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetAmount, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.FeeValue, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.FeeLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.FeeValue, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.BetsNumber.ToString(), normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetsNumberLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.BetsNumber.ToString(), normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.AmountPerBet, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.AmountPerBetLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.AmountPerBet, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.TotalAmount, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.TotalAmountLabel, boldFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.TotalAmount, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += 20;
            valWidth = ev.Graphics.MeasureString(BetReceiptItem.BetDetails.PossibleWin, normalFont);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.PossibleWinLabel, boldFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(BetReceiptItem.BetDetails.PossibleWin, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);

            height += 15;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 20;

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
            height += 30;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Title, headingFont);
            ev.Graphics.DrawString(WithdrawReceiptItem.Title, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            Rectangle logoRect = new Rectangle(30, 10, 250, 60);
            ev.Graphics.DrawImage(LogoImg, logoRect, 0, 0, LogoImg.Width, LogoImg.Height, GraphicsUnit.Pixel);
            if (!string.IsNullOrEmpty(WithdrawReceiptItem.ShopAddress))
            {
                height += 20;
                valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.ShopAddress, headingFont);
                ev.Graphics.DrawString(WithdrawReceiptItem.ShopAddress, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            }
            else
            {
                height += 20;
                var deviceInfo = $"{WithdrawReceiptItem.DeviceIdLabel}: {WithdrawReceiptItem.DeviceId}";
                valWidth = ev.Graphics.MeasureString(deviceInfo, headingFont);
                ev.Graphics.DrawString(deviceInfo, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);

                height += 20;
                var branchInfo = $"{WithdrawReceiptItem.BranchIdLabel}: {WithdrawReceiptItem.BranchId}";
                valWidth = ev.Graphics.MeasureString(branchInfo, headingFont);
                ev.Graphics.DrawString(branchInfo, headingFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            }
            height += 25;
            var printDate = $"{WithdrawReceiptItem.PrintDateLabel}: {WithdrawReceiptItem.PrintDate.ToString("yyyy-MM-ddTHH:mm:ss.sss")}";
            valWidth = ev.Graphics.MeasureString(printDate, normalFont);
            ev.Graphics.DrawString(printDate, normalFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);

            height += 20;
            var withdraw = $"{WithdrawReceiptItem.WithdrawIdLabel}: {WithdrawReceiptItem.WithdrawId}";
            valWidth = ev.Graphics.MeasureString(withdraw, normalFont);
            ev.Graphics.DrawString(withdraw, normalFont, Brushes.Black, 10 + (lineSz.Width - valWidth.Width) / 2, height, DefaultStringFormat);
            height += 20;

            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 20;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Amount, normalFont);
            ev.Graphics.DrawString(WithdrawReceiptItem.AmountLabel, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            ev.Graphics.DrawString(WithdrawReceiptItem.Amount, normalFont, Brushes.Black, 257 - valWidth.Width, height, DefaultStringFormat);
            height += 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);
            height += 20;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.Description, normalFont);
            RectangleF drawRect = new RectangleF(10, height, lineSz.Width, (float)Math.Ceiling(valWidth.Width/(lineSz.Width-20))*valWidth.Height);
            ev.Graphics.DrawString(WithdrawReceiptItem.Description, normalFont, Brushes.Black, drawRect, DefaultStringFormat);
            height += drawRect.Height + 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);

            height += 20;
            valWidth = ev.Graphics.MeasureString(WithdrawReceiptItem.AttentionMessage, normalFont);
            drawRect = new RectangleF(10, height, lineSz.Width, (float)Math.Ceiling(valWidth.Width/(lineSz.Width-20))*valWidth.Height);
            ev.Graphics.DrawString(WithdrawReceiptItem.AttentionMessage, normalFont, Brushes.Black, drawRect, DefaultStringFormat);
            height += drawRect.Height + 20;
            ev.Graphics.DrawString(line, normalFont, Brushes.Black, 10, height, DefaultStringFormat);

            height += 20;
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
