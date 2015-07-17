Vimeo API
----------------------------------------------------

This project allows you to upload a video to Vimeo (with progress and cancellation support)!  In addition you can set tags, title, description, and privacy options.

This project can someday support all of the possible things you can do with Vimeo; but it is just what we needed at TechSmith.  In other words it can't do everything.  But hopefully you or somebody else will add something here or there and it'll have everything that you would want to do that is supported!

Getting Started:
-----------------------------------------------------

Hopefully you find it easy to use without any documentation.

To get started if you are using this for the first time you don't have what is known as an access token.  So in that case construct the Vimeo object:
var vimeo = new Vimeo(clientid, clientsecret);

Then get the authorization URL;
vimeo.GetUserAuthorizationURL(ref strURL);

Load up this URL in a web browser, login, and accept the permissions.  Copy the WHOLE URL and pass that string in order to obtain an access token:
vimeo.ObtainAccessToken(strWHOLEURL);

Yay!  Now you can upload videos!

If you already had an access token still construct the Vimeo object:
var vimeo = new Vimeo(clientid, clientsecret);

But then just load that access token:
vimeo.LoadAccessToken(strAccessToken);

Now you can upload videos!
