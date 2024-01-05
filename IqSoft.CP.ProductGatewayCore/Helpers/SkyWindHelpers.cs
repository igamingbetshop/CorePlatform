using IqSoft.CP.Common;
using IqSoft.CP.Common.Enums;
using System.Collections.Generic;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public class SkyWindHelpers
    {
        private static class Platforms
        {
            public const string Web = "web";
            public const string Mobile = "mobile";
        }

        public static  DeviceTypes MapDeviceType(string platform)
        {
            if (platform == Platforms.Mobile)
                return DeviceTypes.Mobile;
            if (platform == Platforms.Web)
                return DeviceTypes.Desktop;
            return DeviceTypes.Desktop;
        }

        public enum ResponseCodes
        {
            NoError = 0,
            DuplicateTransaction = 1,
            InternalMerchantError = -1,
            PlayerNotFound = -2,
            TokenExpiered = -3,
            InsufficientBalance = -4,
            InsufficientFreeBet = -5,
            InvalidFreeBet = -6,
            Other = -8
        }

        private readonly static Dictionary<int, int> Errors = new Dictionary<int, int>
        {
            {Constants.SuccessResponseCode, (int) ResponseCodes.NoError},
            {Constants.Errors.TransactionAlreadyExists, (int) ResponseCodes.DuplicateTransaction},
            {Constants.Errors. WrongApiCredentials, (int) ResponseCodes.InternalMerchantError},
            {Constants.Errors.ClientNotFound, (int) ResponseCodes.PlayerNotFound},
            {Constants.Errors.TokenExpired, (int) ResponseCodes.TokenExpiered},
            {Constants.Errors.LowBalance, (int) ResponseCodes.InsufficientBalance},
            {Constants.Errors.GeneralException, (int) ResponseCodes.Other}
        };

        public static int GetErrorCode(int errorId)
        {
            if (Errors.ContainsKey(errorId))
                return Errors[errorId];
            return (int)ResponseCodes.Other;
        }

       
    }
}