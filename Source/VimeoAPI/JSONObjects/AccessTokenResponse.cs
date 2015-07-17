using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   //Note this is used with Newtonsoft.Json.JsonConvert.DeserializeObject AKA reflection
   //So you can't just adjust the variable names without careful consideration
   public class AccessTokenResponse
   {
      public string access_token;
      public string token_type;
      public string scope;//space separated list of scopes
      public UserResponse user;
   }
}
