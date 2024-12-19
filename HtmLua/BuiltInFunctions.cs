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

        public void go(object html)
        {
            _html += html.ToString();
        }
    }
}
