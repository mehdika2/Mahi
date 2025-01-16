using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi.Core
{
    public class PageNotFoundException : Exception
    {
        public PageNotFoundException(string page) : base(page) { }
    }
}
