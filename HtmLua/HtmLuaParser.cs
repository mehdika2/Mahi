using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Mahi.HtmLua
{
	public class HtmLuaParser
	{
		int position = 0;
		bool gopen = false;
		string template = string.Empty;
		string script = string.Empty;

		public string ToLua(string htmlua)
		{
			template = htmlua;

			// parsing section
			for (; position < template.Length; position++)
				Parse();

			if (gopen)
				EndGo();

			return script;
		}

		void Parse()
		{
			switch (current)
			{
				case '$':
					// script section
					if (next == '{') ParseSection();

					// identifier section
					else if (next == '(') ParseIdentifierSection();

					// comment section
					else if (next == '.') ParseComment();

					// signel $
					else if (next == '$')
					{
						position++;
						script += current;
					}

					// identifier 
					else ParseIdentifier();

					break;

				case '<':
					ParseHtml();
					break;

				default:
					if (!gopen)
						StartGo();

					script += current;
					break;
			}
		}

		void ParseSection()
		{
			if (gopen)
				EndGo();

			position += 2;
			int openBrackets = 0;
			bool stringOpen = false;
			while (!isAtEnd())
				switch (current)
				{
					case '<':
						if (stringOpen)
						{
							script += current;
							position++;
							break;
						}
						else if (char.IsLetter(next))
						{
							ParseHtml();
							break;
						}
						position++;
						StartGo();
						break;
					case '}':
						if (stringOpen || openBrackets > 0)
						{
							if (!stringOpen)
								openBrackets--;
							script += current;
							position++;
							break;
						}
						position++;
						StartGo();
						return;
					case '\'':
					case '"':
						stringOpen = !stringOpen;
						script += current;
						position++;
						break;
					case '{':
						if (!stringOpen)
							openBrackets++;
						script += current;
						position++;
						break;
					default:
						script += current;
						position++;
						break;
				}
		}

		void ParseHtml()
		{
			if (!gopen)
				StartGo();

			if (!char.IsLetter(next))
			{
				script += current;
				return;
			}

			Stack<string> openTags = new Stack<string>();
			while (!isAtEnd())
				switch (current)
				{
					case '<':
						script += current;

						// close tag
						if (next == '/')
						{
							script += next;
							position += 2;

							// loop for manage self-closing tags
							string closedTag = GetTagName();
							while (openTags.Count > 0)
							{
								string lastTag = openTags.Pop();
								if (lastTag != closedTag)
								{
									if (SelfClosingTags.List.Contains(lastTag))
										continue;
									throw new HtmlParseException($"Except closing <{lastTag}> after defining it but got </{closedTag}>" + GetErrorLine(position, template));
								}
								else break;
							}

							if (openTags.Count == 0)
							{
								script += current;
								position++;
								EndGo();
								return;
							}
							continue;
						}

						// open tag
						position++;
						string tag = GetTagName();
						if (!SelfClosingTags.List.Contains(tag))
							openTags.Push(tag);
						else if (openTags.Count == 0)
						{
							script += current;
							position++;
							EndGo();
							return;
						}
						break;
					case '$':
						Parse();
						break;
					default:
						script += current;
						position++;
						break;
				}
		}

		void ParseComment()
		{
			int index = after.IndexOf(".$");
			if (index == -1)
				throw new HtmlParseException("Except \".$\" to end comment section" + GetErrorLine(position, template));

			// remove whole line
			for (int i = script.Length - 1; i >= 2; i--)
				if (script.Substring(i - 1, 2) == Environment.NewLine)
				{
					script = script.Substring(0, i);
					break;
				}
				else if (script[i] != ' ' && script[i] != '\n')
				{
					script = script.Substring(0, i + 1);
					break;
				}

			position += index + 2;
		}

		void ParseIdentifierSection()
		{
			if (gopen)
				EndGo();

			position += 2;
			int openParentheses = 0;
			while (openParentheses >= 0)
			{
				switch (current)
				{
					case '(':
						openParentheses++;
						break;
					case ')':
						openParentheses--;
						if (openParentheses < 0)
						{
							position++;
							StartGo();
							return;
						}
						break;
				}
				script += current;
				position++;
			}
		}

		void ParseIdentifier()
		{
			if (gopen)
				EndGo();

			position++;
			bool stringOpen = false;
			int openParentheses = 0;
			while (!isAtEnd())
			{
				switch (current)
				{
					case '(':
						openParentheses++;
						break;
					case ')':
						if (stringOpen)
							break;
						openParentheses--;
						if (openParentheses == 0)
						{
							script += current;

							//! May need to execute
							//position++;

							//! May need check if not AtEnd here
							StartGo();
							return;
						}
						break;
					case '\'':
					case '"':
						stringOpen = !stringOpen;
						break;
					case '\r':
					case '<':
					case ' ':
						if (!stringOpen && openParentheses == 0)
						{
							//! May need check if not AtEnd here
							StartGo();
							return;
						}
						break;
				}
				script += current;
				position++;
			}

			////! May need check if not AtEnd here too!
			//StartGo();
		}

		void StartGo()
		{
			if (script.Length > 0 && char.IsLetterOrDigit(script[script.Length - 1]))
				script += ' ';
			script += "go([[";
			gopen = true;
		}

		void EndGo()
		{
			script += "]])";
			gopen = false;
		}

		string GetTagName()
		{
			string tagName = null;
			bool endTagName = false;
			for (int i = position; i < template.Length; i++)
				switch (current)
				{
					case '>':
						if (!gopen)
							StartGo();
						return tagName;
					case '$':
						Parse();
						break;
					case '\n':
					case '\r':
					case ' ':
						if (!endTagName)
							endTagName = true;
						script += current;
						position++;
						break;
					default:
						if (!endTagName)
							tagName += current;
						script += current;
						position++;
						break;
				}

			throw new HtmlParseException("Except closing tag section after opening it by \"<\" using \">\"" + GetErrorLine(position, template));
		}

		string GetErrorLine(int index, string code)
		{
			string[] lines = code.Substring(0, index).Split("\r\n");
			int column = lines[lines.Length - 1].Length;
			return Environment.NewLine + $" at line {lines.Length} : {column}";
		}

		char current { get { return template[position]; } }
		char next { get { return template[position + 1]; } }
		string after { get { return template.Substring(position); } }
		bool isAtEnd() { return position >= template.Length; }
	}
}
