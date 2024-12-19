using Fardin;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.HtmLua
{
    public class BuiltInFunctions
    {
        public string _html = string.Empty;

        private HttpRequest request;
        private HttpResponse response;
        private Lua lua;

        public BuiltInFunctions(Lua lua, HttpRequest request, HttpResponse response)
        {
            this.lua = lua;
            this.request = request;
            this.response = response;
        }

        public void go(object html)
        {
            _html += html.ToString();
        }

        public LuaTable headers
        {
            get
            {
                return ConvertDictionaryToLuaTable("headers", request.Headers.ToDictionary(i => i.Name, i => i.Value));
            }
        }

		private LuaTable ConvertArrayToLuaTable(string tableName, int[] array)
		{
            var table = lua.GetTable(tableName);
            if (table != null) return table;

			lua.NewTable(tableName);
			table = lua[tableName] as LuaTable;

			for (int i = 0; i < array.Length; i++)
				table[i + 1] = array[i]; // Shift index to 1-based

			return table;
		}

		private LuaTable ConvertDictionaryToLuaTable(string tableName, Dictionary<string, string> dictionary)
		{
			var table = lua.GetTable(tableName);
			if (table != null) return table;

			lua.NewTable(tableName);
			table = lua[tableName] as LuaTable;

			foreach (var kvp in dictionary)
				table[kvp.Key] = kvp.Value;

			return table;
		}
	}
}
