using System;
using System.Collections.Generic;
using IqSoft.CP.Common;

namespace IqSoft.CP.ProductGateway.Helpers
{
    public static class MicroGamingHelpers
    {
        public static class Methods
        {
            public const string Login = "login";
            public const string GetBalance = "getbalance";
            public const string Play = "play";
            public const string EndGame = "endgame";
            public const string RefreshToken = "refreshtoken";
        }

        public static class Wallets
        {
            public const string Vanguard = "vanguard";
            public const string Local = "local";
        }

        public static class PlayTypes
        {
            public const string Bet = "bet";
            public const string Win = "win";
            public const string ProgressiveWin = "progressivewin";
            public const string Refund = "refund";
            public const string TransferToMgs = "transfertomgs";
            public const string TransferFromMgs = "transferfrommgs";
            public const string TournamentPurchase = "tournamentpurchase";
            public const string Admin = "admin";
        }

        public static Tuple<string, string> GetError(int error)
        {
            if (Errors.ContainsKey(error))
                return Errors[error];
            return Errors[Constants.Errors.GeneralException];
        }

        private readonly static Dictionary<int, Tuple<string, string>> Errors  = new Dictionary<int, Tuple<string, string>>
        {
            {Constants.Errors.GeneralException, Tuple.Create("6000", "Unspecified Error") },
            {Constants.Errors.WrongToken, Tuple.Create("6001", "The player token is invalid.") },
            {Constants.Errors.SessionNotFound, Tuple.Create("6001", "The player token is invalid.") },
            {Constants.Errors.TokenExpired, Tuple.Create("6002", "The player token expired.") },
            {Constants.Errors.SessionExpired, Tuple.Create("6002", "The player token expired.") },
            {Constants.Errors.WrongApiCredentials, Tuple.Create("6003", "The authentication credentials for the API are incorrect.") },                     
            {Constants.Errors.WrongLoginParameters, Tuple.Create("6101", "Login validation failed. Login name or password is incorrect.") },
            {Constants.Errors.ClientNotFound, Tuple.Create("6101", "Login validation failed. Login name or password is incorrect.")},
            {Constants.Errors.WrongPassword, Tuple.Create("6101", "Login validation failed. Login name or password is incorrect.")},
            {Constants.Errors.ClientBlocked, Tuple.Create("6102", "Account is locked.")},
            {Constants.Errors.DocumentNotVerified, Tuple.Create("6102", "Account is locked.")},           
            {Constants.Errors.DocumentAlreadyRollbacked, Tuple.Create("6501", "Already processed with different details.") },//Play (Type = Refund)
            {Constants.Errors.LowBalance, Tuple.Create("6503", "Player has insufficient funds.") },//Play (Bet / TransferToMgs)
            {Constants.Errors.ClientMaxLimitExceeded, Tuple.Create("6505", "The player exceeded their daily protection limit.") },//Play (Bet / TransferToMgs)
            {Constants.Errors.PartnerProductSettingNotFound, Tuple.Create("6510", "The player is not permitted to play this game.") },//Play (Bet / TransferToMgs)
            {Constants.Errors.ProductBlockedForThisPartner, Tuple.Create("6510", "The player is not permitted to play this game.")},//Play (Bet / TransferToMgs)
            {Constants.Errors.ProductNotAllowedForThisPartner, Tuple.Create("6510", "The player is not permitted to play this game.")},//Play (Bet / TransferToMgs)
            {Constants.Errors.WrongProductId, Tuple.Create("6511", "The external system name does not exist (gamereference).")},//Play (Bet / TransferToMgs)
            {Constants.Errors.ProductNotFound, Tuple.Create("6511", "The external system name does not exist (gamereference).")},//Play (Bet / TransferToMgs)
        };        
    }
}