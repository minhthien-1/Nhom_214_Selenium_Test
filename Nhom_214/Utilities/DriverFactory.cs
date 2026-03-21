using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Nhom_214.Utilities
{
    public class DriverFactory
    {
        public static IWebDriver CreateDriver()
        {
            var driver = new ChromeDriver();
            driver.Manage().Window.Maximize();
            // Địa chỉ web bạn đang chạy trên VS Code
            driver.Navigate().GoToUrl("http://localhost:5000");
            return driver;
        }

    }
}
