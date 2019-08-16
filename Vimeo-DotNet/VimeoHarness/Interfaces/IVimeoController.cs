using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VimeoHarness.DataTypes;

namespace VimeoHarness.Interfaces
{
   public interface IVimeoController
   {
      void Login();

      event EventHandler<ObtainedAccessTokenArgs> ObtainedAccessToken;

      void Upload( string fileToUpload, string title, string description, string tags );
   }
}
