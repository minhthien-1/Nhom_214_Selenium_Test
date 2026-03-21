using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Nhom_214.Pages
{
    public class RoomPage
    {
        private IWebDriver driver;

        // 1. Locators cho nút chức năng chính
        private By btnCreateResort = By.XPath("//button[contains(text(), '+ Tạo Resort Mới')]");
        private By btnAddRoom = By.XPath("(//button[contains(text(), '+ Thêm phòng')])[1]"); // Lấy nút thêm phòng đầu tiên
        private By btnEditRoom = By.XPath("(//button[text()='Sửa'])[1]");
        private By btnDeleteRoom = By.XPath("(//button[text()='Xóa'])[1]");

        // 2. Locators trong Modal Thêm/Sửa Phòng
        private By ddlRoomType = By.Id("room-type-id");
        private By txtPrice = By.Id("room-price");
        private By ddlBed = By.Id("num-bed");
        private By ddlStatus = By.Id("room-status");
        private By ddlLocation = By.Id("room-location");
        private By txtAddress = By.Id("room-address");
        private By txtDescription = By.Id("room-description");
        private By btnSaveRoom = By.XPath("//button[contains(@class, 'bg-blue-600') or contains(text(), 'Thêm Phòng') or contains(text(), 'Lưu Thay Đổi')]");
        private By btnCancelRoom = By.XPath("//button[contains(text(), 'Hủy')]");

        public RoomPage(IWebDriver driver)
        {
            this.driver = driver;
        }

        // ================= CÁC HÀM THAO TÁC =================

        // --- Resort ---
        public void ClickCreateResort() => driver.FindElement(btnCreateResort).Click();

        public void HandleResortPrompt(string resortName, bool isAccept)
        {
            IAlert promptAlert = driver.SwitchTo().Alert();
            if (resortName != "") promptAlert.SendKeys(resortName);

            if (isAccept) promptAlert.Accept();
            else promptAlert.Dismiss();
        }

        // --- Phòng ---
        public void ClickAddRoom() => driver.FindElement(btnAddRoom).Click();
        public void ClickFirstEditRoom() => driver.FindElement(btnEditRoom).Click();
        public void ClickFirstDeleteRoom() => driver.FindElement(btnDeleteRoom).Click();

        public void HandleDeleteConfirm(bool isAccept)
        {
            IAlert confirmAlert = driver.SwitchTo().Alert();
            if (isAccept) confirmAlert.Accept();
            else confirmAlert.Dismiss();
        }

        public void EnterRoomData(string typeValue, string price, string bedValue, string statusValue, string locationValue)
        {
            if (typeValue != "") new SelectElement(driver.FindElement(ddlRoomType)).SelectByValue(typeValue);

            driver.FindElement(txtPrice).Clear();
            if (price != "") driver.FindElement(txtPrice).SendKeys(price);

            if (bedValue != "") new SelectElement(driver.FindElement(ddlBed)).SelectByValue(bedValue);
            if (statusValue != "") new SelectElement(driver.FindElement(ddlStatus)).SelectByValue(statusValue);
            if (locationValue != "") new SelectElement(driver.FindElement(ddlLocation)).SelectByValue(locationValue);
        }

        public void ClickSaveRoom() => driver.FindElement(btnSaveRoom).Click();
        public void ClickCancelRoom() => driver.FindElement(btnCancelRoom).Click();
    }
}