using Mahi.LuaEngine;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mahi
{
    internal class UnitTest
    {
        internal static void Run()
        {
            HtmLuaParser parser = new HtmLuaParser();
            var script = parser.ParseHtmlToLua(File.ReadAllText("test.htmlua"));

            Console.WriteLine(script);

            try
            {
                using (Lua lua = new Lua())
                {
                    BuiltInFunctions builtInFunctions = new BuiltInFunctions();

                    lua.RegisterFunction("go", builtInFunctions, typeof(BuiltInFunctions).GetMethod("go"));

                    lua.DoString(script);
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Running script success.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                script = "-- " + ex.ToString().Replace("\r\n", "--") + Environment.NewLine + script;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Script faild to run!");
                Console.ResetColor();
            }

            File.WriteAllText("_.lua", script);
        }
    }
}

