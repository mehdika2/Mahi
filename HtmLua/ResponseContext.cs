using Fardin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace Mahi.HtmLua
{
	public class ResponseContext
	{
		private HttpResponse response;
		private Lua lua;

		public ResponseContext(Lua lua, HttpResponse response) 
		{
			this.lua = lua;
			this.response = response;
		}

        public int status { get { return response.StatusCode; } }
        public string statusText { get { return response.StatusText; } }
        public string httpVersion { get { return response.HttpVersion; } }
        public LuaTable headers { get { return Core.LuaInvoker.ConvertDictionaryToLuaTable(lua, response.Headers.ToDictionary(i => i.Name, i => i.Value)); } }
		public LuaTable cookies {  get { return Core.LuaInvoker.ConvertDictionaryToLuaTable(lua, response.Cookies.ToDictionary(i => i.Name, i => i.Value)); } }
	}
}
