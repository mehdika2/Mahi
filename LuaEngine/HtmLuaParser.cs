using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mahi.LuaEngine
{
    internal class HtmLuaParser
    {
        int index;
        string template;
        string finalHtml;
        int startGo = -1;
        internal string ParseHtmlToLua(string template)
        {
            this.template = template;
#if DEBUG
            //          template = @"
            //  <h1>Generated Table</h1>
            //  <table border='1'>
            //      <tr>
            //          <th>Index</th>
            //          <th>Value</th>
            //      </tr>
            //      ${ 
            //	rows != {}
            //          for i = 1, 5 do
            //              table.insert(rows, {i, 'Value ' .. i})
            //          end
            //      }
            //      ${ for _, row in ipairs(rows) do }
            //    <tr>
            //		$. hello world comment .$
            //		<td style=""color: $(row[1] == 1 then go('red') else go('black'))"">
            //			${
            //				if row[1] == 1 then
            //					go('<span>Admin</span>')
            //				else go('<span>$row[1]<span>')
            //			}
            //		</td>
            //		<td>$row[2]</td>
            //	</tr>
            //$end
            //  </table>";
#endif

            finalHtml = string.Empty;
            for (index = 0; index < template.Length; index++)
            {
                if (current == '$' && ParseExpression())
                    continue;
                if (startGo == -1)
                    StartGo();
                finalHtml += template[index];
            }
            if (startGo != -1)
                EndGo();
            finalHtml += Environment.NewLine;
            return finalHtml;
        }

        void StartGo()
        {
            startGo = index;
            finalHtml += " go([[";
        }

        void EndGo()
        {
            startGo = -1;
            finalHtml += "]])";
        }

        bool ParseExpression()
        {
            if (startGo != -1)
                EndGo();
            if (isEnd) return false;
            else if (next == '{')
            {
                index += 2;
                ScriptExpression();
                return false;
            }
            // comment
            else if (next == '.')
            {
                int end = template.Substring(index).IndexOf(".$");
                if (end == -1)
                    index = template.Length;
                else index += end + 2;
                return true;
            }
            // raw $$ => html $
            else if (next == '$')
            {
                index++;
                return false;
            }
            else
            {
                index++;
                IdentifierExpression();
            }
            return false;
        }

        Stack<int> openScripts = new Stack<int>();
        int openedScope = 0;
        void ScriptExpression()
        {
            openScripts.Push(index);
            if (current == '{')
                openedScope++;
            if (current == '}' && openedScope == 0)
            {
                index++;
                openScripts.Pop();
                return;
            }
            if (current == '}')
                openedScope--;
            finalHtml += current;
            index++;
            ScriptExpression();
        }

        void IdentifierExpression()
        {
            string match = "";
            if (current == '(')
            {
                match += Regex.Match(template.Substring(index), @"\(([^()]+|\((?:[^()]+|\([^()]*\))*\))*\)").Value.Remove(0, 1);
                match = match.Remove(match.Length - 1, 1);
                index += 2;
            }
            else match += Regex.Match(template.Substring(index), "^[^ \n<]+").Value;
            finalHtml += match;
            index += match.Length;
        }

        char current
        {
            get
            {
                return template[index];
            }
        }

        char next
        {
            get
            {
                return template[index + 1];
            }
        }

        bool isEnd
        {
            get
            {
                return index + 1 >= template.Length;
            }
        }

#if DEBUG
        string after
        {
            get
            {
                return template.Substring(index);
            }
        }
#endif
    }
}
