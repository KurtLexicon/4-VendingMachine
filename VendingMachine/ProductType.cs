using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMClasses {
    internal class Beverage : Product{
        public Beverage(string name, string description, int price ) : base(name, description, "drink it", price) {
        }
    }

    internal class Food : Product {
        public Food(string name, string description, int price) : base(name, description, "eat it", price) {
        }
    }

    internal class Car : Product {
        public Car(string name, string description, int price) : base(name, description, "drive it", price) {
        }
    }
}
