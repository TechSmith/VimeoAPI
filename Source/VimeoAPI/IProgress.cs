using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   public interface IProgress
   {
      void SetProgress(double dPercent);
      System.Boolean GetCanceled();
   }
}
