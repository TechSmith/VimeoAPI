using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VimeoAPI.JSONObjects
{
   public class Upload
   {
      public string appraoch;
      public long size;
   }
   public class Privacy
   {
      public string view;
      //public string embed;
      //public bool download;
   }

   public class UploadRequest
   {
      public Upload upload;
      public string name;
      //public string description;
      public Privacy privacy;
   }
}
