using Fardin;
using Mahi.Settings;
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
			return request.Cookies.Any(i => i.Name == (AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key"));
		}

		public void set(string name, bool keep = false)
		{
			DateTime expireDate = DateTime.Now.AddMinutes(AppConfig.Instance.Auth.Timeout ?? 60); // read from config
			response.Cookies.AddCookie(new HttpCookie(AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key", name,
				AppConfig.Instance.Auth.Path ?? "/", SameSiteMode.Strict, true, true, keep ? expireDate : null));
		}

		public string name => request.Cookies.FirstOrDefault(i => i.Name == (AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key"))?.Value;

        public void clear()
		{
			response.Cookies.RemoveCookie(AppConfig.Instance.Auth.Name ?? "Mahi_Auth_Key");
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
