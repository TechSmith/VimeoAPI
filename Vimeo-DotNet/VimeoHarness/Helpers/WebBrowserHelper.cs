using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VimeoHarness.Helpers
{
   public static class WebBrowserHelper
   {
      public static readonly DependencyProperty BindableSourceProperty =
        DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(WebBrowserHelper), new UIPropertyMetadata(null, BindableSourcePropertyChanged, CoerceBrowserRefresh));

      public static string GetBindableSource( DependencyObject obj )
      {
         return (string)obj.GetValue( BindableSourceProperty );
      }

      public static void SetBindableSource( DependencyObject obj, string value )
      {
         obj.SetValue( BindableSourceProperty, value );
      }

      public static void BindableSourcePropertyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
      {
         WebBrowser browser = o as WebBrowser;
         if ( browser != null )
         {
            string uri = e.NewValue as string;
            browser.Source = !string.IsNullOrEmpty( uri ) ? new Uri( uri ) : null;
         }
      }

      public static object CoerceBrowserRefresh( DependencyObject d, object value )
      {
         WebBrowser browser = d as WebBrowser;
         if ( browser != null && browser.Source != null )
         {
            browser.Refresh();
         }

         return value;
      }

      public static readonly DependencyProperty NavigateToStringProperty = DependencyProperty.RegisterAttached(
        "NavigateToString",
        typeof( string ),
        typeof( WebBrowserHelper ),
        new FrameworkPropertyMetadata( OnNavigateToStringChanged ) );

      [AttachedPropertyBrowsableForType( typeof( WebBrowser ) )]
      public static string GetNavigateToString( WebBrowser d )
      {
         return (string)d.GetValue( NavigateToStringProperty );
      }

      public static void SetNavigateToString( WebBrowser d, string value )
      {
         d.SetValue( NavigateToStringProperty, value );
      }

      private static void OnNavigateToStringChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
      {
         var navigateTo = e.NewValue as string;
         if ( navigateTo != null )
         {
            var wb = d as WebBrowser;
            wb?.NavigateToString( navigateTo );
         }
      }
   }
}
