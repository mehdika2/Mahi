﻿using System;
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
				var builtInFunctions = new BuiltInFunctions(response);

				lua.RegisterFunction("go", builtInFunctions, typeof(BuiltInFunctions).GetMethod("go"));
				lua.RegisterFunction("setStatus", builtInFunctions, typeof(BuiltInFunctions).GetMethod("setStatus"));

				lua["request"] = new
				{
					method = request.Method,
					url = request.Url,
					httpVersion = request.HttpVersion,
					headers = ConvertDictionaryToLuaTable(lua, request.Headers.ToDictionary(i => i.Name, i => i.Value)),
					isMultipartRequest = request.IsMultipartRequest,
					content = request.Content
				};

				lua["response"] = new ResponseContext(lua, response);

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
