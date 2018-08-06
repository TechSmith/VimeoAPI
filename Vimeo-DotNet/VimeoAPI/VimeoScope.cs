using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   [Flags]
   public enum VimeoScope
   {
      Public  = 1 << 0,//View public videos
      Private = 1 << 1,//View private videos
      Upload  = 1 << 2,//Upload a video
      Edit    = 1 << 3//Edit videos, groups, albums, etc.
   }

   public static class VimeoScopeExtensions
   {
      public static VimeoScope GetBasicUploadScope()//Not an extension :(
      {
         return VimeoScope.Public | VimeoScope.Private | VimeoScope.Upload | VimeoScope.Edit;
      }

      public static IEnumerable<VimeoScope> GetAllFlags() //Not an extension either :(
      {
         IEnumerable<VimeoScope> list = new List<VimeoScope>()
         {
            VimeoScope.Public,
            VimeoScope.Private,
            VimeoScope.Upload,
            VimeoScope.Edit
         };

         return list;
      }

      public static IEnumerable<VimeoScope> GetFlags(this VimeoScope scope)
      {
         foreach (VimeoScope value in GetAllFlags())
         {
            if (scope.HasFlag(value))
            {
               yield return value;
            }
         }
      }

      public static IEnumerable<string> GetFlagsString(this VimeoScope scope)
      {
         foreach (VimeoScope value in GetAllFlags())
         {
            if (scope.HasFlag(value))
            {
               yield return value.AsString();
            }
         }
      }

      public static string AsString(this VimeoScope scope)
      {
         switch (scope)
         {
            default:
            case VimeoScope.Public:
               Debug.Assert(scope == VimeoScope.Public);//Missing case?
               return "public";
            case VimeoScope.Private:
               return "private";
            case VimeoScope.Upload:
               return "upload";
            case VimeoScope.Edit:
               return "edit";
         }
      }

      public static string GetSpaceSeparatedValue(this VimeoScope scope)
      {
         return string.Join(" ", scope.GetFlagsString());
      }
   }
}
