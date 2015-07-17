using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   //Note this is used with Newtonsoft.Json.JsonConvert.DeserializeObject AKA reflection
   //So you can't just adjust the variable names without careful consideration
   public class UploadTicketResponse
   {
      public string uri;
      public string complete_uri;
      public string ticket_id;
      public UserResponse user;
      public string upload_link_secure;
   }
}
