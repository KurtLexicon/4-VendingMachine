using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMClasses {

    public abstract class Product {
        public string Name { get; }
        public string Description { get; protected set; }
        public string Usage { get; protected set; }
        public int Price { get; protected set; }
        public virtual bool Changeable { get; } = false;

        protected Product(string name, string description, string usage, int price) {
            if (string.IsNullOrWhiteSpace(name)) throw new MissingValueException();
            if (string.IsNullOrWhiteSpace(description)) throw new MissingValueException();
            if (string.IsNullOrWhiteSpace(usage)) throw new MissingValueException();
            if (price <= 0) throw new InvalidPriceException();
            Name = name;
            Description = description;
            Usage = usage;
            Price = price;
        }

        virtual protected void Change(int? price) {
            if (price <= 0) throw new InvalidPriceException();
            if (price != null) Price = price.Value;
        }

        virtual public bool Change(string? description, string? usage, int? price) {
            return false;
        }

        public override string ToString() {
            return $"{Name} ({Description}), Price {Price}";
        }

        public string Examine() {
            string[] infoItems = {
                $"Name: {Name}",
                $"Description: {Description}",
                $"Usage: {Usage}",
                $"Price: {Price}",
            };
            return string.Join(" - ", infoItems);
        }

        virtual public string Use() {
            return Usage;
        }
    }

    public abstract class LexiconProduct : Product {
        public LexiconProduct(string name, string description, string usage, int price) : base(name, description, usage, price) { }
    }

    public class Banana : LexiconProduct {
        public Banana(int price) : base(
            name: "Banana",
            description : "A yellow fruit",
            usage: "feed a monkey",
            price: price
         ) { }
    }

    public class Tomato : LexiconProduct {
        public Tomato(int price) : base(
            name: "Tomato",
            description: "A red something",
            usage: "throw it at some annoying person",
            price: price
         ) { }
    }

    public class Cucumber : LexiconProduct {
        public Cucumber(int price) : base(
            name: "Cucumber",
            description: "A green vegetable",
            usage: "store it in some dark place",
            price: price
         ) { }
    }

    public class CustomProduct : Product {
        public override bool Changeable { get; } = true;

        public CustomProduct(string name, string description, string usage, int price) : base(
            name:name,
            description: description,
            usage: usage,
            price: price
        ){}

        override public bool Change(string? description, string? usage, int? price) {
            base.Change(price);
            if (!string.IsNullOrWhiteSpace(description)) Description = description;
            if (!string.IsNullOrWhiteSpace(usage)) Usage = usage;
            return true;
        }
    }
}
