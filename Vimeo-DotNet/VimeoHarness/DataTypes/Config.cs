using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace VimeoHarness.DataTypes
{
   public class Config
   {
      private readonly string _configPath;
      public Config()
      {
         _configPath = string.Format( @"Software\TechSmith\Camtasia Studio\19.0");
      }
      public void Load()
      {
         RegistryKey key = Registry.CurrentUser.OpenSubKey( _configPath, true/*writable*/ );

         if ( key == null )
         {
            key = Registry.CurrentUser.CreateSubKey( _configPath );
         }

         AccessToken = (string)key.GetValue( "VimeoAccessToken", string.Empty );
      }

      public void Save()
      {
         RegistryKey key = Registry.CurrentUser.OpenSubKey( _configPath, true/*writable*/ );
         if ( key == null )
         {
            return;
         }

         key.SetValue( "VimeoAccessToken", AccessToken );
      }

      public string AccessToken;
   }
}
