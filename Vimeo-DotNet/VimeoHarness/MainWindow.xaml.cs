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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VimeoHarness.Controllers;
using VimeoHarness.DataTypes;
using VimeoHarness.Interfaces;
using VimeoHarness.ViewModel;

namespace VimeoHarness
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();

         var controller = new VimeoController();

         controller.ObtainedAccessToken += Controller_ObtainedAccessToken;

         DataContext = new MainWindowViewModel( controller )
         {
            FileToUpload = "C:\\Green.avi",
            Title ="Test",
            Description="Some description",
            Tags="testing"
         };

         controller.LoadAccessToken();
      }

      public IVimeoModel ViewModel => DataContext as IVimeoModel;

      private void Controller_ObtainedAccessToken( object sender, ObtainedAccessTokenArgs e )
      {
         ViewModel.AccessToken = e.AccessToken;
         ViewModel.LoggedOnAs = e.Username;
      }
   }
}
