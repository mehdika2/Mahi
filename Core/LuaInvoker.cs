using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fardin;
using NLua;
using Mahi.HtmLua;
using System.Data.SqlClient;
using System.Collections.Specialized;

namespace Mahi.Core
{
	public static class LuaInvoker
	{
		public static void Run(string script, MemoryStream stream, HttpRequest request, HttpResponse response)
		{
			using (Lua lua = new Lua())
			{
				var builtInFunctions = new BuiltInFunctions(lua, response);

				// register import
				lua.RegisterFunction("import", builtInFunctions, typeof(BuiltInFunctions).GetMethod("import"));

				// register html helpers
				lua.RegisterFunction("go", builtInFunctions, typeof(BuiltInFunctions).GetMethod("go"));

				// register response helpers
				lua.RegisterFunction("log", builtInFunctions, typeof(BuiltInFunctions).GetMethod("log"));
				lua.RegisterFunction("setStatus", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setStatus"));
				lua.RegisterFunction("redirect", builtInFunctions, typeof(BuiltInFunctions).GetMethod("redirect"));
				lua.RegisterFunction("addHeader", builtInFunctions, typeof(BuiltInFunctions).GetMethod("addHeader"));
				lua.RegisterFunction("setCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setCookie"));
				lua.RegisterFunction("deleteCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("deleteCookie"));
				lua.RegisterFunction("create_mssql_connection", builtInFunctions, typeof(BuiltInFunctions).GetMethod("create_mssql_connection"));
				lua.RegisterFunction("isNullOrEmpty", builtInFunctions, typeof(BuiltInFunctions).GetMethod("isNullOrEmpty"));

				// contains key built in function
				lua.DoString("function containsKey(table, key) return table[key] ~= nil end");

				lua["request"] = new
				{
					method = request.Method,
					uri = request.Uri,
					httpVersion = request.HttpVersion,
					headers = ConvertDictionaryToLuaTable(lua, request.Headers.ToDictionary(i => i.Name, i => i.Value)),
					cookies = ConvertDictionaryToLuaTable(lua, request.Cookies.ToDictionary(i => i.Name, i => i.Value)),
					post = ConvertDictionaryToLuaTable(lua, request.RequestParameters),
					get = ConvertDictionaryToLuaTable(lua, request.UrlParameters),
					isMultipartRequest = request.IsMultipartRequest,
					content = request.Content,
					items = request.Items,
					userAddress = request.Items["R_IP_ADDRESS"],
					userPort = request.Items["R_IP_PORT"],
				};

				lua["response"] = new ResponseContext(lua, response);

				string name = '_' + Guid.NewGuid().ToString().Substring(0, 4);

				lua.DoString($"{name} = false ::start:: if {name} then return else {name} = true end " + script);

				stream.Write(Encoding.UTF8.GetBytes(builtInFunctions._html));
			}
		}

		public static LuaTable ConvertArrayToLuaTable(Lua lua, int[] array)
		{
			var table = lua.DoString("return {}")[0] as LuaTable;

			for (int i = 0; i < array.Length; i++)
				table[i + 1] = array[i]; // Shift index to 1-based

			return table;
		}

		public static LuaTable ConvertDictionaryToLuaTable(Lua lua, Dictionary<string, string> dictionary)
		{
			var table = lua.DoString("return {}")[0] as LuaTable;

			foreach (var kvp in dictionary)
				table[kvp.Key] = kvp.Value;

			return table;
		}

		public static LuaTable ConvertDictionaryToLuaTable(Lua lua, NameValueCollection collection)
		{
			var table = lua.DoString("return {}")[0] as LuaTable;

			if (collection == null)
				return table;

			foreach (var key in collection.AllKeys)
				table[key] = collection[key];

			return table;
		}
	}
}
