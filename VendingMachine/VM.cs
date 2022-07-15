using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMClasses {
    public class VendingMachine : IVending {
        private const CurrencyCode defaultCurrencyCode = CurrencyCode.SEK;
        private static readonly int[] defaultDenominations = { 1, 2, 5, 10, 20, 50, 100, 500, 1000 };
        private readonly List<Product> defaultProducts = new() {
                new Tomato(5),
                new Banana(2),
                new Cucumber(19)};

        private Currency Currency { get; }
        private int[] Denominations { get; }
        private Dictionary<string, Product> Products { get; }


        public VendingMachine() : this(defaultCurrencyCode, Array.Empty<int>()) { }

        public VendingMachine(CurrencyCode currencyCode, int[] denominations) {
            Products = defaultProducts.ToDictionary(p => p.Name);
            Currency = Currency.Get(currencyCode);
            Denominations = denominations.Length switch {
                0 => defaultDenominations,
                _ => denominations
            };
        }

        public int[] AllowedCoins { get { return Denominations[..]; } }
        public int Balance { get; private set; } = 0;
        public string BalanceText { get { return AmountString(Balance); } }

        public int GetLowestPurchasePrice() {
            return Products
                    .Select(kv => kv.Value.Price)
                    .Min();
        }

        public string Purchase(string productName) {
            if (!Products.ContainsKey(productName)) throw new ProductNotFoundException();

            Product product = Products[productName];
            if (product.Price > Balance) {
                throw new BalanceTooLowException();
            }

            Balance -= product.Price;
            return product.Usage;
        }

        public List<Product> ShowAll() {
            return Products.Values.ToList();
        }

        public void InsertMoney(int value) {
            if (!Denominations.Contains(value)) {
                throw new InvalidDenominationException();
            }
            Balance += value;
        }

        public Dictionary<int, int> EndTransaction() {
            List<int> orderedDenominations = GetDenominationsOrdered();
            Dictionary<int, int> returns = new();
            foreach (int size in orderedDenominations) {
                while (size <= Balance) {
                    ReturnCoin(size);
                }
            }
            if (Balance > 0) {
                // Could happen if lowest denomination is not 1
                DonateToCharity(Balance);
            }
            return returns;

            void ReturnCoin(int size) {
                if (!returns.ContainsKey(size)) {
                    returns[size] = 0;
                }
                returns[size] += 1;
                Balance -= size;
            }
        }

        private void DonateToCharity(int value) {
            Balance -= value;
            // And here we could insert some code to
            // actually send the money somewhere. Unfortunately
            // this has not yet been implemented, it'a all a
            // question of prioritizations right, so for the moment
            // all the money meant for charity (temporarily) goes to
            // ... well..
        }

        public string AmountString(int amount) {
            return Currency.GetString(amount);
        }

        private List<int> GetDenominationsOrdered() {
            List<int> orderedDenominations = new(Denominations);
            orderedDenominations.Sort();
            orderedDenominations.Reverse();
            return orderedDenominations;
        }

        // ============================
        // Product Administration
        // ============================

        public List<Product> GetAllProducts() {
            return Products.Values.ToList();
        }

        public List<Product> GetCustomProducts() {
            return Products
                .Values
                .Where(p => p.Changeable)
                .ToList();
        }

        public bool TryAddCustomProduct(string name, string description, string usage, int price) {
            if (Products.ContainsKey(name)) return false;
            Products[name] = new CustomProduct(name, description, usage, price);
            return true;
        }

        public bool TryChangeCustomProduct(string name, string? description, string? usage, int? price) {
            if (!Products.TryGetValue(name, out Product? p)) return false;
            return p.Change(description, usage, price);
        }

        public bool TryRemoveCustomProduct(string name) {
            if (!Products.TryGetValue(name, out Product? p)) return false;
            if (!p.Changeable) return false;
            return Products.Remove(name);
        }
    }
}
