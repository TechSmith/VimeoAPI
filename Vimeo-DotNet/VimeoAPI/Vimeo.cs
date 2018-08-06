using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Configuration;

namespace VimeoAPI
{
   public class Vimeo
   {
      private long m_lChunkSize = 1048576;

      protected string m_strClientID;
      protected string m_strClientSecret;
      protected string m_strRedirectURI = @"http://localhost";
      protected VimeoScope m_eScope = VimeoScopeExtensions.GetBasicUploadScope();
      public Vimeo(string strClientID, string strClientSecret)
      {
         VimeoLogger.Log("Vimeo::Vimeo");
         m_strClientID = strClientID;
         m_strClientSecret = strClientSecret;

         Debug.Assert(!string.IsNullOrEmpty(m_strClientID) && !string.IsNullOrEmpty(m_strClientSecret));//Required!
      }

      public bool GetUserAuthorizationURL(ref string strURL)
      {
         VimeoLogger.Log("Vimeo::GetUserAuthorizationURL");
         //Field           Required    Description
         //response_type   Yes         MUST be set to "code"
         //client_id       Yes         Your client identifier
         //redirect_uri    Yes         This must be required, and must match your app callback URL
         //scope           No          Defaults to "public". This is a space-separated list of scopes you want to access
         //state           Yes         A unique value which the client will return alongside access tokens

         string strScopeValue = m_eScope.GetSpaceSeparatedValue();
         Debug.Assert(!string.IsNullOrEmpty(strScopeValue));//Should not happen!
         Debug.Assert(!string.IsNullOrEmpty(m_strClientID));
         Debug.Assert(!string.IsNullOrEmpty(m_strRedirectURI));

         IDictionary<string, string> KeyValues = new Dictionary<string, string>
         {
            {"response_type"  , "code"          },
            {"client_id"      , m_strClientID   },
            {"redirect_uri"   , m_strRedirectURI},
            {"scope"          , strScopeValue   },
            //{"state"          , String.Empty    }Not really required :)
         };
         string strQueryParams = GenerateQueryParams(KeyValues);

         strURL = BuildURL(Endpoints.Authorize, strQueryParams);
         return true;
      }

      private string GenerateQueryParams(IDictionary<string, string> KeyValues)
      {
         VimeoLogger.Log("Vimeo::GenerateQueryParams");
         if (KeyValues.Count == 0)
         {
            return string.Empty;
         }

         string strRet = string.Empty;

         foreach (var pair in KeyValues)
         {
            if (!string.IsNullOrEmpty(strRet))
            {
               strRet += "&";
            }
            strRet += string.Format("{0}={1}",
               HttpUtility.UrlEncode(pair.Key),
               HttpUtility.UrlEncode(pair.Value));
         }

         return strRet;
      }

      private string BuildURL(string strPath, string strParams)
      {
         VimeoLogger.Log("Vimeo::BuildURL");
         Debug.Assert(strPath.StartsWith("/"));//This is expected

         return string.Format("https://api.vimeo.com{0}{1}", strPath,
            string.IsNullOrEmpty(strParams) ? string.Empty : "?" + strParams);
      }

      public bool ObtainAccessToken(string strURL)
      {
         VimeoLogger.Log("Vimeo::ObtainAccessToken");
         if (!StartsWithCallbackURL(strURL))
         {
            return false;
         }

         if (strURL.Contains("error"))
         {
            return false;
         }

         string strAccessCode = strURL.Substring(strURL.IndexOf("?") + 6); /*The +6 is to get after the ?code=*/

         return ExchangeCodeForAccessTokens(strAccessCode);
      }

      private string m_strAccessToken;
      private string m_strUserName;

      private bool ExchangeCodeForAccessTokens(string strAccessCode)
      {
         VimeoLogger.Log("Vimeo::ExchangeCodeForAccessTokens");
         m_strAccessToken = string.Empty;

         Debug.Assert(!StartsWithCallbackURL(strAccessCode));//Call ObtainAccessToken
         string strURL = BuildURL(Endpoints.AccessToken, string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         //The authorization header can be found on your app page or can be built with your client id and client secret
         //'Authorization: basic ' + base64(client_id + ':' + client_secret)
         string strAuthorization = string.Format("{0}:{1}", m_strClientID, m_strClientSecret);
         string strAuthorizationBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(strAuthorization));
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization", string.Format("Basic {0}", strAuthorizationBase64)}
         };
         request.Method = @"POST";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         try
         {
            {
               //Field        Required    Description
               //grant_type   Yes         Must be set to "authorization_code"
               //code         Yes         The authorization code received from the authorization server.
               //redirect_uri Yes         Must match the redirect uri from the previous step
               IDictionary<string, string> KeyValues = new Dictionary<string, string>
               {
                  {"grant_type"     , "authorization_code" },
                  {"code"           , strAccessCode        },
                  {"redirect_uri"   , m_strRedirectURI     }
               };
               string strQueryParams = GenerateQueryParams(KeyValues);

               var bytesData = Encoding.UTF8.GetBytes(strQueryParams);
               request.ContentLength = bytesData.Length;
               Stream requestStream = request.GetRequestStream();
               requestStream.Write(bytesData, 0, bytesData.Length);
               requestStream.Close();
            }

            //Do call
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log(response);
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::ExchangeCodeForAccessTokens; response: " + strResponse);
            r.Close();
            responseStream.Close();
            response.Close();

            AccessTokenResponse accessToken = null;

            try
            {
               accessToken = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessTokenResponse>(strResponse);

               //Check scope
               foreach (var strScope in m_eScope.GetFlagsString())
               {
                  if (!accessToken.scope.Contains(strScope))
                  {
                     return false;
                  }
               }

               //Save off access_token for authenticated requests
               m_strAccessToken = accessToken.access_token;

               //Save off name too :)
               m_strUserName = accessToken.user.name;
            }
            catch (System.OverflowException ex)
            {
               VimeoLogger.Log(ex);
               BackupParseForAccessToken(strResponse);
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
               VimeoLogger.Log(ex);
               BackupParseForAccessToken(strResponse);
            }
         }
         catch (WebException wex)
         {
            VimeoLogger.Log(wex);
            return false;
         }

         //Return true if I have an access_token :)
         return !string.IsNullOrEmpty(m_strAccessToken);
      }

      public void BackupParseForAccessToken(string strResponse)
      {
         VimeoLogger.Log("Vimeo::BackupParseForAccessToken");
         //This sucks!  It should hopefully NEVER HAPPEN but it could potentially for some user account.
         //You have to do something!
         int nStart = strResponse.IndexOf("\"access_token\":\"");
         int nEnd = strResponse.IndexOf("\",\"", nStart);
         string strAccessToken = strResponse.Substring(nStart, nEnd - nStart);
         m_strAccessToken = string.Join( string.Empty, strAccessToken.Split(new char[] { '\"', ',' }, StringSplitOptions.RemoveEmptyEntries));
      }

      public static bool StartsWithCallbackURL(string strURL)
      {
         VimeoLogger.Log("Vimeo::StartsWithCallbackURL");
         return strURL.StartsWith(@"http://localhost");
      }

      public bool GetAccessToken(ref string strAccessToken)
      {
         VimeoLogger.Log("Vimeo::GetAccessToken");
         strAccessToken = m_strAccessToken;
         return !string.IsNullOrEmpty(strAccessToken);
      }

      private string m_strClipURI;

      public enum Privacy
      {
         Anybody,
         Nobody,
         Contacts,
         Password
      }

      public bool Upload(string strFile, string strTitle, string strDescription, string strTags, Privacy ePrivacy, string strPassword, IProgress pProgress)
      {
         VimeoLogger.Log("Vimeo::Upload");
         Debug.Assert(File.Exists(strFile));

         Debug.Assert(!string.IsNullOrEmpty(m_strAccessToken));//Did you call ObtainAccessToken?
         if ( string.IsNullOrEmpty(m_strAccessToken))
         {
            return false;
         }

         if (pProgress != null && pProgress.GetCanceled())
         {
            return false;
         }

         UploadTicketResponse ticket = GenerateUploadTicket();

         //Make sure ticket is good! :)
         if (ticket == null || string.IsNullOrWhiteSpace(ticket.ticket_id))
         {
            return false;
         }

         if (pProgress != null && pProgress.GetCanceled())
         {
            return false;
         }

         bool bOK = UploadFile(ticket.upload_link_secure, strFile, ticket.complete_uri, pProgress);

         if (pProgress != null && pProgress.GetCanceled())
         {
            return false;
         }

         if (bOK)
         {
            bOK = SetVideoInformation(m_strClipURI, strTitle, strDescription);
         }

         if (pProgress != null && pProgress.GetCanceled())
         {
            return false;
         }

         if ( bOK )
         {
            bool bSetPrivacy = SetPrivacyInformation( m_strClipURI, ePrivacy, strPassword );
            if ( !bSetPrivacy )
            {
               //TODO: keep note; but not fail upload.
            }
         }

         if (pProgress != null && pProgress.GetCanceled())
         {
            return false;
         }

         if (bOK)
         {
            bOK = SetVideoTags(m_strClipURI, strTags);
         }

         if (pProgress != null && pProgress.GetCanceled())
         {
            return false;
         }

         return bOK;
      }

      private UploadTicketResponse GenerateUploadTicket()
      {
         VimeoLogger.Log("Vimeo::GenerateUploadTicket");
         string strURL = BuildURL(Endpoints.UploadTicket, string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization", string.Format("bearer {0}", m_strAccessToken)}
         };
         request.Method = @"POST";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         UploadTicketResponse uploadTicket = null;
         try
         {
            {
               //Field           Required       Description
               //type            Yes            Must be set to streaming
               //upgrade_to_1080 No             If set to true, and you have the ability to create 1080p videos, we will create a 1080p version of your video. Check out the FAQ for more information.
               IDictionary<string, string> KeyValues = new Dictionary<string, string>
               {
                  {"type"     , "streaming" }
               };
               string strQueryParams = GenerateQueryParams(KeyValues);

               var bytesData = Encoding.UTF8.GetBytes(strQueryParams);
               request.ContentLength = bytesData.Length;
               Stream requestStream = request.GetRequestStream();
               requestStream.Write(bytesData, 0, bytesData.Length);
               requestStream.Close();
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log(response);
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::GenerateUploadTicket; response: " + strResponse);
            r.Close();
            responseStream.Close();
            response.Close();

            uploadTicket =
                 Newtonsoft.Json.JsonConvert.DeserializeObject<UploadTicketResponse>(strResponse);
         }
         catch (WebException wex)
         {
            VimeoLogger.Log(wex);
            m_strErrorMessage = wex.Message;
            return null;
         }

         return uploadTicket;
      }

      private bool UploadFile(string strURL, string strFile, string strCompleteURI, IProgress pProgress)
      {
         VimeoLogger.Log("Vimeo::UploadFile");
         Debug.Assert(File.Exists(strFile));
         Debug.Assert(!string.IsNullOrWhiteSpace(strURL));

         m_strClipURI = null;

         FileInfo fi = new FileInfo(strFile);
         long lFileSize = fi.Length;

         FileStream fstream = File.OpenRead(strFile);

         long lChunkSize = m_lChunkSize;

         byte[] buffer = new byte[lChunkSize];
         long lStart = 0;
         while (true)
         {
            if (pProgress != null && pProgress.GetCanceled())
            {
               fstream.Close();
               return false;
            }

            long lEnd = Math.Min(lStart + lChunkSize, lFileSize);
            long lBytes = lEnd - lStart;

            HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
            request.Method = @"PUT";
            request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
            request.ContentType = @"video/mp4";
            request.Headers = new WebHeaderCollection()
            {
               {"Content-Range", string.Format("bytes {0}-{1}/{2}", lStart, lEnd, lFileSize)}
            };
            request.KeepAlive = false;

            try
            {
               {
                  long lRead = (long)fstream.Read(buffer, 0, (int)lBytes);
                  Debug.Assert(lRead == lBytes);
                  request.ContentLength = lRead;
                  Stream requestStream = request.GetRequestStream();
                  requestStream.Write(buffer, 0, (int)lRead);
                  requestStream.Close();
               }

               //Do call
               HttpWebResponse response = request.GetResponse() as HttpWebResponse;
               VimeoLogger.Log(response);

               HttpStatusCode status = response.StatusCode;
               //Successful uploads with have a HTTP 200 status code. A 501 error means you did not perform a PUT or the request was malformed.

               //Not sure exactly what to do if I get an error :(
               if (status != HttpStatusCode.OK)
               {
                  return false;
               }

               Stream responseStream = response.GetResponseStream();
               StreamReader r = new StreamReader(responseStream);
               string strResponse = r.ReadToEnd();
               VimeoLogger.Log("Vimeo::UploadFile; response: " + strResponse);
               r.Close();
               responseStream.Close();
               response.Close();

               if (pProgress != null)
               {
                  double dPercentage = lEnd * 100.0 / lFileSize;
                  pProgress.SetProgress(dPercentage);
               }

               lStart += lBytes;
               if (lEnd >= lFileSize)
               {
                  break;
               }
            }
            catch (WebException wex)
            {
               VimeoLogger.Log(wex);
               m_strErrorMessage = wex.Message;
               fstream.Close();
               return false;
            }
         }

         fstream.Close();

         //Verify the upload
         bool bOK = VerifyUpload(strURL);

         if (bOK)
         {
            bOK = CompleteUpload(strCompleteURI);
         }

         return bOK;
      }

      private bool VerifyUpload(string strURL)
      {
         VimeoLogger.Log("Vimeo::VerifyUpload");

         HttpStatusCode status = HttpStatusCode.Unused;
         {
            Debug.Assert(!string.IsNullOrWhiteSpace(strURL));

            HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
            request.Method = @"PUT";
            request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
            request.Headers = new WebHeaderCollection()
            {
               {"Content-Range"  , "bytes */*"}
            };
            request.ContentLength = 0;
            request.KeepAlive = false;

            //Do call
            HttpWebResponse response = null;
            try
            {
               response = request.GetResponse() as HttpWebResponse;
               VimeoLogger.Log(response);
            }
            catch (WebException ex)
            {
               VimeoLogger.Log(ex);
               //This happens for me.  I think it is because I did an upload and didn't complete the upload
               response = ex.Response as HttpWebResponse;
            }
            status = response.StatusCode;
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::VerifyUpload; response: " + strResponse);
            r.Close();
            responseStream.Close();
            response.Close();
         }

         return status == HttpStatusCode.OK 
            || ((int)status) == 308;//"The remote server return an error: (308) Resume Incomplete."
      }

      private bool CompleteUpload(string strCompleteUri)
      {
         VimeoLogger.Log("Vimeo::CompleteUpload");
         Debug.Assert(!string.IsNullOrWhiteSpace(strCompleteUri));

         string strURL = BuildURL(strCompleteUri, string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization", string.Format("bearer {0}", m_strAccessToken)}
         };
         request.Method = @"DELETE";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.KeepAlive = false;

         //Do call
         HttpStatusCode status = HttpStatusCode.OK;
         try
         {
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log(response);
            status = response.StatusCode;

            string strClipURI = response.Headers["Location"];
            m_strClipURI = strClipURI;
            VimeoLogger.Log("Vimeo::CompleteUpload; ClipURI: " + m_strClipURI);

            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::CompleteUpload; response: " + strResponse);
            r.Close();
            responseStream.Close();
            response.Close();
         }
         catch (WebException wex)
         {
            VimeoLogger.Log(wex);
            m_strErrorMessage = wex.Message;
            return false;
         }

         return status == HttpStatusCode.Created;
      }

      public bool SetVideoInformation(string strClipURI, string strTitle, string strDescription)
      {
         VimeoLogger.Log("Vimeo::SetVideoInformation");
         Debug.Assert( !string.IsNullOrEmpty( strClipURI ) );
         if ( string.IsNullOrEmpty( strClipURI ) )
         {
            VimeoLogger.Log("Vimeo::SetVideoInformation; No Clip URI");
            return false;
         }

         string strURL = BuildURL(strClipURI, string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization"  , string.Format("bearer {0}", m_strAccessToken) }
         };
         request.Method = @"PATCH";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         //Do call
         HttpStatusCode status = HttpStatusCode.OK;
         try
         {
            {
               IDictionary<string, string> KeyValues = new Dictionary<string, string>
               {
                  {"name"        , strTitle                                      },
                  {"description" , strDescription                                }
               };
               string strQueryParams = GenerateQueryParams(KeyValues);

               VimeoLogger.Log("Vimeo::SetVideoInformation; name: " + strTitle);
               VimeoLogger.Log("Vimeo::SetVideoInformation; description: " + strDescription);

               var bytesData = Encoding.UTF8.GetBytes(strQueryParams);
               request.ContentLength = bytesData.Length;
               Stream requestStream = request.GetRequestStream();
               requestStream.Write(bytesData, 0, bytesData.Length);
               requestStream.Close();
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log(response);
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::SetVideoInformation; response: " + strResponse);
            status = response.StatusCode;
            r.Close();
            responseStream.Close();
            response.Close();
         }
         catch (WebException wex)
         {
            VimeoLogger.Log(wex);
            return false;
         }

         return status == HttpStatusCode.OK;
      }

      public bool SetPrivacyInformation( string strClipURI, Privacy ePrivacy, string strPassword )
      {
         VimeoLogger.Log( "Vimeo::SetPrivacyInformation" );
         Debug.Assert( !string.IsNullOrEmpty( strClipURI ) );
         if ( string.IsNullOrEmpty( strClipURI ) )
         {
            VimeoLogger.Log( "Vimeo::SetPrivacyInformation; No Clip URI" );
            return false;
         }

         string strURL = BuildURL( strClipURI, string.Empty );

         HttpWebRequest request = WebRequest.CreateDefault( new Uri( strURL ) ) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization"  , string.Format("bearer {0}", m_strAccessToken) }
         };
         request.Method = @"PATCH";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         //Do call
         HttpStatusCode status = HttpStatusCode.OK;
         try
         {
            {
               IDictionary<string, string> KeyValues = new Dictionary<string, string>
               {
                  {"privacy.view", GetPrivacyString(ePrivacy)                    }
               };
               if (ePrivacy == Privacy.Password)
               {
                  KeyValues.Add(
                     new KeyValuePair<string, string>("password", strPassword));
               }

               string strQueryParams = GenerateQueryParams(KeyValues);

               VimeoLogger.Log( "Vimeo::SetPrivacyInformation; privacy.view: " + GetPrivacyString( ePrivacy ) );
               VimeoLogger.Log( "Vimeo::SetPrivacyInformation; password: " + strPassword );

               var bytesData = Encoding.UTF8.GetBytes( strQueryParams );
               request.ContentLength = bytesData.Length;
               Stream requestStream = request.GetRequestStream();
               requestStream.Write(bytesData, 0, bytesData.Length);
               requestStream.Close();
            }


            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log( response );
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log( "Vimeo::SetPrivacyInformation; response: " + strResponse );
            status = response.StatusCode;
            r.Close();
            responseStream.Close();
            response.Close();
         }
         catch (WebException wex)
         {
            VimeoLogger.Log( wex );
            return false;
         }

         return status == HttpStatusCode.OK;
      }

      private string GetPrivacyString(Privacy ePrivacy)
      {
         VimeoLogger.Log("Vimeo::GetPrivacyString");
         switch (ePrivacy)
         {
            default:
            case Privacy.Anybody:
               Debug.Assert(ePrivacy == Privacy.Anybody);
               return "anybody";
            case Privacy.Nobody:
               return "nobody";
            case Privacy.Contacts:
               return "contacts";
            case Privacy.Password:
               return "password";
         }
         return "anybody";
      }

      public bool SetVideoTags(string strClipURI, string strTags)
      {
         VimeoLogger.Log("Vimeo::SetVideoTags");
         Debug.Assert(!string.IsNullOrEmpty(m_strClipURI));
         if ( string.IsNullOrEmpty(m_strClipURI))
         {
            return false;
         }

         string strURL = BuildURL( string.Format(Endpoints.ClipTags, strClipURI, strTags), string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization"  , string.Format("bearer {0}", m_strAccessToken) }
         };
         request.Method = @"PUT";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         //Do call
         HttpStatusCode status = HttpStatusCode.OK;
         try
         {
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log(response);
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::SetVideoTags; response: " + strResponse);
            r.Close();
            status = response.StatusCode;
            responseStream.Close();
            response.Close();
         }
         catch (WebException wex)
         {
            VimeoLogger.Log(wex);
            return false;
         }

         return status == HttpStatusCode.OK;
      }

      public bool LoadAccessToken(string strAccessToken)
      {
         VimeoLogger.Log("Vimeo::LoadAccessToken");
         m_strAccessToken  = string.Empty;
         m_strUserName     = string.Empty;
         string strURL = BuildURL(Endpoints.Me, string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization", string.Format("bearer {0}", strAccessToken)}
         };
         request.Method = @"GET";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         //Do call
         try
         {
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;
            VimeoLogger.Log(response);
            Stream responseStream = response.GetResponseStream();
            StreamReader r = new StreamReader(responseStream);
            string strResponse = r.ReadToEnd();
            VimeoLogger.Log("Vimeo::LoadAccessToken; response: " + strResponse);
            r.Close();
            responseStream.Close();
            response.Close();

            UserResponse userResponse =
              Newtonsoft.Json.JsonConvert.DeserializeObject<UserResponse>(strResponse);

            if ( string.IsNullOrEmpty(userResponse.uri))
            {
               return false;
            }

            //Save off access_token for authenticated requests
            m_strAccessToken = strAccessToken;

            //Save off username too! :)
            m_strUserName = userResponse.name;
         }
         catch (WebException wex)
         {
            VimeoLogger.Log(wex);
            Debug.Assert(wex.Status == WebExceptionStatus.NameResolutionFailure);//This is the one I've seen
            return false;
         }

         return true;
      }

      private CategoriesResponse m_Categories;

      public int GetCategories()
      {
         VimeoLogger.Log("Vimeo::GetCategories");
         Debug.Assert(!string.IsNullOrEmpty(m_strAccessToken));//Did you call ObtainAccessToken?
         if ( string.IsNullOrEmpty(m_strAccessToken))
         {
            return -1;
         }

         m_Categories = null;

         string strURL = BuildURL(Endpoints.Categories, string.Empty);

         HttpWebRequest request = WebRequest.CreateDefault(new Uri(strURL)) as HttpWebRequest;
         request.Headers = new WebHeaderCollection()
         {
            {"Authorization"  , string.Format("bearer {0}", m_strAccessToken) }
         };
         request.Method = @"GET";
         request.Accept = @"application/vnd.vimeo.*+json; version=3.2";
         request.ContentType = @"application/x-www-form-urlencoded";
         request.KeepAlive = false;

         //Name      Type  Required Description
         //page      int   No       The page number to show.
         //per_page  int   No       Number of items to show on each page. Max 50.
         //Using defaults

         //Do call
         HttpWebResponse response = request.GetResponse() as HttpWebResponse;
         VimeoLogger.Log(response);
         Stream responseStream = response.GetResponseStream();
         StreamReader r = new StreamReader(responseStream);
         string strResponse = r.ReadToEnd();
         VimeoLogger.Log("Vimeo::GetCategories; response: " + strResponse);
         r.Close();
         responseStream.Close();
         response.Close();

         CategoriesResponse categories =
              Newtonsoft.Json.JsonConvert.DeserializeObject<CategoriesResponse>(strResponse);

         m_Categories = categories;

         return categories.data.Count;
      }

      public bool GetUserName(ref string strUserName)
      {
         VimeoLogger.Log("Vimeo::GetUserName");
         strUserName = m_strUserName;
         return !string.IsNullOrEmpty(strUserName);
      }

      public bool GetURL(ref string strURL)
      {
         VimeoLogger.Log("Vimeo::GetURL");
         if ( string.IsNullOrEmpty(m_strClipURI))
         {
            return false;
         }

         int nSlash = m_strClipURI.LastIndexOf('/');
         strURL = "http://vimeo.com/" + m_strClipURI.Substring(nSlash + 1);

         return !string.IsNullOrEmpty(strURL);
      }

      private string m_strErrorMessage = string.Empty;

      public bool GetErrorMessage(ref string strErrorMessage)
      {
         VimeoLogger.Log("Vimeo::GetErrorMessage");
         strErrorMessage = m_strErrorMessage;

         return !string.IsNullOrEmpty(strErrorMessage);
      }
   }

   public class VimeoLogger
   {
      public static string GetLogFullPath()
      {
         string strPath = System.IO.Path.GetTempPath();
         strPath += "VimeoLog.txt";
         return strPath;
      }
      public static void Log( string strMessage )
      {
         using (StreamWriter w = File.AppendText(GetLogFullPath()))
         {
            w.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + ": " + strMessage);
         }
      }

      public static void Log(Exception ex)
      {
         Log("---Exception---");
         Log("Type: " + ex.GetType());
         Log("Exception: " + ex.Message);
         Log("Stack Track: " + ex.StackTrace);
         Log("Exception Source: " + ex.Source);
         Log("Helplink : " + ex.HelpLink);
      }

      public static void Log(WebException wex)
      {
         Log("--WebException--");
         Log("Status : " + wex.Status);
         Log((Exception)wex);
      }

      public static void Log(HttpWebResponse response)
      {
         Log("--HttpWebResponse--");
         Log("Status code: " + response.StatusCode);
         Log("Content length: " + response.ContentLength);
         Log("Content type: " + response.ContentType);
         Log("Content type: " + response.Headers);
         Log("Content type: " + response.Method);
         Log("Content type: " + response.StatusDescription);
      }
   }
}
