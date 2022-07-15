using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMClasses {
    public interface IVending {
        int[] AllowedCoins { get; }
        int Balance { get; }
        string BalanceText { get; }

        string Purchase(string productName);
        List<Product> ShowAll();
        void InsertMoney(int value);
        Dictionary<int, int> EndTransaction();

        int GetLowestPurchasePrice();
        string AmountString(int amount);
    }
}
