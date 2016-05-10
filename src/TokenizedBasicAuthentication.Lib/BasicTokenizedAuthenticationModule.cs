using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;

namespace WhiteChamber.TokenizedBasicAuthentication.Lib
{
    /// <summary>
    /// Class that acts as a HttpModule which uses basic authentication to authorize requests. 
    /// The authorization header of the requests are transformed to a token, which will prevent the credentials being send multiple times.
    /// </summary>
    public class BasicTokenizedAuthenticationModule : IHttpModule
    {
        private const double TokenExpiryInHours = 8;
        private const string TokenCookieName = "AuthToken";

        private readonly SecureAuthorizationHelper _secureAuthorizationHelper;

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicTokenizedAuthenticationModule"/> class.
        /// </summary>
        public BasicTokenizedAuthenticationModule()
        {
            _secureAuthorizationHelper = new SecureAuthorizationHelper();
        }

        /// <summary>
        /// Initializes a module and prepares it to handle requests.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpApplication"/> that provides access to the methods, properties, and events common to all application objects within an ASP.NET application</param>
        public void Init(HttpApplication context)
        {
            context.AuthenticateRequest += OnAuthenticateRequest;
        }

        /// <summary>
        /// Disposes of the resources (other than memory) used by the module that implements <see cref="T:System.Web.IHttpModule"/>.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Checks the credentials if they are valid.
        /// </summary>
        /// <param name="username">The name of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>True if the credentials are valid, otherwise false.</returns>
        private bool ValidateCredentials(string username, string password)
        {
            // This function can contain any logic to validate the user and password.
            // Currently a static membership provider is configured in the web.config. 
            // This can be any provider, like Active Directory or some database.

            return Membership.ValidateUser(username, password);
        }

        /// <summary>
        /// Handles the AuthenticateRequest event of the context control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnAuthenticateRequest(object sender, EventArgs e)
        {
            var context = HttpContext.Current;
            if (context == null || !DoRequest(context))
            {
                var application = (HttpApplication)sender;
                application.CompleteRequest();
            }
        }

        private bool DoRequest(HttpContext context)
        {
            /* SCENARIOS:
             * 1. No authorization header and no cookie: 401 Unauthorized
             * 2. Authorization header but no cookie: Login, create cookie and return JS code to get 401 in browser in JavaScript
             * 3. No authorization header but cookie: 200 OK
             * 
             * 4. Authorization header and cookie: Return JS code to get 401 in browser in JavaScript
             */

            // Read the cookie
            AuthorizationToken authCookie = null;
            var authCookieOriginal = context.Request.Cookies.Get(TokenCookieName);
            if (authCookieOriginal != null)
                authCookie = _secureAuthorizationHelper.DeserializeToken(authCookieOriginal.Value);

            // Read the header
            var authHeader = _secureAuthorizationHelper.DeserializeHeader(context.Request.Headers["Authorization"]);
            // Check if it is specified, for easier readability.
            var hasHeader = authHeader != null;
            var hasCookie = authCookie != null;


            // 4. Return 401 when both cookie and header are set
            if (hasHeader && hasCookie)
            {
                UserIsUnauthorized(context, false);
                return false;
            }

            // 3. No authorization header but cookie: 200 OK
            if (!hasHeader && hasCookie)
            {
                if (authCookie.IsTemporary)
                {
                    authCookie.ExpirationTime = DateTime.UtcNow.AddHours(TokenExpiryInHours);
                    authCookie.IsTemporary = false;
                    SetCookie(context, _secureAuthorizationHelper.SerializeToken(authCookie));
                }

                SetCurrentPrincipal(context, authCookie.UserName);
                return true;
            }

            // 2. Authorization header but no cookie: Login, create cookie and return JS code to get 401 in browser in JavaScript
            if (hasHeader && !hasCookie && ValidateCredentials(authHeader.UserName, authHeader.Password))
            {
                var cookieValue = _secureAuthorizationHelper.SerializeToken(new AuthorizationToken
                {
                    UserName = authHeader.UserName,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(1),
                    IsTemporary = true
                });
                SetCookie(context, cookieValue, DateTime.UtcNow.AddHours(TokenExpiryInHours));

                GenerateLogoutPage(context.Response);
                return false;
            }

            // If we are not yet validated; make sure we authenticate.
            UserIsUnauthorized(context, true);

            return false;
        }

        private void UserIsUnauthorized(HttpContext context, bool challenge)
        {
            context.Response.Status = "401 Unauthorized";
            context.Response.StatusCode = 401;

            if (challenge)
                context.Response.AddHeader("WWW-Authenticate", "Basic realm=\"" + context.Request.Url.Host + "\"");
        }

        private void GenerateLogoutPage(HttpResponse response)
        {
            var message = @"Signing in... Please wait.";
            // The JS below does a HEAD request. If it is not supported, change it to GET.
            var js = @"var ieResult; try { if (ieResult = document.execCommand('ClearAuthenticationCache','false')) { location.reload(); } } catch (err) { } if (!ieResult) { if (window.crypto && typeof window.crypto.logout === 'function'){ window.crypto.logout(); location.reload(); }else{ var xmlhttp = new XMLHttpRequest(); xmlhttp.onreadystatechange = function() { if (xmlhttp.readyState == XMLHttpRequest.DONE ){ location.reload(); } }; xmlhttp.open('HEAD', 'logout', true); xmlhttp.send(); } }";

            response.ClearContent();
            response.ContentType = "text/html";

            response.Output.Write(@"<html><head><script language=""javascript"">");
            response.Output.Write(js);
            response.Output.Write(@"</script></head><body>");
            response.Output.Write(message);
            response.Output.Write(@"</body></html>");
        }

        private static void SetCookie(HttpContext context, string value, DateTime expired = default(DateTime))
        {
            context.Response.Cookies.Remove(TokenCookieName);
            context.Response.Cookies.Add(new HttpCookie(TokenCookieName, value)
            {
                HttpOnly = true,
                Secure = context.Request.IsSecureConnection,
                Expires = expired,
                Path = context.Request.ApplicationPath
            });
        }

        /// <summary>
        /// Sets the currrent principal and its identity as specified.
        /// </summary>
        private void SetCurrentPrincipal(HttpContext context, string userName)
        {
            IPrincipal principal = new GenericPrincipal(new GenericIdentity(userName), null);
            Thread.CurrentPrincipal = principal;
            context.User = principal;
        }
    }
}