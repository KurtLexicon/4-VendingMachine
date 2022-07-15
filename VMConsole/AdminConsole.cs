using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMClasses;
using static System.Console;

namespace VMConsole {
    internal class AdminConsole {
        readonly ConsoleHelpers ch = new();

        private VendingMachine VM { get; set; }

        public AdminConsole(VendingMachine vm) {
            VM = vm;
        }

        public void Run() {
            while (ConfigureVM()) { }
        }

        private bool ReturnFalse() {
            return false;
        }

        private bool ConfigureVM() {
            return ch.MenuScreen(
                "Configure Products",
                "What do you want to do?",
                new List<MenuItem> {
                    new MenuItem("Return to main menu", ReturnFalse, false ),
                    new MenuItem("List all products", ListAllProductsScreen, true ),
                    new MenuItem("List custom products", ListCustomProductsScreen, true ),
                    new MenuItem("Add a new custom product", AddProductScreen, true ),
                    new MenuItem("Remove an existing custom product", RemoveProductScreen, true ),
                    new MenuItem("Modify an existing custom product", ChangeProductScreen, true ),
                },
                "Return to previous menu",
                "Give number for your choice"
            );
        }

        private bool ListAllProductsScreen() {
            ch.Header("All products");
            List<Product> products = VM.ShowAll();

            products.ForEach(p => WriteLine(p.Examine()));
            ch.WaitKey();
            return true;
        }

        private bool ListCustomProductsScreen() {
            ch.Header("Custom products");
            List<Product> products = VM.GetCustomProducts();
            if (products.Count == 0) {
                ch.WaitKey("There do not exist any custom products");
                return true;
            }

            products.ForEach(p => WriteLine(p.Examine()));
            ch.WaitKey();
            return true;
        }

        private bool AddProductScreen() {
            while (true) {
                ch.Header("Create a new product and add to the machine");

                WriteLine("Leave name blank (Just press ENTER) for exit");

                string name = ch.ReadString("Name");
                if (string.IsNullOrWhiteSpace(name)) return true;

                string description = ch.ReadNotEmptyString("Description");
                string usage = ch.ReadNotEmptyString("Usage");
                int price = ch.ReadInt("Price", i => i > 0, "Price must be larger than 0, please try again");

                ch.Delimiter();
                WriteLine($"The product {name} is ready to be created");
                WriteLine($"Description: {description}");
                WriteLine($"Usage: {usage}");
                WriteLine($"Price: {price}");
                ch.Delimiter();
                WriteLine();

                bool wantCreate = ch.GetYesOrNo("Do you want to create this product?");
                if (!wantCreate) continue;

                bool success = VM!.TryAddCustomProduct(name, description, usage, price);
                ch.Delimiter();

                if (success) {
                    ch.WaitKey($"Product {name} has been added");
                } else {
                    ch.WaitKey($"Product {name} could not be added, since there already exists a product with this name");
                }
            }
        }

        private bool RemoveProductScreen() {
            ch.Header("Remove an existing custom product from the machine");

            List<Product> products = VM.GetCustomProducts();
            if (products.Count == 0) {
                ch.WaitKey("There do not exist any custom products");
                return true;
            }

            Product? productToRemove = ch.SelectMenu(
                products,
                (p) => $"{p!.Name}({p.Description})",
                "Write the number for the product you want to remove (or 0 for exit screen)",
                null
            );

            WriteLine();
            if (productToRemove != null) {
                if (ch.GetYesOrNo($"Do you want to remove product \"{productToRemove.Name}?\"")) {
                    VM.TryRemoveCustomProduct(productToRemove.Name);
                }
            }
            return true;
        }

        private bool ChangeProductScreen() {
            ch.Header("Modify an existing custom product in the machine");

            List<Product> products = VM.GetCustomProducts();
            if (products.Count == 0) {
                ch.WaitKey("There do not exist any custom products");
                return true;
            }

            Product? productToChange = ch.SelectMenu(
                products,
                (p) => $"{p!.Name} - {p.Description} - {p.Usage} - {p.Price}",
                "Write the number for the product you want to modify (or 0 for exit screen)",
                null
            );

            WriteLine();
            if (productToChange != null) {
                DoChangeProduct(productToChange);
            }
            return true;
        }

        private void DoChangeProduct(Product p) {
            ch.Header($"Modify existing custom product {p.Name}");

            WriteLine($"Set new values for the properties (Or leave value blank to leave the property unchanged)");
            WriteLine();

            string description = ch.ReadString($"Description ({p.Description})");
            string usage = ch.ReadString($"Usage ({p.Description})");
            int? price = ch.ReadIntOrNull($"Price ({p.Price})");

            bool success = VM!.TryChangeCustomProduct(p.Name, description, usage, price);

            ch.Delimiter();

            if (success) {
                ch.WaitKey("Product {name} has successfully been changed");
            } else {
                ch.WaitKey("Product {name} could not be changed");
            }
        }
    }
}
