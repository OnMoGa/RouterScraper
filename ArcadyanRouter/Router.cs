using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using AngleSharp;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.DOM;

namespace ArcadyanRouter {


	public enum Page {
		Login,
		Home,
		WifiDevices
	}

	public class Router {
		private Uri baseUri;
		private string username;
		private string password;
		private IWebDriver driver;

		private Page currentPage;



		public bool loggedIn {
			get {
				return driver.tryFindElement(By.CssSelector(".icon-lock"), TimeSpan.FromSeconds(2)) != null;
			}
		}


		public Router(Uri baseUri, string username, string password) {
			this.baseUri=baseUri;
			this.username=username;
			this.password=password;

			string driverDirectory = Directory.GetCurrentDirectory();
			driver = new ChromeDriver(driverDirectory);
			currentPage = loggedIn ? Page.Home : Page.Login;
		}


		public IEnumerable<WifiDevice> getWifiDevices() {

			if (!loggedIn) {
				login();
			}

			if (currentPage == Page.Home) {
				IWebElement wifiDevicesButton = driver.tryFindElement(By.Id("owl_lan_device"));
				wifiDevicesButton.Click();
				currentPage = Page.WifiDevices;
			} else {
				IWebElement refreshButton = driver.FindElement(By.CssSelector(".btn.btn-primary.btn-large"));
				refreshButton.Click();
			}

			


			IWebElement wifiDevicesTable = driver.tryFindElement(By.Id("tb_station_info0"));

			IEnumerable<IWebElement> rows = wifiDevicesTable.FindElements(By.CssSelector("tr")).Skip(1);

			List<WifiDevice> devices = new List<WifiDevice>();

			foreach (IWebElement row in rows) {
				ReadOnlyCollection<IWebElement> cols = row.FindElements(By.CssSelector("td"));
				
				WifiDevice device = new WifiDevice();

				device.name = cols[1].Text;
				device.macAddress = PhysicalAddress.Parse(cols[2].Text.Replace(':', '-'));

				IPAddress.TryParse(cols[3].Text, out IPAddress ipv4);
				device.ipv4Address = ipv4;

				IPAddress.TryParse(cols[4].Text, out IPAddress ipv6);
				device.ipv6Address = ipv6;

				device.connection = cols[5].Text switch {
					"5GHz" => Connection.Wifi5,
					"2.4GHz" => Connection.Wifi2,
					_ => Connection.Ethernet
				};

				device.signalStrength = int.Parse(cols[6].Text[..^3]);

				string link = cols[7].Text[..^4];
				device.linkRateMbps = int.Parse(link);

				devices.Add(device);
			}

			return devices;
		}




		private void login() {
			driver.Navigate().GoToUrl($"{baseUri}login.htm");

			currentPage = Page.Home;
			if(loggedIn) return;
			currentPage = Page.Login;

			IWebElement logoutButton = driver.tryFindElement(By.CssSelector(".icon-lock"), TimeSpan.FromSeconds(2));
			logoutButton?.Click();

			IWebElement usernameInput = driver.tryFindElement(By.Id("usernameNormal"));
			usernameInput.Clear();
			usernameInput.SendKeys(username);

			IWebElement passwordInput = driver.tryFindElement(By.Id("passwordNormal"));
			passwordInput.Clear();
			passwordInput.SendKeys(password);
			passwordInput.SendKeys("\n");

			currentPage = Page.Home;
		}



		




	}



	public static class IWebDriverExtentions {

		public static IWebElement tryFindElement(this IWebDriver driver, By by, TimeSpan? timeout = null) {
			DateTime start = DateTime.Now;
			while (true) {
				try {
					IWebElement element = driver.FindElement(by);
					return element;
				} catch (NoSuchElementException e) {
					//Console.WriteLine("Cant find element");
					if (timeout != null && DateTime.Now.Subtract(start) > timeout) {
						return null;
					}
					Thread.Sleep(100);
				}
			}
		}


	}


}
