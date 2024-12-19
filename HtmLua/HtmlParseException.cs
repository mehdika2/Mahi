using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.HtmLua
{
	public class HtmlParseException : Exception
	{
		public HtmlParseException() : base("An error occurred while parsing HTML.") { }

		public HtmlParseException(string message) : base(message) { }

        public HtmlParseException(string message, Exception innerException)
			: base(message, innerException) { }
	}
}
