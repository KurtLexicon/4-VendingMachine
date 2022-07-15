using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMClasses {
    public enum CurrencyCode {
        SEK,
        USD
    }

    enum SymbolPosition {
        Before,
        After
    }

    public class Currency {
        static public Dictionary<CurrencyCode, Currency> Currencies { get; } = new() {
            { CurrencyCode.SEK, new Currency("kr", SymbolPosition.After) },
            { CurrencyCode.USD, new Currency("$", SymbolPosition.Before) },
        };

        private string Symbol { get; }
        private SymbolPosition Position { get; }

        private Currency(string symbol, SymbolPosition position) {
            this.Symbol = symbol;
            this.Position = position;
        }

        public string GetString(int value) {
            return Position switch {
                SymbolPosition.Before => $"{Symbol} {value}",
                SymbolPosition.After => $"{value} {Symbol}",
                _ => throw new NotImplementedException()
            };
        }

        static public Currency Get(CurrencyCode code) {
            if(!Currencies.TryGetValue(code, out Currency? currency)) {
                throw new InvalidCurrencyCodeException();
            }
            return currency;
        }
    }
}
