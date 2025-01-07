using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fardin;
using NLua;
using System.Data.SqlClient;
using System.Collections.Specialized;
using Mahi.Settings;
using System.IO;

namespace Mahi.Core
{
	public static class LuaInvoker
	{
		public static object Run(string script, MemoryStream stream, HttpRequest request, HttpResponse response)
		{
			using (var lua = new Lua())
			{
				var builtInFunctions = new BuiltInFunctions(lua, response);

				RegisterBuiltInFunctions(lua, request, response, builtInFunctions);

				string name = '_' + Guid.NewGuid().ToString().Substring(0, 4);

				object result = lua.DoString($"{name} = false ::start:: if {name} then return else {name} = true end " + script);
				
				stream.Write(Encoding.UTF8.GetBytes(builtInFunctions._html));

				return result;
			}
		}

		public static LuaTable ConvertArrayToLuaTable(Lua lua, object[] array)
		{
			var table = lua.DoString("return {}")[0] as LuaTable;

			for (int i = 0; i < array.Length; i++)
				table[i + 1] = array[i]; // Shift index to 1-based

			return table;
		}

		public static LuaTable ConvertDictionaryToLuaTable(Lua lua, Dictionary<string, string> dictionary)
		{
			var table = lua.DoString("return {}")[0] as LuaTable;

			if (dictionary == null)
				return table;

			foreach (var kvp in dictionary)
				table[kvp.Key.Replace('.', '_')] = kvp.Value;

			return table;
		}

		public static LuaTable ConvertNameValueCollectionToLuaTable(Lua lua, NameValueCollection collection)
		{
			var table = lua.DoString("return {}")[0] as LuaTable;

			if (collection == null)
				return table;

			foreach (var key in collection.AllKeys)
				table[key] = collection[key];

			return table;
		}

		static void RegisterBuiltInFunctions(Lua lua, HttpRequest request, HttpResponse response, BuiltInFunctions builtInFunctions)
		{
			lua.State.Encoding = Encoding.UTF8;

			// register import
			lua.RegisterFunction("import", builtInFunctions, typeof(BuiltInFunctions).GetMethod("import"));

			// register html helpers
			lua.RegisterFunction("go", builtInFunctions, typeof(BuiltInFunctions).GetMethod("go"));
			lua.RegisterFunction("safe", builtInFunctions, typeof(BuiltInFunctions).GetMethod("safe"));

			// register response helpers
			lua.RegisterFunction("log", builtInFunctions, typeof(BuiltInFunctions).GetMethod("log"));
			lua.RegisterFunction("setStatus", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setStatus"));
			lua.RegisterFunction("redirect", builtInFunctions, typeof(BuiltInFunctions).GetMethod("redirect"));
			lua.RegisterFunction("addHeader", builtInFunctions, typeof(BuiltInFunctions).GetMethod("addHeader"));
			lua.RegisterFunction("setCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setCookie"));
			lua.RegisterFunction("deleteCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("deleteCookie"));
			lua.RegisterFunction("create_mssql_connection", builtInFunctions, typeof(BuiltInFunctions).GetMethod("create_mssql_connection"));
			lua.RegisterFunction("isNullOrEmpty", builtInFunctions, typeof(BuiltInFunctions).GetMethod("isNullOrEmpty"));
			lua.RegisterFunction("getError", builtInFunctions, typeof(BuiltInFunctions).GetMethod("getError"));
			lua.RegisterFunction("clearError", builtInFunctions, typeof(BuiltInFunctions).GetMethod("clearError"));

			// contains key built in function
			lua.DoString("function containsKey(table, key) return table[key] ~= nil end");

			lua["request"] = new
			{
				method = request.Method,
				uri = request.Uri,
				httpVersion = request.HttpVersion,
				headers = ConvertDictionaryToLuaTable(lua, request.Headers.ToDictionary(i => i.Name, i => i.Value)),
				cookies = ConvertDictionaryToLuaTable(lua, request.Cookies.ToDictionary(i => i.Name, i => i.Value)),
				post = ConvertNameValueCollectionToLuaTable(lua, request.RequestParameters),
				get = ConvertNameValueCollectionToLuaTable(lua, request.UrlParameters),
				isMultipartRequest = request.IsMultipartRequest,
				content = request.Content,
				items = request.Items,
				userAddress = request.Items["R_IP_ADDRESS"],
				userPort = request.Items["R_IP_PORT"],
			};

			lua["response"] = new ResponseContext(lua, response);

			var config = AppConfig.Instance;

			lua["appconfig"] = new
			{
				connectionStrings = ConvertDictionaryToLuaTable(lua, config.ConnectionStrings),
				defaultPages = ConvertArrayToLuaTable(lua, config.DefaultPages),
				extentionRequired = config.ExtentionRequired,
				notExtentionInUrl = config.NotExtentionInUrl,
				errorPages = ConvertDictionaryToLuaTable(lua, config.ErrorPages)
			};
		}
	}
}
