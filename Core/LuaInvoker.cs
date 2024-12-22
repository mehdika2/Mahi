using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fardin;
using NLua;
using Mahi.HtmLua;

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
				lua.RegisterFunction("setStatus", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setStatus"));
				lua.RegisterFunction("redirect", builtInFunctions, typeof(BuiltInFunctions).GetMethod("redirect"));
				lua.RegisterFunction("addHeader", builtInFunctions, typeof(BuiltInFunctions).GetMethod("addHeader"));
				lua.RegisterFunction("setCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setCookie"));
				lua.RegisterFunction("deleteCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("deleteCookie"));

				// contains key built in function
				lua.DoString("function containsKey(table, key) return table[key] ~= nil end");

				lua["request"] = new
				{
					method = request.Method,
					uri = request.Uri,
					httpVersion = request.HttpVersion,
					headers = ConvertDictionaryToLuaTable(lua, request.Headers.ToDictionary(i => i.Name, i => i.Value)),
					cookies = ConvertDictionaryToLuaTable(lua, request.Cookies.ToDictionary(i => i.Name, i => i.Value)),
					isMultipartRequest = request.IsMultipartRequest,
					content = request.Content,
					items = request.Items,
					userAddress = request.Items["R_IP_ADDRESS"],
					userPort = request.Items["R_IP_PORT"]
				};

				lua["response"] = new ResponseContext(lua, response);

				lua["json"] = new BuiltInJson(lua);

				lua.DoString(script);

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
	}
}
