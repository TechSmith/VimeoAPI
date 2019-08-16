using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VimeoHarness.ViewModel;

namespace VimeoHarness.Dialogs
{
   /// <summary>
   /// Interaction logic for LoginDlg.xaml
   /// </summary>
   public partial class LoginDlg : Window
   {
      public LoginDlg()
      {
         InitializeComponent();
      }

      public LoginDlgViewModel ViewModel => DataContext as LoginDlgViewModel;

      private void WebBrowser_Navigating( object sender, System.Windows.Navigation.NavigatingCancelEventArgs e )
      {
         string url = e.Uri.ToString();
         if ( VimeoAPI.Vimeo.StartsWithCallbackURL( url ) )
         {
            ViewModel.CallbackURL = url;
            DialogResult = true;
            Close();
         }
      }
   }
}
