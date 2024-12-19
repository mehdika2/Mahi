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
        private HttpResponse response;

        public BuiltInFunctions(HttpResponse response)
        {
            this.response = response;
        }

        public void go(object html)
        {
            _html += html?.ToString();
        }

		public void setStatus(int status, string text = null)
		{
			response.StatusCode = status;
            response.StatusText = text;
		}
	}
}
