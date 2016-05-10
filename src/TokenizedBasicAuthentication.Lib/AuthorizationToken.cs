using System;

namespace WhiteChamber.TokenizedBasicAuthentication.Lib
{
    /// <summary>
    /// Class containing token information.
    /// </summary>
    public class AuthorizationToken
    {
        /// <summary>
        /// The name of the current user where this token belongs to.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Time when this token will expire.
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// Boolean value indicating whether the current token is temporary and should be replaced with a final version.
        /// Temporary tokens should not have a long life time.
        /// </summary>
        public bool IsTemporary { get; set; }
    }
}