# Tokenized Basic Authentication
This project is a simple ASP.net MVC web site with basic authentication.
Because basic authentication requires the user name and password to be sent with each request, this little project helps to prevent that. It will generate a token that authorizes the caller and therefor the *Authorization* header can be cleared.
The token itself will be stored in a session *cookie*.

Advantage of this approach is that you do not need to concern about any login form. Just use the default browser popup.

## How it works
Several requests can be handled by this HttpModule.

1. No authorization header and no token

   Returns 401 to the client with a basic authentication challenge.

2. Authorization header and no token

   Returns 200 to the client with a temporary token and some JavaScript code. This JavaScript code will make sure that the *Authorization* header is cleared. For Internet Explorer and Edge, the authentication cache is cleared. For Chromium based browsers (i.e. Chrome, Opera, Vivaldi), an additional request is done to the server. This additional request will get a status code 401 back, which will invalidate the *Authorization* header. After clearing the header, the page is reloaded.

   The temporary token will be replaced in step 4. This token is now persistent in the browser (so not a session cookie). However this temporary token is only valid for 1 minute.

3. Authorization header and a token

   This will always return a 401 to the client, without a challenge. The additional call of step 2 above, will use this to invalidate the header.

4. No authorization header and a token

   This is the final stage and therefor will return a 200 with the proper page requested. This response will also contains a final token which is valid for 8 hours and only available in the current browser session.

## Token? What is it?
The token is a pretty simple Base64 encoded string with the user name and expiry time. The token is immutable, because of an additional hash that is included. The HttpModule has some static bytes what is included in the hash, so it is not possible for a client to recalculate the hash without knowing it.

An example of a token is: `vi1Bk62dveSCq259LFlH3f4H5nxR3g8AGkDo+/u5wUVtYXRoaWpzCjIwMTYtMDUtMTJUMDE6MzQ6MTguNTE5ODgwMloKMA==`. Try it out, decode it :)

If you decode it, you’d see the last number is a *0*. This indicates whether the token is temporary or not. If this would be a 1, the token will be replaced with the final token and expires after 8 hours. The temporary is only valid for 1 minute.
