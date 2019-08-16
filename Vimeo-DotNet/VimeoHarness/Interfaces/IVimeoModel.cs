using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VimeoHarness.Interfaces
{
   public interface IVimeoModel
   {
      string AccessToken { get; set; }
      string LoggedOnAs { get; set; }
   }
}
