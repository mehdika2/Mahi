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

namespace Mahi
{
	internal class UnitTest
	{
		string template;
		internal void htmlTemplate()
		{
			template = @"
    <h1>Generated Table</h1>
    <table border='1'>
        <tr>
            <th>Index</th>
            <th>Value</th>
        </tr>
        ${ 
			rows = {}
            for i = 1, 5 do
                table.insert(rows, {i, 'Value ' .. i})
            end
        }
        ${ for _, row in ipairs(rows) do }
		    <tr>
				$. hello world comment .$
				<td style=""color: $(row[1] == 1 then print('red') else print('black'))"">
					${
						if row[1] == 1 then
							<span>Admin</span>
						else <span>$row[1]<span>
					}
				</td>
				<td>$row[2]</td>
			</tr>
		$end
    </table>";

			string generatedHtml = GenerateHtml();
			Console.WriteLine(generatedHtml);
		}

		int index;
		string finalHtml;
		string sub { get { return finalHtml.Substring(index, finalHtml.Length - index); } }
		string GenerateHtml()
		{
			finalHtml = string.Empty;
			for (index = 0; index < template.Length; index++)
			{
				if (current == '$' && ParseExpression())
					continue;
				finalHtml += template[index];
			}
			return finalHtml;
		}

		bool ParseExpression()
		{
			if (isEnd) return false;
			else if (next == '{')
			{
				//index += 2;
				GoExpression();
				return false;
			}
			// comment
			else if (next == '.')
			{
				//// remove line
				//int from = finalHtml.Length - 1;
				//for (; from >= 0; from--)
				//{
				//	char s = finalHtml[from];
				//	if (finalHtml[from] != '\r' && finalHtml[from] != '\n' && finalHtml[from] != '\t' && finalHtml[from] != ' ')
				//	{
				//		from++;
				//		break;
				//	}
				//}
				//finalHtml = finalHtml.Remove(from, index - from);
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

		void GoExpression()
		{
			MatchCollection matches = Regex.Matches(template.Substring(index), @"\}[^$<]*[$<]?", RegexOptions.Singleline);
			if (matches.Count > 0)
			{
				//> finding end of ${
				int end = matches[matches.Count - 1].Index;
				Console.WriteLine(matches[matches.Count - 1].Value);
				index += matches[matches.Count - 1].Length;
			}
			else throw new InvalidExpressionException("Except '}' after '${' in I:" + index);
		}

		void IdentifierExpression()
		{
			string match = "";
			if (current == '(')
			{
				match += Regex.Match(template.Substring(index), @"\(([^()]+|\((?:[^()]+|\([^()]*\))*\))*\)").Value.Remove(0,1);
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

		string after
		{
			get
			{
				return template.Substring(index);
			}
		}
	}
}
