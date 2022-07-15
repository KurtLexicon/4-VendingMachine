using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMClasses;
using static System.Console;

namespace VMConsole {


    internal class CustomerConsole {
        readonly static ConsoleHelpers ch = new();

        private IVending VM { get; }

        public CustomerConsole(IVending VM) {
            this.VM = VM;
        }

        public void Run(int exitCode) {
            ch.SetExitCode(exitCode);
            while (true) {
                try {
                    ShowPurchaseScreen();
                }
                catch (ExitProgram) {
                    return;
                }
            }
        }

        private void ShowPurchaseScreen() {
            while (true) {
                ch.Header($"Your balance is now {VM.BalanceText}");

                ProductDictionary pd = new(VM);
                WriteLine("Choose from menu:");
                WriteLine();
                WriteMenuList(pd);
                WriteLine();

                int menuItem = ch.ReadInt("Enter your choice");
                switch (menuItem) {
                    case 0:
                        ch.Delimiter('=');
                        ch.WriteCentered("");
                        ch.Delimiter('=');
                        EnterMoneySection(1);
                        return;
                    case 99:
                        ShowEndTransactionScreen();
                        return;
                    default:
                        DoPurchase(menuItem, pd);
                        break;
                }
            }
        }

        private void EnterMoneySection(int headerLine) {
            string startMessage = "";
            while (true) {
                int startLine = Console.CursorTop;
                int coinSize = 0;
                while (coinSize == 0) {
                    if (!string.IsNullOrWhiteSpace(startMessage)) {
                        WriteLine(startMessage);
                        WriteLine();
                    }
                    WriteLine($"Your balance is now {VM.AmountString(VM.Balance)}");
                    Console.CursorTop = startLine + 3;
                    ch.Delimiter();
                    WriteLine($"Enter coin size, or just press ENTER for stop adding coins");
                    int? nullableCoinSize = ch.ReadIntOrNull("");
                    if (nullableCoinSize == null) {
                        if (VM.Balance == 0)
                            ShowEndTransactionScreen();
                        return;
                    }
                    coinSize = nullableCoinSize.Value;
                    if (!VM.AllowedCoins.Contains(coinSize)) {
                        coinSize = 0;
                        startMessage = "Oops that was not a valid coin, please try again";
                        startMessage += "\n";
                        startMessage += $"Valid coins are {string.Join(",", VM.AllowedCoins)}";
                        ClearConsolFrom(startLine);
                    }
                }

                VM.InsertMoney(coinSize);
                WriteBalanceHeader(headerLine);
                startMessage = $"You added {VM.AmountString(coinSize)}";
                ClearConsolFrom(startLine);
            }
        }
        
        void WriteBalanceHeader(int lineNo) {
            int saveLine = CursorTop;
            int saveCol = CursorLeft;
            CursorTop = lineNo;
            CursorLeft = 0;
            WriteLine(new string(' ', WindowWidth));
            CursorTop = lineNo;
            CursorLeft = 0;
            ch.WriteCentered($"Your balance is now {VM.BalanceText}");
            CursorTop = saveLine;
            CursorLeft = saveCol;
        }

        private static void ClearConsolFrom(int startLine) {
            int endLine = Console.CursorTop;
            Console.CursorTop = startLine;
            while (CursorTop <= endLine) {
                WriteLine(new string(' ', WindowWidth));
            }
            Console.CursorTop = startLine;
            Console.CursorLeft = 0;
        }

        private void DoPurchase(int menuKey, ProductDictionary productDictionary) {
            if (productDictionary.TryGetValue(menuKey, out Product? p)) {
                string usage;
                try {
                    usage = VM.Purchase(p.Name);
                }
                catch (BalanceTooLowException) {
                    ch.Delimiter('=');
                    ch.WriteCentered("Your Balance is too low for the chosen product");
                    ch.WriteCentered("Please enter more money to buy this product");
                    ch.Delimiter('=');
                    EnterMoneySection(1);
                    return;
                }

                Clear();
                ch.Delimiter('=');
                ch.WriteCentered($"Congratulations to your purchase of a {p.Name}");
                ch.WriteCentered($"You may now {usage}!");
                ch.WriteCentered($"Your remaining balance is {VM.BalanceText}");
                ch.Delimiter('=');
                ch.WaitKey("");
            } else {
                ch.Delimiter();
                ch.WriteCentered("Oops that was not a valid choice.");
                ch.Delimiter();
                ch.WaitKey("");
            }
        }

        private void ShowEndTransactionScreen() {
            ch.Header("Thanks for your visit!");

            int balanceBefore = VM.Balance;
            Dictionary<int, int> dicReturn = VM.EndTransaction();
            int sumReturn = dicReturn.Select(kv => kv.Key * kv.Value).Sum();
            if (sumReturn > 0) {
                WriteLine($"Please don't forget your change of {VM.AmountString(sumReturn)}");
                WriteLine();
                foreach (KeyValuePair<int, int> entry in dicReturn) {
                    int coinSize = entry.Key;
                    int n = entry.Value;
                    WriteLine($"{n} coins of value {VM.AmountString(coinSize)}");
                }
            }

            int remaining = balanceBefore - sumReturn;
            if (remaining > 0) {
                // Could happen if lowest coin size is not 1
                WriteLine();
                WriteLine($"The remaining value of {VM.AmountString(remaining)} is too small");
                WriteLine($"to be returned and will be domated to charity");
            }

            WriteLine();
            ch.Delimiter();
            ch.WriteCentered("Press any key to return to Start Screen");
            ch.Delimiter();
            ReadKey();
        }

        class ProductDictionary : Dictionary<int, Product> {
            readonly private IVending VM;

            public ProductDictionary(IVending VM) {
                this.VM = VM;
                int menuNumber = 1;
                foreach (Product p in VM.ShowAll()) {
                    this[menuNumber++] = p;
                }
            }

            public override string ToString() {
                List<string> lines = new();
                foreach (int key in this.Keys) {
                    Product p = this[key];
                    string str = $"{key} - {p.Name}, {VM.AmountString(p.Price)}";
                    if (p.Price > VM.Balance) str = RedString(str);
                    lines.Add(str);
                }
                return string.Join("\n", lines);
            }


            static string RedString(string str) {
                return $"[red]{str}";
            }
        }

        static void WriteMenuList(ProductDictionary productDictionary) {
            WriteLine("0 - Add more money");
            WriteLinesColored($"{productDictionary}");
            WriteLine("99 - End purchase - Surplus money will be returned");
        }

        static void WriteLinesColored(string str) {
            List<string> lines = str.Split('\n').ToList();
            lines.ForEach(s => {
                if(s.StartsWith("[red]")) {
                    ForegroundColor = ConsoleColor.Red;
                    WriteLine(s[5..]);
                    Console.ResetColor();
                } else {
                    WriteLine(s);
                }
            });
        }
    }
}
