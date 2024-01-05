namespace IqSoft.CP.Integration.Payments.Models.Skrill
{
    public enum ReturnUrlTarget
    {
        /// <summary>
        /// Opens the target URL in the full body
        /// of the window - the URL contents fills the entire browser window.
        /// </summary>
        _top = 1,

        /// <summary>
        /// Opens the target URL in the parent frame.
        /// </summary>
        _parent = 2,

        /// <summary>
        /// Opens the target URL in the same frame as the payment form.
        /// </summary>
        _self = 3,

        /// <summary>
        /// Opens the target URL in a new browser window.        
        /// </summary>
        _blank = 4
    }
}
