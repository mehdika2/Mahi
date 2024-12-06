using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.LuaEngine
{
    public class BuiltInFunctions
    {
        public string _html = string.Empty;

        public void go(string html)
        {
            _html += html;
        }
    }
}
