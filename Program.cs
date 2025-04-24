using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace BankCustomerTransactions
{
    class Program
    {
        static void Main(string[] args)
        {
            IWebDriver driver = new ChromeDriver();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            driver.Manage().Window.Maximize();

            try
            {
                Console.WriteLine("Navigating to the banking site...");
                driver.Navigate().GoToUrl("https://www.globalsqa.com/angularJs-protractor/BankingProject/#/login");

                LoginAsCustomer(driver, wait, "Hermoine Granger");
                SelectAccount(driver, "1003");

                Console.WriteLine("Logged in as Hermoine Granger (Account 1003)\n");

                var transactions = new List<Transaction>
                {
                    new Transaction(50000, TransactionType.Credit),
                    new Transaction(3000, TransactionType.Debit),
                    new Transaction(2000, TransactionType.Debit),
                    new Transaction(5000, TransactionType.Credit),
                    new Transaction(10000, TransactionType.Debit),
                    new Transaction(15000, TransactionType.Debit),
                    new Transaction(1500, TransactionType.Credit)
                };

                decimal expectedBalance = 0;

                foreach (var tx in transactions)
                {
                    PerformTransaction(driver, wait, tx);

                    // Update expected balance
                    expectedBalance = tx.Type == TransactionType.Credit
                        ? expectedBalance + tx.Amount
                        : expectedBalance - tx.Amount;

                    decimal actualBalance = GetDisplayedBalance(driver);

                    if (actualBalance == expectedBalance)
                    {
                        Console.WriteLine($" {tx.Type} {tx.Amount} → Balance OK: {actualBalance}");
                    }
                    else
                    {
                        Console.WriteLine($" {tx.Type} {tx.Amount} → Balance MISMATCH: Expected {expectedBalance}, Got {actualBalance}");
                    }

                    Thread.Sleep(300); // Small wait for UI refresh
                }

                Console.WriteLine($"\n Test completed. Final Balance: {expectedBalance}");
                Console.WriteLine("Press any key to close...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                driver.Quit();
            }
        }

        // === Reusable Helpers ===

        static void LoginAsCustomer(IWebDriver driver, WebDriverWait wait, string customerName)
        {
            driver.FindElement(By.XPath("//button[text()='Customer Login']")).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id("userSelect")));

            var userSelect = new SelectElement(driver.FindElement(By.Id("userSelect")));
            userSelect.SelectByText(customerName);

            driver.FindElement(By.XPath("//button[text()='Login']")).Click();
        }

        static void SelectAccount(IWebDriver driver, string accountNumber)
        {
            var accountSelect = new SelectElement(driver.FindElement(By.Id("accountSelect")));
            accountSelect.SelectByText(accountNumber);
        }

        static void PerformTransaction(IWebDriver driver, WebDriverWait wait, Transaction tx)
        {
            string action = tx.Type == TransactionType.Credit ? "Deposit" : "Withdrawl";

            driver.FindElement(By.XPath($"//button[contains(text(),'{action}')]")).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//input[@ng-model='amount']")));

            var input = driver.FindElement(By.XPath("//input[@ng-model='amount']"));
            input.Clear();
            input.SendKeys(tx.Amount.ToString(CultureInfo.InvariantCulture));

            driver.FindElement(By.XPath($"//button[text()='{action}']")).Click();
        }

        static decimal GetDisplayedBalance(IWebDriver driver)
        {
            var balanceText = driver.FindElement(By.XPath("//strong[.='Balance :']/following-sibling::strong")).Text;
            return decimal.Parse(balanceText.Trim(), CultureInfo.InvariantCulture);
        }
    }

    enum TransactionType
    {
        Credit,
        Debit
    }

    class Transaction
    {
        public decimal Amount { get; }
        public TransactionType Type { get; }

        public Transaction(decimal amount, TransactionType type)
        {
            Amount = amount;
            Type = type;
        }
    }
}
