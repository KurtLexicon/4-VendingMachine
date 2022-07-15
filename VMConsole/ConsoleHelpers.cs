using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace VMConsole {
    public struct MenuItem {
        public string Text { get; }
        public Action Action { get; }
        public bool ContinueLoop { get; }

        public MenuItem(string text, Action action, bool continueLoop) {
            Text = text;
            Action = action;
            ContinueLoop = continueLoop;
        }
        public override string ToString() => Text;
    }

    internal class ConsoleHelpers {
        public const int width = 60;
        private int? exitCode = null;

        public void SetExitCode(int exitCode) {
            this.exitCode = exitCode;
        }

        // =========================
        // Output helpers
        // =========================

        public void Header(string header) {
            header ??= "";
            Clear();
            Delimiter('=');
            WriteCentered(header);
            Delimiter('=');
            WriteLine();
        }

        public void Delimiter(char delimiterChar = '-') {
            WriteLine(new string(delimiterChar, width));
        }

        public void WriteCentered(string str) {
            WriteLine(Center(str, width));
        }

        public string Center(string str, int width) {
            str = str.Trim();
            int nPad = Math.Max(0, (width + str.Length) / 2);
            return str.PadLeft(nPad);
        }

        // =========================
        // Input helpers
        // =========================

        public void WaitKey(string? prompt = null) {
            WriteLine();
            if (!string.IsNullOrWhiteSpace(prompt)) {
                WriteLine(prompt);
            }
            WriteLine("Press any key to continue");
            Thread.Sleep(200);
            ReadKey();
        }

        public bool GetYesOrNo(string prompt) {
            char answer = GetCharInput("YN", prompt);
            return answer == 'Y';
        }

        public char GetCharInput(IEnumerable<char> allowed, string prompt) {
            string errMsg = "";
            while (true) {
                int startLine = CursorTop;
                Console.WriteLine(prompt);
                Write(String.Join("/", allowed) + "?");
                string strAnswer = Console.ReadLine() ?? "";
                char chrAnwer = strAnswer.Length > 0 ? strAnswer.ToUpper()[0] : ' ';
                if (allowed.Any(c => c == chrAnwer)) return chrAnwer;
                errMsg = "Unrecognized letter, please try again";
                ResetCursor(startLine, errMsg);
            }
        }

        public string ReadString(string arg) {
            Write("{0}: ", arg);
            string str = ReadLine() ?? "";
            if (str == null) {
                throw new Exception("No input to read");
            }
            return str.Trim();
        }

        public string ReadNotEmptyString(string arg) {
            while(true) {
                int startLine = CursorTop;
                string str = ReadString(arg);
                if(!string.IsNullOrEmpty(str)) return str;
                ResetCursor(startLine, "The value can not be empty, try again");
            }
        }


        public int ReadInt(string prompt, Func<int, bool> condition, string errorMsg) {
            while(true) {
                int value = ReadIntOrNull(prompt, false)!.Value;
                if (condition(value)) return value; ;
                WriteLine(errorMsg);
            }
        }

        public int ReadInt(string prompt) {
            return ReadIntOrNull(prompt, false)!.Value;
        }

        public int? ReadIntOrNull(string prompt, bool returnNullForEmpty = true) {
            while (true) {
                int startLine = CursorTop;
                Write($"{prompt}: ");
                string x = ReadLine() ?? "";
                if (x.Trim() == "" && returnNullForEmpty) return null;
                if (int.TryParse(x, out int value)) {
                    return CheckExit(value);
                }
                WriteLine();
                ResetCursor(startLine, "This was not a number, try again");
            }
        }

        public int CheckExit(int value) {
            if (value == exitCode) throw new ExitProgram();
            return value;
        }

        public int[] ReadIntList(string arg) {
            while (true) {
                string str = ReadString(arg);
                string[] arStr = str.Split(',');
                List<int> ret = new();
                List<string> invalidNumbers = new();
                foreach (string strInt in arStr) {
                    if (string.IsNullOrEmpty(strInt))
                        continue;
                    if (!int.TryParse(strInt.Trim(), out int value)) {
                        invalidNumbers.Add(strInt.Trim());
                    }
                    ret.Add(value);
                }
                if (invalidNumbers.Count > 0) {
                    string errMsg = string.Format("Invalid integers in the list, e.g. {0}, please try again", invalidNumbers[0]);
                    ResetCursor(CursorTop, errMsg);
                    continue;
                }
                return ret.ToArray();
            }
        }

        // =========================
        // Menu Helpers
        // =========================

        public bool MenuScreen(
            string header1,
            string header2,
            List<MenuItem> menuItems,
            string returnPrompt,
            string prompt,
            bool alwaysExit = false
        ) {
            while (true) {
                Header(header1);
                WriteLine(header2);
                WriteLine();

                bool doContinue = DelegateMenu(
                    menuItems,
                    returnPrompt,
                    prompt
                );
                WriteLine();
                if (alwaysExit) return doContinue;
                if (!doContinue) return false;
            }
        }

        public Dictionary<int, T> MenuDict<T>(IEnumerable<T> list) {
            Dictionary<int, T> dic = new();
            int i = 1;
            foreach (T item in list) {
                dic[i++] = item;
            }
            return dic;
        }

        public bool DelegateMenu(
            List<MenuItem> menuItems,
            string returnPrompt,
            string prompt
        ) {
            MenuItem item = SelectMenu<MenuItem>(
                menuItems,
                item => item.Text,
                prompt,
                new MenuItem(returnPrompt, Nothing, false));
            if (item.Text == "") return false;
            item.Action();
            return item.ContinueLoop;
        }

        private void Nothing() {
            return;
        }

        public T SelectMenu<T>(
            IEnumerable<T> items,
            Func<T, string> selector,
            string prompt,
            T defaultValue,
            bool returnDefaultForZero = true
        ) {
            string errMsg = "";
            while (true) {
                int startLine = CursorTop;
                Dictionary<int, T> menuDict = MenuDict(items);
                foreach (KeyValuePair<int, T> kv in menuDict) {
                    WriteLine($"{kv.Key} - {selector(kv.Value)}");
                }
                WriteLine();
                if (errMsg != "") WriteLine(errMsg);
                WriteLine(prompt);
                int key = ReadInt("?");
                if (key == 0 && returnDefaultForZero) return defaultValue;
                if (menuDict.ContainsKey(key)) {
                    WriteLine();
                    return menuDict[key];
                }
                errMsg = $"{key} is not a valid choice. Please try again";
                ResetCursor(startLine);
            }
        }


        // =========================
        // Misc
        // =========================

        void ResetCursor(int startLine, string str = "") {
            int endLine = Console.CursorTop;
            Console.CursorTop = startLine;
            while (CursorTop <= endLine) {
                WriteLine(new string(' ', WindowWidth));
            }
            Console.CursorTop = startLine;
            Console.CursorLeft = 0;
            if (str != "") WriteLine(str);
        }

        public void TheEnd() {
            // https://onlineasciitools.com/convert-text-to-ascii-art
            Clear();
            WriteLine();
            WriteLine();
            WriteCentered(" =========================================================== ");
            WriteCentered(" =========================================================== ");
            WriteCentered(" ===          :::::::::: ::::    ::: :::::::::           === ");
            WriteCentered(" ===          :+:        :+:+:   :+: :+:    :+:          === ");
            WriteCentered(" ===          +:+        :+:+:+  +:+ +:+    +:+          === ");
            WriteCentered(" ===          +#++:++#   +#+ +:+ +#+ +#+    +:+          === ");
            WriteCentered(" ===          +#+        +#+  +#+#+# +#+    +#+          === ");
            WriteCentered(" ===          #+#        #+#   #+#+# #+#    #+#          === ");
            WriteCentered(" ===          ########## ###    #### #########           === ");
            WriteCentered(" =========================================================== ");
            WriteCentered(" =========================================================== ");
            WriteLine();
            WriteLine();
        }
    }
}
