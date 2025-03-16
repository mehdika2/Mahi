using Fardin;
using Mahi.Core;
using Microsoft.Data.Sqlite;
using NLua;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace Mahi.LuaCore
{
    public class BuiltInFunctions
    {
        public string _html = string.Empty;
        private Lua lua;
        private HttpRequest request;
        private HttpResponse response;

        public BuiltInFunctions(Lua lua, HttpRequest request, HttpResponse response)
        {
            this.lua = lua;
            this.request = request;
            this.response = response;
        }

        public void go(object html)
        {
            _html += html?.ToString();
        }

        public void safe(object html)
        {
            _html += System.Net.WebUtility.HtmlEncode(html?.ToString());
        }

        public void log(string text)
        {
            Program.Logger.Log(text);
        }

        public void setStatus(int status, string text = null)
        {
            response.StatusCode = status;
            response.StatusText = text;
        }

        public void redirect(string url)
        {
            response.StatusCode = 302;
            if (string.IsNullOrWhiteSpace(url))
                response.Headers.Add("Location", "/");
            else response.Headers.Add("Location", url);
        }

        public void addHeader(string name, string value)
        {
            response.Headers.Add(name, value);
        }

        public void setCookie(string name, string value, string expire = null, string path = "/", string samesite = "Lax", bool secure = true, bool httpOnly = false)
        {
            DateTime expireDate = DateTime.Now.AddDays(1);
            if (expire != null)
                expireDate = DateTime.Parse(expire);

            response.Cookies.AddCookie(new HttpCookie(name, value, path, (SameSiteMode)Enum.Parse(typeof(SameSiteMode), samesite), secure, httpOnly, expireDate));
        }

        public void deleteCookie(string name)
        {
            response.Cookies.RemoveCookie(name);
        }

        [Obsolete]
        public SqlConnection create_mssql_connection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }

        public SqliteConnection create_sqlite_connection(string connectionString)
        {
            return new SqliteConnection(connectionString);
        }

        public bool isNullOrEmpty(string input)
        {
            return string.IsNullOrWhiteSpace(input);
        }

        public Exception getError()
        {
            var ex = RequestHandler.LastError;
            if (ex == null)
                return null;
            return new Exception(ex.Message, ex.InnerException ?? default)
            {
                Source = ex.Source,
                HResult = ex.HResult,
                HelpLink = ex.HelpLink
            };
        }

        public void clearError()
        {
            RequestHandler.LastError = null;
        }

        static Dictionary<string, object> temp = new Dictionary<string, object>();
        public void setTemp(string name, object value)
        {
            temp[name] = value;
        }

        public object getTemp(string name)
        {
            if (temp.TryGetValue(name, out var value))
                return value;
            return null;
        }

        public void setItem(string name, object value)
        {
            request.Items[name] = value;
        }

        public object getItem(string name)
        {
            if (request.Items.TryGetValue(name, out var value))
                return value;
            return null;
        }

        public byte[] base64_decode(string input)
        {
            return Convert.FromBase64String(input);
        }

        public string base64_encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        public byte[] utf_encode(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public string utf_decode(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public LuaTable match(string input, string pattern)
        {
            var table = lua.DoString("return {}")[0] as LuaTable;
            var result = Regex.Match(input, pattern);
            table["success"] = result.Success;
            table["index"] = result.Index;
            table["length"] = result.Length;
            return table;
        }

        public LuaTable matches(string input, string pattern)
        {
            var table = lua.DoString("return {}")[0] as LuaTable;
            int index = 1;
            foreach (Match match in Regex.Matches(input, pattern))
            {
                var mtable = lua.DoString("return {}")[0] as LuaTable;
                mtable["success"] = match.Success;
                mtable["index"] = match.Index;
                mtable["length"] = match.Length;
                table[index++] = mtable;
            }
            return table;
        }
    }
}
