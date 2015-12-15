using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   //Note this is used with Newtonsoft.Json.JsonConvert.DeserializeObject AKA reflection
   //So you can't just adjust the variable names without careful consideration
   public class UserResponse
   {
      public string uri;
      public string name;
      //public string link;
      //public string location;
      //public string bio;
      //public string created_time;
      //public string account;
      //public Pictures pictures;
      //public List<string> websites;
      //public PeferencesResponse preferences;
      //public List<string> content_filter;
      //public UploadQuotaResponse upload_quota;
   }

   public class MetadataResponse
   {
      public ConnectionsResponse connections;
   }

   public class ConnectionsResponse
   {
      public ConnectionResponse activities;
      public ConnectionResponse albums;
      public ConnectionResponse channels;
      public ConnectionResponse feed;
      public ConnectionResponse followers;
      public ConnectionResponse following;
      public ConnectionResponse groups;
      public ConnectionResponse likes;
      public ConnectionResponse portfolios;
      public ConnectionResponse videos;
      public ConnectionResponse watchlater;
      public ConnectionResponse shared;
      public ConnectionResponse pictures;
   }

   public class ConnectionResponse
   {
      public string uri;
      public List<string> options;
      public int total;
   }

   public class PeferencesResponse
   {
      public VideosResponse videos;
   }

   public class VideosResponse
   {
      public string privacy;
   }

   public class UploadQuotaResponse
   {
      public SpaceResponse space;
      public QuotaResponse quota;
   }

   public class SpaceResponse
   {
      public long free;
      public long max;
      public long used;
   }

   public class QuotaResponse
   {
      public bool hd;
      public bool sd;
   }
}
