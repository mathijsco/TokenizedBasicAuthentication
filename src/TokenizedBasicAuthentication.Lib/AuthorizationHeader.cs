namespace WhiteChamber.TokenizedBasicAuthentication.Lib
{
    /// <summary>
    /// Class containing credentials from an Authorization header of a HTTP request.
    /// </summary>
    public class AuthorizationHeader
    {
        /// <summary>
        /// The user name of this authorization header.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The password of this authorization header.
        /// </summary>
        public string Password { get; set; }
    }
}