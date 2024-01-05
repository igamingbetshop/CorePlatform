namespace IqSoft.CP.Common.Enums
{
    public enum ClientStates
    {
        Active = 1,
        BlockedForDeposit = 2,
        BlockedForBet = 3,
        BlockedForWithdraw = 4,
        ForceBlock = 5,
        FullBlocked = 6000,
        Suspended = 7,
        Disabled = 8
    }
}