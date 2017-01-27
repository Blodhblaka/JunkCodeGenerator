using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunkCodeGenerator
{
    static class JunkCodenz
    {
        public static Random rnd = new Random();


        public static void RemoveComments(ref List<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("//") && !lines[i].Contains("//["))
                {
                    int startIndex = lines[i].IndexOf("//");
                    lines[i] = lines[i].Remove(startIndex);
                }
            }
        }

        public static void MoveUsing(ref List<string> lines)
        {
            List<string> usingLines = new List<string>();
            foreach (var line in lines)
            {
                if (line.Contains("using "))
                {
                    bool save = true;
                    for (int i = 0; i < usingLines.Count; i++)
                    {
                        if (line == usingLines[i])
                            save = false;
                    }
                    if (save)
                        usingLines.Add(line);
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Contains("using "))
                    lines[i] = "";

            }

            lines.InsertRange(0, usingLines);
        }
        public static void SwapLines(ref string[] lines)
        {
            int startIdx = 0, endIdx = 0;
            while (FindSwapLines(ref lines, ref startIdx, ref endIdx))
            {
                string[] swappedLines = new string[endIdx - startIdx];
                int l = 0;

                for (int i = startIdx; i < endIdx; i++)
                {
                    swappedLines[l] = lines[i];
                    l++;
                }
                swappedLines = swappedLines.OrderBy(line => rnd.Next()).ToArray();

                l = 0;
                for (int i = startIdx; i < endIdx; i++)
                {
                    lines[i] = swappedLines[l];
                    l++;
                }
            }
        }



        public static void GenFunc(ref string[] lines)
        {
            FindJunkCodeFunctions(ref lines);
        }

        public static void GenRandomStaticVars(ref string[] lines)
        {
            int _start = -1;
            List<string> listOfLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("[JunkcodeStaticVars]") && _start == -1)
                {
                    lines[i] = ""; //Add functions
                    _start = i;
                }
                if (_start != -1)
                {
                    string GenEntries = "";
                    for (int l = 0; l < rnd.Next(10, 20); l++)
                    {
                        GenEntries += "\n" + GenerateVarsOfRandomness(true);

                    }
                    lines[i] = GenEntries;
                    _start = -1;
                }
                listOfLines.Add(lines[i] + "\n");

            }
            lines = listOfLines.ToArray();
        }

        public static void GenRandomVars(ref string[] lines)
        {
            int _start = -1;
            List<string> listOfLines = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("[JunkcodeVars]") && _start == -1)
                {
                    lines[i] = ""; //Add functions
                    _start = i;
                }
                if (_start != -1)
                {
                    string GenEntries = "";
                    for (int l = 0; l < rnd.Next(10, 20); l++)
                    {
                        GenEntries += "\n" + GenerateVarsOfRandomness(false);

                    }
                    lines[i] = GenEntries;
                    _start = -1;
                }
                listOfLines.Add(lines[i] + "\n");

            }
            lines = listOfLines.ToArray();
        }

        static void FindJunkCodeFunctions(ref string[] lines)
        {
            int _start = -1;
            List<string> listOfLines = new List<string>();
            string[] nameSpaces = new string[100000];
            int generatedNamespaces = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("[JunkcodeFunc]") && _start == -1)
                {
                    lines[i] = ""; //Add functions
                    _start = i;
                }
                if (_start != -1)
                {
                    string GenEntries = "";
                    for (int l = 0; l < rnd.Next(10, 20); l++)
                    {
                        string[] randomClass = GenerateRandomClassOfRandomness();
                        nameSpaces[generatedNamespaces] = randomClass[0];
                        generatedNamespaces += 1;
                        GenEntries += "\n" + randomClass[1] + "\n";

                    }
                    lines[i] = GenEntries;
                    _start = -1;
                }
                listOfLines.Add(lines[i] + "\n");

            }
            for (int i = 0; i < generatedNamespaces; i++)
            {
                listOfLines.Add("\n" + nameSpaces[i] + "\n");
            }
            lines = listOfLines.ToArray();
        }

        static string[] GenerateRandomClassOfRandomness()
        {
            string classOfRandomness = "";
            string usage = "";
            string randNameSpace = RandomString(rnd.Next(5, 10));
            string randFuncName = RandomString(rnd.Next(5, 10));
            string randClassName = RandomString(rnd.Next(5, 10));
            string randVar1 = RandomString(rnd.Next(5, 10));
            string randVar2 = RandomString(rnd.Next(5, 10));

            if (rnd.Next(0, 10) > 5)
            {
                classOfRandomness = "\nnamespace " + randNameSpace +
                    "\n{\nclass " + randClassName +
                    "\n{\n[SwapLines]\n [JunkcodeVars]\n  volatile int " + randVar1 + " = 0; \n[SwapLines/] \n public " + randClassName + "( int " + randVar2 + "){ " + randFuncName + "(" + randVar2 + ");}public void " + randFuncName + "(int " + randVar2 + "){ " + randVar1 + "++;if (" + randVar1 + "< " + randVar2 + "){ " + randFuncName + "(" + randVar2 + ");}else " + randVar1 + " = 0;}}}";
                usage = "new " + randNameSpace + "." + randClassName + "(" + rnd.Next(1, 5) + ");";
            }
            else
            {
                classOfRandomness = "\nnamespace " + randNameSpace + "\n{\n    class " + randClassName + "\n    {        public volatile static int " + randVar1 + " = 1111;        int a = new Random().Next(0, 2);        int b = new Random().Next(0, 2);        int c = new Random().Next(0, 2);        public int " + randFuncName + "()        {            if(a+b+c > 0)            {                " + randVar1 + " += a + b + c;            }            return new Random().Next(0, " + randVar1 + ");        }    }}";
                usage = "new " + randNameSpace + "." + randClassName + "()." + randFuncName + "();";

            }
            return new string[] { classOfRandomness, usage };
        }

        static string GenerateVarsOfRandomness(bool Static)
        {
            string toReturn = "\n";
            if (Static)
                toReturn += "static ";
            int a = rnd.Next(1, 4);
            const string quote = "\"";

            switch (a)
            {
                case 1:
                    toReturn += "int " + RandomString(rnd.Next(3, 10)) + " = " + rnd.Next(-100, 10000);
                    break;
                case 2:
                    toReturn += "float " + RandomString(rnd.Next(3, 10)) + " = " + rnd.Next(-100, 10000);
                    break;
                case 3:
                    toReturn += "string " + RandomString(rnd.Next(3, 10)) + " = " + quote + RandomString(10) + quote;
                    break;
                default:
                    toReturn += "int " + RandomString(rnd.Next(3, 10)) + " = " + rnd.Next(-100, 10000);
                    break;
            }
            toReturn += ";\n";
            return toReturn;
        }

        static bool FindSwapLines(ref string[] lines, ref int startIdx, ref int endIdx)
        {
            int _start = -1;
            if (lines.Length <= 0)
                return false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] != null)
                {
                    if (lines[i].Contains("[SwapLines]") && _start == -1)
                    {
                        lines[i] = "";
                        _start = i;
                    }

                    if (lines[i].Contains("[SwapLines/]") && _start != -1)
                    {
                        lines[i] = "";
                        startIdx = _start;
                        endIdx = i;
                        return true;
                    }
                }
            }
            return false;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public static IEnumerable<string> SplitToLines(this string input)
        {
            if (input == null)
            {
                yield break;
            }

            using (System.IO.StringReader reader = new System.IO.StringReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
