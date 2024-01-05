using IqSoft.CP.Common;
using System.Collections.Generic;

namespace IqSoft.CP.PaymentGateway.Helpers
{
    public class SDPayHelpers
    {
        public enum ResponseCodes
        {
            Success = 100,
            WrongMerchantId = 101,
            WrongString = 102,
            DescryptionFaild = 103,
            WrongCMDCode = 104,
            ClientNotFound = 106,
            UnexpectedException = 105
        }

        public static Dictionary<int, int> ResponseCodesMapping { get; private set; } = new Dictionary<int, int>
        {
           // {Constants.Errors.WrongInputParameters, (int) ResponseCodes.WrongMerchantId},
            {Constants.Errors.WrongInputParameters, (int) ResponseCodes.WrongString},
            {Constants.Errors.WrongHash, (int) ResponseCodes.DescryptionFaild},
            {Constants.Errors.ClientNotFound, (int) ResponseCodes.ClientNotFound},
            {Constants.Errors.GeneralException, (int) ResponseCodes.UnexpectedException}
        };

        public static int GetErrorCode(int ourErrorCode)
        {
            if (ResponseCodesMapping.ContainsKey(ourErrorCode))
                return ResponseCodesMapping[ourErrorCode];
            return (int)ResponseCodes.UnexpectedException;
        }
    }
}