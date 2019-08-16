using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using VimeoHarness.DataTypes;
using VimeoHarness.Dialogs;
using VimeoHarness.Interfaces;
using VimeoHarness.ViewModel;

namespace VimeoHarness.Controllers
{
   public class VimeoController : IVimeoController
   {
      private readonly VimeoAPI.Vimeo _Vimeo;
      private readonly Config _Config;
      public VimeoController()
      {
         _Vimeo = new VimeoAPI.Vimeo( "5f53d7b66f4f13e0d96ccbf39d699fb98571b6a9",
            "q8NnGuIUCqexanswFpa4f8eyluDo2a/HSKoQlDf8BRbFUL7Rb+zRwpoDSVKR73Vhw4X7XFMNfT+N4sw/OSZ5glrElCYuKhbtqvTXoFbOLO/q8zisMT7XhOo+tyjZ/KQr" );

         _Config = new Config();
         _Config.Load();
      }

      public void LoadAccessToken()
      {
         string accessToken = _Config.AccessToken;

         if ( !string.IsNullOrEmpty( accessToken ))
         {
            bool bOK = _Vimeo.LoadAccessToken( accessToken );

            if( bOK )
            {
               LoadedAccessToken();
            }
            else
            {
               Registry.Users.SetValue( "VimeoAccessToken", string.Empty );
            }
         }
      }

      public event EventHandler<ObtainedAccessTokenArgs> ObtainedAccessToken;

      public void Login()
      {
         string strURL = string.Empty;
         _Vimeo.GetUserAuthorizationURL( ref strURL );

         LoginDlg dlg = new LoginDlg()
         {
            DataContext = new LoginDlgViewModel()
            {
               WebAddress = strURL
            }
         };

         var result = dlg.ShowDialog();
         if ( result.HasValue && result.Value == true )
         {
            _Vimeo.ObtainAccessToken( dlg.ViewModel.CallbackURL );

            LoadedAccessToken();
         }
      }

      private void LoadedAccessToken()
      {
         string accesstoken = string.Empty;
         _Vimeo.GetAccessToken( ref accesstoken );

         string username = string.Empty;
         _Vimeo.GetUserName( ref username );

         _Config.AccessToken = accesstoken;
         _Config.Save();

         ObtainedAccessToken.Invoke( this, new ObtainedAccessTokenArgs()
         {
            AccessToken = accesstoken,
            Username = username
         } );
      }

      public void Upload( string fileToUpload, string title, string description, string tags )
      {
         string captionData = @"WEBVTT

00:01.000 --> 00:03.000
Now is the winter of our discontent.

00:04.000 --> 00:06.000
— Really?
— Really.

00:08.000 --> 00:09.000
Bummer.";

         bool bOK = _Vimeo.Upload( fileToUpload, title, description, tags, VimeoAPI.Vimeo.Privacy.Anybody, "", captionData, null );
      }
   }
}
