using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VimeoHarness.Interfaces;
using WPFCommonViewModel.Command;

namespace VimeoHarness.ViewModel
{
   public class MainWindowViewModel : WPFCommonViewModel.ViewModel.ViewModelBase,
                                      IVimeoModel
   {
      private readonly IVimeoController _vimeoController;
      public MainWindowViewModel( IVimeoController vimeoController )
      {
         _vimeoController = vimeoController;
      }

      private string _loggedOnAs;
      public string LoggedOnAs
      {
         get => _loggedOnAs;
         set => SetProperty( ref _loggedOnAs, value );
      }

      private ICommand _loginCommand;
      public ICommand LoginCommand => _loginCommand ?? ( _loginCommand = new RelayCommand( () => _vimeoController.Login(), () => true ) );

      private string _accessToken;
      public string AccessToken
      {
         get => _accessToken;
         set => SetProperty( ref _accessToken, value );
      }

      private string _fileToUpload;
      public string FileToUpload
      {
         get => _fileToUpload;
         set => SetProperty( ref _fileToUpload, value );
      }

      private string _title;
      public string Title
      {
         get => _title;
         set => SetProperty( ref _title, value );
      }

      private string _description;
      public string Description
      {
         get => _description;
         set => SetProperty( ref _description, value );
      }

      private string _tags;
      public string Tags
      {
         get => _tags;
         set => SetProperty( ref _tags, value );
      }

      private ICommand _uploadCommand;
      public ICommand UploadCommand => _uploadCommand ?? ( _uploadCommand = new RelayCommand( () => _vimeoController.Upload( FileToUpload, Title, Description, Tags ), () => true ) );
   }
}
