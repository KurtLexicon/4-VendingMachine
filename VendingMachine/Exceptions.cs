using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMClasses {
    public class InvalidDenominationException : Exception { };
    public class BalanceTooLowException : Exception { };
    public class ProductNotFoundException : Exception { };
    public class InvalidCurrencyCodeException : Exception { };
    public class MissingValueException : Exception { };
    public class InvalidPriceException : Exception { };
}
