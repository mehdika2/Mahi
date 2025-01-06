using Fardin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;
using NLua.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Core
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

        public void safe(object html)
        {
            _html += System.Net.WebUtility.HtmlEncode(html?.ToString());
        }

        public void log(string text)
        {
            Program.Log(text);
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

            response.Cookies.AddCookie(new HttpCookie(name, value, expireDate, path, (SameSiteMode)Enum.Parse(typeof(SameSiteMode), samesite), secure, httpOnly));
        }

        public void deleteCookie(string name)
        {
            response.Cookies.RemoveCookie(name);
        }

        public SqlConnection create_mssql_connection(string connectionString)
        {
            return new SqlConnection(connectionString);
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
			return new Exception(ex.Message, ex.InnerException ?? default(Exception))
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
	}
}
