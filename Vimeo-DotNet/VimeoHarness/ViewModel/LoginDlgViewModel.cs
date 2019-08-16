using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VimeoHarness.ViewModel
{
   public class LoginDlgViewModel : WPFCommonViewModel.ViewModel.ViewModelBase
   {
      private string _webAddress;
      public string WebAddress
      {
         get => _webAddress;
         set => SetProperty( ref _webAddress, value );
      }

      private string _callbackURL;
      public string CallbackURL
      {
         get => _callbackURL;
         set => SetProperty( ref _callbackURL, value );
      }
   }
}
