using System;
using System.Collections.Generic;
using System.Linq;

namespace Mahi
{
    public class Header
    {
        public string Name { get; set; }
        public List<string> Values { get; set; }

        #region Dependent Properties
        public string Value
        {
            get
            {
                return string.Join("; ", Values.ToArray());
            }
        }
        #endregion

        public Header(string name, string value)
        {
            if (name.Contains(" ") || name.Contains(" "))
                throw new FormatException("Can't use spaces in header name.");

            Name = name;
            Values = new List<string>();
            Values.Add(value);
        }

        public Header(string name, string[] values)
        {
            if (name.Contains(" ") || name.Contains(" "))
                throw new FormatException("Can't use spaces in header name.");

            Name = name;
            Values = values.ToList();
        }

        public static Header Parse(string header)
        {
            string name = string.Empty;
            List<string> values = new List<string>();
            var headerLineParts = header.Split(new[] { ": " }, 2, StringSplitOptions.None);

            if (name.Contains(" ") || name.Contains(" "))
                throw new FormatException("Can't use spaces in header name.");

            if (headerLineParts.Length >= 2)
            {
                name = headerLineParts[0];
                values = headerLineParts[1].Split(new[] { "; ", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            if (values.Count == 1)
                return new Header(name, values[0]);
            return new Header(name, values.ToArray());
        }

        public void AddValue(string value)
        {
            Values.Add(value);
        }

        public override string ToString()
        {
            return Name + ": " + Value;
        }
    }
}
