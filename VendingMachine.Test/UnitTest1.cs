using VMClasses;
namespace VendingMachineTest {
    public class ProductTest {
        [Fact]
        public void TestCreateBanana() {
            // Arrange
            string expectedName = "Banana";
            string expectedDescription = "A yellow fruit";
            string expectedUsage = "feed a monkey";
            int price = 12;
            string expectedExamine =
                $"Name: {expectedName} - Description: {expectedDescription} - Usage: {expectedUsage} - Price: {price}";

            // Act
            Product product = new Banana(price);

            // Assert
            Assert.Equal(expectedUsage, product.Use());
            Assert.Equal(expectedExamine, product.Examine());
        }

        [Fact]
        public void TestCreateCustomProduct() {
            // Arrange
            string name = "Volvo";
            string description = "Tractor";
            string usage = "plough a field";
            int price = 123;
            string expectedExamine =
                $"Name: {name} - Description: {description} - Usage: {usage} - Price: {price}";

            // Act
            Product product = new CustomProduct(name, description, usage, price);

            // Assert
            Assert.Equal( name, product.Name);
            Assert.Equal(description, product.Description);
            Assert.Equal(usage, product.Usage);
            Assert.Equal(price, product.Price);
            Assert.Equal(usage, product.Use());
            Assert.Equal(expectedExamine, product.Examine());
        }

        [Fact]
        public void TestCreateVM() {
            // Arrange
            CurrencyCode currencyCode = CurrencyCode.SEK;
            int[] denominations = Array.Empty<int>();
            int[] expectedDenominations = { 1, 2, 5, 10, 20, 50, 100, 500, 1000 };
            int expectedBalance = 0;

            // Act
            VendingMachine vm = new (currencyCode, denominations);

            // Assert
            Assert.Equal(expectedBalance, vm.Balance);
            Assert.Equal(expectedDenominations, vm.AllowedCoins);
        }

        [Fact]
        public void TestAddCustomProductToVMExistingName() {
            // Arrange
            VendingMachine vm = new();
            string name = "Banana";
            string description = "xyz";
            string usage = "abc";
            int price = 1;
            bool expectedResult = false;

            // Act
            bool result = vm.TryAddCustomProduct(name, description, usage, price);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void TestAddCustomProductChangePurchaseRemove() {
            // Arrange
            string name = "Volvo";
            string description = "xyz";
            string usage = "abc";
            int price = 1;
            bool expectedResultCreateProduct = true;
            string newUsage = "plough";
            string expectedResultPurchase = newUsage;

            VendingMachine vm = new();
            vm.InsertMoney(1);

            string expectedExamineAfterChange =
                $"Name: {name} - Description: {description} - Usage: {newUsage} - Price: {price}";

            // Act - Create Product
            bool resultCreateProduct = vm.TryAddCustomProduct(name, description, usage, price);

            // Assert
            Assert.Equal(expectedResultCreateProduct, resultCreateProduct);

            // Act - Change Product
            bool resultChangeProduct = vm.TryChangeCustomProduct(name, null, newUsage, null);
            Product? productAfterChange = vm.GetAllProducts().Find(p => p.Name == name);

            // Assert
            Assert.True(resultChangeProduct);
            Assert.Equal(expectedExamineAfterChange, productAfterChange!.Examine());

            // Act - Purchase
            string resultPurchase = vm.Purchase(name);

            // Assert
            Assert.Equal(expectedResultCreateProduct, resultCreateProduct);
            Assert.Equal(expectedResultPurchase, resultPurchase);

            // Act - Remove Product
            bool resultRemove = vm.TryRemoveCustomProduct("Volvo");

            // Assert
            Assert.True(resultRemove);

            // Act - PurchaseRemoveProduct
            // Act
            void act() => vm.Purchase(name);

            // Assert
            Assert.Throws<ProductNotFoundException>(act);
        }


        [Fact]
        public void TestRemoveLexiconProduct() {
            // Arrange
            Banana banana = new(9999);
            VendingMachine vm = new();
            string name = banana.Name;
            string expectedResultPurchase = banana.Usage;
            vm.InsertMoney(1000);

            // Act
            bool resultRemove = vm.TryRemoveCustomProduct(name);
            string resultPurchase = vm.Purchase(name);

            // Assert
            Assert.False(resultRemove);

            Assert.Equal(expectedResultPurchase, resultPurchase);
        }

        [Fact]
        public void TestChangeLexiconProduct() {
            // Arrange
            Banana banana = new(9999);
            VendingMachine vm = new();
            string name = banana.Name;
            string expectedResultPurchase = banana.Usage;
            vm.InsertMoney(1000);
            string newUsage = "paint it blue";

            // Act
            bool resultChange = vm.TryChangeCustomProduct(name, null, newUsage, null);
            string resultPurchase = vm.Purchase(name);

            // Assert
            Assert.False(resultChange);
            Assert.Equal(expectedResultPurchase, resultPurchase);
        }


        [Fact]
        public void TestInsertValidCoins() {
            // Arrange
            VendingMachine vm = new (CurrencyCode.SEK, Array.Empty<int>());
            int expectedBalance = 10;

            // Act
            vm.InsertMoney(1);
            vm.InsertMoney(2);
            vm.InsertMoney(2);
            vm.InsertMoney(5);
            int resultBalance = vm.Balance;

            // Assert
            Assert.Equal(expectedBalance, resultBalance);
        }

        [Fact]
        public void TestInsertInvalidCoins() {
            // Arrange

            VendingMachine vm = new (CurrencyCode.SEK, Array.Empty<int>());
            int invalidCoin = 4;

            // Act
            void act() => vm.InsertMoney(invalidCoin);

            // Assert
            InvalidDenominationException exception = Assert.Throws<InvalidDenominationException>(act);
        }

        [Fact]
        public void TestEndTransaction() {
            // Arrange
            VendingMachine vm = new(CurrencyCode.SEK, Array.Empty<int>());
            vm.InsertMoney(10);
            vm.InsertMoney(10);
            vm.InsertMoney(10);
            vm.InsertMoney(2);
            vm.InsertMoney(2);
            vm.InsertMoney(2);
            vm.InsertMoney(1);
            vm.InsertMoney(1);
            vm.InsertMoney(1);

            Dictionary<int, int> expectedCoins = new() {
                {20, 1},
                {10, 1},
                { 5, 1},
                { 2, 2}
            };
            int expectedBalanceBefore = 39;
            int expectedBalanceAfter = 0;

            // Act
            int resultBalanceBefore = vm.Balance;
            Dictionary<int, int> resultCoins = vm.EndTransaction();
            int resultBalanceAfter = vm.Balance;

            // Assert
            Assert.Equal(expectedBalanceBefore, resultBalanceBefore);
            Assert.Equal(expectedCoins, resultCoins);
            Assert.Equal(expectedBalanceAfter, resultBalanceAfter);
        }

        [Fact]
        public void TestPurchaseBalanceTooLow() {
            // Arrange
            VendingMachine vm = new();
            Product product = vm.GetAllProducts()[0];

            // Act
            vm.InsertMoney(1);
            void act() => vm.Purchase(product.Name);

            // Assert
            BalanceTooLowException exception = Assert.Throws<BalanceTooLowException>(act);
        }

        [Fact]
        public void TestPurchaseProductNotFound() {
            // Arrange
            VendingMachine vm = new();
            string productName = "4p$R86B$x43E";

            // Act
            vm.InsertMoney(1);
            void act() => vm.Purchase(productName);

            // Assert
            Assert.Throws<ProductNotFoundException>(act);
        }

        [Fact]
        public void TestPurchaseSuccessfulAndBalanceTooLow() {
            // Arrange
            VendingMachine vm = new();
            List<Product> products = vm.GetAllProducts();
            Product product = products[0];

            string expectedResult = product.Usage;
            int expectedBalanceAfter = product.Price - 1;

            // Act
            int insertMoney = product.Price * 2 - 1;
            while (vm.Balance < insertMoney) {
                vm.InsertMoney(1);
            }
            string result = vm.Purchase(product.Name);

            // Assert

            Assert.Equal(expectedResult, result);
            Assert.Equal(expectedBalanceAfter, vm.Balance);
            Assert.Throws<BalanceTooLowException>(() => vm.Purchase(product.Name));
            Assert.Equal(expectedBalanceAfter, vm.Balance);
        }
    }
}


