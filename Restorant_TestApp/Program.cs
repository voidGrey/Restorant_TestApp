using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

using Restorant_TestApp.Entities;
using RestSharp;
using System.Text;

namespace Restorant_TestApp
{
    internal class Program
    {
        public static WebDriver webDriver;
        public static bool IlkGiris = true;

        public static string mail = "deneme@gmail.com";
        public static string sifre = "05385547740Ms?!";

        private static void Main(string[] args)
        {
            baslangic:
            Console.Clear();
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Restorant MVC Otomasyon Tester");
            Console.ResetColor();
            Console.WriteLine("\n⬆️  ve ⬇️  Tuşlarına basarak menüde gezebilirsin \u001b[32mEnter/Return\u001b[0m ile seçebilirsiniz:");
            (int left, int top) = Console.GetCursorPosition();
            var option = 1;
            var decorator = "✅ \u001b[36m";
            ConsoleKeyInfo key;
            bool isSelected = false;

            while (!isSelected)
            {
                Console.SetCursorPosition(left , top);

                Console.WriteLine($"{(option == 1 ? decorator : "   ")}Tüm Kategorileri Ekle\u001b[0m");
                Console.WriteLine($"{(option == 2 ? decorator : "   ")}Rastgele Yemek Ekle\u001b[0m");

                key = Console.ReadKey(false);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        option = option == 1 ? 3 : option - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        option = option == 3 ? 1 : option + 1;
                        break;

                    case ConsoleKey.Enter:
                        isSelected = true;
                        break;
                }
            }

            switch (option)
            {
                case 1:
                    TumKategorileriEkle();
                    break;

                case 2:
                    TumYemekleriEkle();
                    break;

                default:
                    break;
            }

            Console.WriteLine($"\n{decorator} İşlem Tamamlandı");
            Console.ReadLine();
            Console.Clear();

            goto baslangic;
        }

        public static Categories KategoriCek()
        {
            // API isteği için RestSharp kullanıyoruz
            var client = new RestClient("https://www.themealdb.com/api/json/v1/1/categories.php");
            var request = new RestRequest("categories.php", Method.Get);

            // API'den verileri çekmek için isteği gönder
            var response = client.Execute(request);

            if (response.IsSuccessful)
            {
                // JSON verilerini çözümle
                string json = response.Content;

                Categories categoryResponse = JsonConvert.DeserializeObject<Categories>(json);
                return categoryResponse;
            }
            else
            {
                Console.WriteLine("API isteği başarısız oldu. Hata: " + response.ErrorMessage);
                return null;
            }
        }

        public static Meals YemekCek(int count)
        {
            Meals TotalMeals = new Meals();
            TotalMeals.meals = new();

            for (int i = 0; i < count; i++)
            {
                var client = new RestClient("https://www.themealdb.com/api/json/v1/1/random.php");
                var request = new RestRequest("random.php", Method.Get);

                var response = client.Execute(request);

                if (response.IsSuccessful)
                {
                    string json = response.Content;

                    Meals mealsResponse = JsonConvert.DeserializeObject<Meals>(json);
                    Meal meal = mealsResponse.meals.FirstOrDefault();
                    TotalMeals.meals.Add(meal);
                }
                else
                {
                    Console.WriteLine("API isteği başarısız oldu. Hata: " + response.ErrorMessage);
                }
            }

            return TotalMeals;
        }

        public static async void TumYemekleriEkle()
        {
            Console.WriteLine("\u001b[32m Eklemek istediğiniz yemek adeti:");
            int adet = int.Parse(Console.ReadLine());

            Meals meals = YemekCek(adet);

            foreach (var item in meals.meals)
            {
                await PaneleGir("Urun/Create");

                webDriver.Navigate().GoToUrl("https://localhost:7142/Admin/Urun/Create");

                var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));

                var urunBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("UrunAdi")));
                urunBox.SendKeys(item.strMeal);

                var fotoBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Name("FotografLink")));
                fotoBox.SendKeys(item.strMealThumb);

                Random random = new Random();
                int rastGeleFiyat = random.Next(10,500);

                var fiyatBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("Fiyat")));
                fiyatBox.SendKeys(rastGeleFiyat.ToString());

                var kategorilerSelectBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("KategoriID")));
                var selectElement = new SelectElement(kategorilerSelectBox);

                selectElement.SelectByText(item.strCategory);

                var olusturButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("olustur")));

                olusturButton.Click();
            }
        }

        public static async void TumKategorileriEkle()
        {
            Categories categories = KategoriCek();
            foreach (var item in categories.categories)
            {
                await PaneleGir("Kategori/Create");

                webDriver.Navigate().GoToUrl("https://localhost:7142/Admin/Kategori/Create");

                var wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));

                ///SeleniumExtras.WaitHelpers.ExpectedConditions.UrlToBe("https://localhost:7142/Admin/Kategori/Create");

                var kategoriBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("KategoriAdi")));
                var kategoriAciklamaBox = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("KategoriAciklama")));

                kategoriBox.SendKeys(item.strCategory);
                kategoriAciklamaBox.SendKeys(item.strCategoryDescription);

                var olusturButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("olustur")));
                olusturButton.Click();
            }
        }

        public static async Task<bool> PaneleGir(string Controller)
        {
            webDriver = DriverAl();
            if (IlkGiris)
            {
                webDriver.Navigate().GoToUrl("https://localhost:7142/FirmaLogin/Login");

                var mailBox = webDriver.FindElement(By.Id("Email"));
                var passwordBox = webDriver.FindElement(By.Id("login__password"));
                var girisButton = webDriver.FindElement(By.CssSelector(".login input[type=\"submit\"]"));

                mailBox.SendKeys(mail);
                passwordBox.SendKeys(sifre);
                girisButton.Click();

                IlkGiris = false;
            }
            webDriver.Navigate().GoToUrl("https://localhost:7142/Admin/" + Controller);
            return true;
        }

        public static WebDriver DriverAl()
        {
            if (webDriver == null)
            {
                WebDriver driver = new ChromeDriver();
                return driver;
            }
            else
            {
                return webDriver;
            }
        }
    }
}