using Fardin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Mahi.HtmLua
{
    public class BuiltInFunctions
    {
        public string _html = string.Empty;
		private Lua lua;
		private HttpResponse response;

        public BuiltInFunctions(Lua lua, HttpResponse response)
        {
			this.lua = lua;
            this.response = response;
        }

        public void go(object html)
        {
            _html += html?.ToString();
        }

		public void setStatus(int status, string text = null)
		{
			response.StatusCode = status;
            response.StatusText = text;
		}

		public object import(string module)
		{
			try
			{
				// Use Lua's require function and return the result.
				return lua.DoString($"return require('modules.{module}')")[0];
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error importing module '{module}': {ex.Message}");
				return null;
			}
		}
	}

	public class BuiltInJson
	{
		private Lua lua;
		public BuiltInJson(Lua lua)
		{
			this.lua = lua;
		}

		public string ser(object obj) 
			=> serialize(obj);
		public string serialize(object obj)
		{
			return JsonConvert.SerializeObject(obj);
		}

		public object deser(string str)
			=> deserialize(str);
		public object deserialize(string str)
		{
			// Deserialize JSON to a dynamic object
			return JsonConvert.DeserializeObject(str);

			//// Convert the dynamic object to a Lua table
			//var table = lua.DoString("return {}")[0] as LuaTable;
			//foreach (var property in deserializedObject.Properties())
			//	table[property.Name] = property.Value.ToObject<object>();
			//return table;
		}
	}
}
