using Fardin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YamlDotNet.Core.Tokens;

namespace Mahi.LuaCore
{
	public class SessionAuthentication
	{
		private HttpRequest request;
		private HttpResponse response;

		public SessionAuthentication(HttpRequest request, HttpResponse response)
		{
			this.request = request;
			this.response = response;
		}

        public bool isAuth()
		{
			return false;
		}

		public void set(string name, bool keep)
		{
			//DateTime expireDate = DateTime.Now.AddMinutes(1); // read from config
			//response.Cookies.AddCookie(new HttpCookie(name, value, expireDate, path, (SameSiteMode)Enum.Parse(typeof(SameSiteMode), samesite), secure, httpOnly));
		}

		public void clear()
		{
			//response.Cookies.RemoveCookie(name);
		}
		public string group()
		{
			return "";
		}

		public bool isInGroup(string name)
		{
			return false;
		}
	}
}
