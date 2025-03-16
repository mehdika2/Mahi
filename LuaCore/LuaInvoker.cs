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
using Mahi.Core;
using System.Reflection;

namespace Mahi.LuaCore
{
	public static class LuaInvoker
	{
		static List<BuiltInFunction> registredFunctions = new List<BuiltInFunction>();

		public static object Run(string script, HttpRequest request, HttpResponse response)
		{
			using (var lua = new Lua())
			{
				var builtInFunctions = new BuiltInFunctions(lua, request, response);

				string name = '_' + Guid.NewGuid().ToString().Substring(0, 4);

				RegisterBuiltInFunctions(lua, request, response, builtInFunctions);

				object result = lua.DoString($"{name} = false ::start:: if {name} then return else {name} = true end " + script);

				response.ResponseStream.Write(Encoding.UTF8.GetBytes(builtInFunctions._html));

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

		public static void RegisterBuitlInFunction(BuiltInFunction function)
		{
			registredFunctions.Add(function);
		}

		static void RegisterBuiltInFunctions(Lua lua, HttpRequest request, HttpResponse response, BuiltInFunctions builtInFunctions)
		{
			SessionAuthentication session = new SessionAuthentication(request, response);

			lua.State.Encoding = Encoding.UTF8;

			// register html helpers
			lua.RegisterFunction("go", builtInFunctions, typeof(BuiltInFunctions).GetMethod("go"));
			lua.RegisterFunction("safe", builtInFunctions, typeof(BuiltInFunctions).GetMethod("safe"));

			// temp & request data
			lua.RegisterFunction("setTemp", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setTemp"));
			lua.RegisterFunction("getTemp", builtInFunctions, typeof(BuiltInFunctions).GetMethod("getTemp"));
			lua.RegisterFunction("setItem", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setItem"));
			lua.RegisterFunction("getItem", builtInFunctions, typeof(BuiltInFunctions).GetMethod("getItem"));

			// register response helpers
			lua.RegisterFunction("log", builtInFunctions, typeof(BuiltInFunctions).GetMethod("log"));
			lua.RegisterFunction("setStatus", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setStatus"));
			lua.RegisterFunction("redirect", builtInFunctions, typeof(BuiltInFunctions).GetMethod("redirect"));
			lua.RegisterFunction("addHeader", builtInFunctions, typeof(BuiltInFunctions).GetMethod("addHeader"));
			lua.RegisterFunction("setCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setCookie"));
			lua.RegisterFunction("deleteCookie", builtInFunctions, typeof(BuiltInFunctions).GetMethod("deleteCookie"));
			lua.RegisterFunction("isNullOrEmpty", builtInFunctions, typeof(BuiltInFunctions).GetMethod("isNullOrEmpty"));
			lua.RegisterFunction("getError", builtInFunctions, typeof(BuiltInFunctions).GetMethod("getError"));
			lua.RegisterFunction("clearError", builtInFunctions, typeof(BuiltInFunctions).GetMethod("clearError"));
			lua.RegisterFunction("match", builtInFunctions, typeof(BuiltInFunctions).GetMethod("match"));
			lua.RegisterFunction("matches", builtInFunctions, typeof(BuiltInFunctions).GetMethod("matches"));

			// register encoding functions
			lua.RegisterFunction("base64_decode", builtInFunctions, typeof(BuiltInFunctions).GetMethod("base64_decode"));
			lua.RegisterFunction("base64_encode", builtInFunctions, typeof(BuiltInFunctions).GetMethod("base64_encode"));
			lua.RegisterFunction("utf_encode", builtInFunctions, typeof(BuiltInFunctions).GetMethod("utf_encode"));
			lua.RegisterFunction("utf_decode", builtInFunctions, typeof(BuiltInFunctions).GetMethod("utf_decode"));

			foreach (var function in registredFunctions)
				if (function.Target == null)
					lua.RegisterFunction(function.Name, function.MethodInfo);
				else lua.RegisterFunction(function.Name, function.Target, function.MethodInfo);

			// contains key built in function
			lua.DoString("function containsKey(table, key) return table[key] ~= nil end " +
				$"package.path = \"{Path.GetFullPath(AppConfig.Instance.BaseDirectory).Replace("\\", "\\\\")}\\\\.libraries\\\\?.lua\"");

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
				baseDirectory = config.BaseDirectory,
				directoryBrowsing = config.DirectoryBrowsing,
				defaultPages = ConvertArrayToLuaTable(lua, config.DefaultPages),
				extentionRequired = config.ExtentionRequired,
				notExtentionInUrl = config.NotExtentionInUrl,
				frobiddenPaths = ConvertArrayToLuaTable(lua, config.FrobiddenPaths),
				errorPages = ConvertDictionaryToLuaTable(lua, config.ErrorPages)
			};

			lua["session"] = session;
		}
	}

	public class BuiltInFunction
	{
		public BuiltInFunction(string name, MethodInfo methodInfo)
		{
			Name = name;
			MethodInfo = methodInfo;
		}

		public BuiltInFunction(string name, object target, MethodInfo methodInfo)
		{
			Name = name;
			Target = target;
			MethodInfo = methodInfo;
		}

		public string Name { get; set; }
		public object Target { get; set; }
		public MethodInfo MethodInfo { get; set; }
	}
}
