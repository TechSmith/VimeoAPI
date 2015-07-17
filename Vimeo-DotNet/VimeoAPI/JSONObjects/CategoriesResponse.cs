using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VimeoAPI
{
   //Note this is used with Newtonsoft.Json.JsonConvert.DeserializeObject AKA reflection
   //So you can't just adjust the variable names without careful consideration
   public class CategoriesResponse
   {
      public int total;
      public int page;
      public int per_page;

      public List<Category> data;
   }

   public class Category
   {
      public string uri;
      public string name;
      public string link;
      public bool top_level;
      public Pictures pictures;
      public Metadata metadata;
      public List<SubCategory> subcategories;
   }

   public class Pictures
   {
      public string uri;
      public bool active;
      public List<Sizes> sizes;
   }

   public class Sizes
   {
      public int width;
      public int height;
      public string link;
   }

   public class Metadata
   {
      public Connections connections;
   }

   public class Connections
   {
      public Connection channels;
      public Connection groups;
      public Connection users;
      public Connection videos;
   }

   public class Connection
   {
      public string uri;
      public List<string> options;
      public int total;
   }

   public class SubCategory
   {
      public string uri;
      public string name;
      public string link;
   }

}
