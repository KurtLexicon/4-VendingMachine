using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMClasses;
using static System.Console;

namespace VMConsole {
    internal class MainConsole {
        const int exitCode = 7691550;
        readonly ConsoleHelpers ch = new();
        private VendingMachine? VM { get; set; } = null;

        internal bool Run() {
            bool doContinue = true;
            while (doContinue) {
                List<MenuItem> menu = VM == null ?
                new List<MenuItem> {
                    new MenuItem("Exit", DoExit, false),
                    new MenuItem("Create new custom Vending Machine", CreateMachineScreen, true),
                    new MenuItem("Create new default Vending Machine", CreateDefaultMachine, true),
                } :
                new List<MenuItem> {
                    new MenuItem("Exit", DoExit, false),
                    new MenuItem("Run Admin Console", RunAdminUI, true),
                    new MenuItem("Run Customer Console", RunUserUI, true),
                };

                doContinue = ch.MenuScreen(
                    "Main Menu",
                    "What do you want to do?",
                    menu,
                    "",
                    "Give number for your choice",
                    true
                );
            }
            return true;
        }

        private bool RunAdminUI() {
            new AdminConsole(VM!).Run();
            return true;
        }

        private bool RunUserUI() {
            ch.WaitKey($"To return to the Main Menu enter the secret code {exitCode} at any time");
            new CustomerConsole(VM!).Run(exitCode);
            return true;
        }

        private bool DoExit() {
            VM = null;
            return false;
        }

        private bool CreateDefaultMachine() {
            VM = new();
            ch.Delimiter();
            return true;
        }

        private bool CreateMachineScreen() {
            ch.Header("Create an new Vending Machine");

            // Select currency

            WriteLine("Select Currency:");
            WriteLine();
            CurrencyCode currencyCode = ch.SelectMenu<CurrencyCode>(
                Currency.Currencies.Keys,
                (p) => p.ToString(),
                "Write the number for the Currency of your choice)",
                0,
                false
            );
            WriteLine();

            // Configure denominations

            WriteLine("Enter comma separated list of denomination values");
            int[] denominations = ch.ReadIntList("Or leave blank for default");

            // Start the machine

            string denominationsText = denominations.Length > 0 ?
                $"denominations {string.Join(", ", denominations)}" :
                "default denominations";

            if (ch.GetYesOrNo($"Do you want to create a machine with currency {currencyCode} and {denominationsText}?")) {
                VM = new(currencyCode, denominations);
                ch.WaitKey("The machine has now been started");
            } else {
                ch.WaitKey("The machine has NOT been started");
            }
            return false;
        }
    }
}

