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
      public class Upload
      {
         public string upload_link;
      }

      public string uri;
      public string link;
      public Upload upload;
   }
}
