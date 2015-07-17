using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   public static class Endpoints
   {
      public const string Authorize    = "/oauth/authorize";
      public const string AccessToken  = "/oauth/access_token";
      public const string UploadTicket = "/me/videos";
      public const string Me           = "/me";
      public const string Video        = "/videos/{0}";
      public const string Categories   = "/categories";
      public const string ClipCategories = "{0}/categories";
      public const string ClipTags     = "{0}/tags/{1}";
   }
}
