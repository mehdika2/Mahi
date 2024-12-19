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
				var builtInFunctions = new BuiltInFunctions(lua, request, response);

				lua.RegisterFunction("go", builtInFunctions, typeof(BuiltInFunctions).GetMethod("go"));

				lua["headers"] = builtInFunctions.headers;
				//lua["Request"] = request;
				//lua["Response"] = response;

				lua.DoString(script);

				stream.Write(Encoding.UTF8.GetBytes(builtInFunctions._html));
			}
		}
	}
}
