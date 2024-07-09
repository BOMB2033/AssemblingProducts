using Microsoft.Office.Interop.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;
using Page = System.Windows.Controls.Page;

namespace AssemblingProducts.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public List<string> ActualData = new List<string>();
        public MainPage()
        {
            InitializeComponent();
        }
        public void DownloadFile(string path)
        {
            WebDriver driver;
            if (radioButtonEdge.IsChecked == false)
                driver = new ChromeDriver();
            else
                driver = new EdgeDriver();
            driver.Navigate().GoToUrl(path);
            driver.FindElement(By.Name("j_username")).SendKeys(textBoxLogin.Text);
            driver.FindElement(By.Name("j_password")).SendKeys(textBoxPassword.Password);
            driver.FindElement(By.Name("submitButton")).Submit();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            Thread.Sleep(10000);
            driver.FindElement(By.XPath("//div[@class='btn btn-enabled btn-gray tutorial-stage-sales-tenth-step tutorial-2-print-button']")).Click();
            driver.FindElement(By.XPath("//td[. = 'Заказ с кодами']")).Click();
            var elements = driver.FindElements(By.XPath("//td[@class='cell numeric available']/div[@class='div-viewer widget']"));
            foreach (var item in elements)
                ActualData.Add(item.Text);
            Thread.Sleep(7000);
            driver.FindElement(By.XPath("//table[@class='user-panel-new tutorial-settings']")).Click();
            driver.FindElement(By.XPath("//td[. = 'Выход']")).Click();
            driver.Close();
            driver.Quit();
        }
        private void btnStartParse_Click(object sender, RoutedEventArgs e)
        {
            DownloadFile(textBoxLonk.Text);
            EditFile(GetNameFile());
        }
        public string GetNameFile()
        {
            DirectoryInfo directory = new DirectoryInfo(
           Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads");
            var files = directory.GetFiles("order code productCode-*.xls");
            DateTime tempTime = files[0].CreationTime;
            int tempIndex = 0;
            for (int i = 0; i < files.Length; i++)
            {
                if (tempTime < files[i].CreationTime)
                {
                    tempTime = files[i].CreationTime;
                    tempIndex = i;
                }
            }
            if (tempTime.AddMinutes(50) > DateTime.Now)
            {
                return files[tempIndex].FullName;
            }
            return null;
        }

        public void EditFile(string path)
        {
            if (path != null)
            {
                // Создаем объект приложения Excel
                Excel.Application excelApp = new Excel.Application();

                // Открываем файл XLS
                Excel.Workbook workbook = excelApp.Workbooks.Open(path);

                // Получаем первый лист в книге
                Excel.Worksheet worksheet = (Excel.Worksheet)workbook.Sheets[1];

                // Получение используемого диапазона
                Range range = worksheet.UsedRange;

                // Удаление второго столбца
                ((Range)worksheet.Columns[2]).Delete(XlDeleteShiftDirection.xlShiftToLeft);
                ((Range)worksheet.Columns[8]).Cells[10].Value = "Статус сборки";
                int i = 11;
                while (true)
                    if (((Range)worksheet.Columns[8]).Cells[i].Value == null)
                        break;
                    else
                        ((Range)worksheet.Columns[8]).Cells[i++].Value = "";

                ((Range)worksheet.Columns[7]).Cells[10].Value = "Наличие";
                i = 11;
                while (true)
                    if (((Range)worksheet.Columns[7]).Cells[i].Value == null)
                        break;
                    else
                        ((Range)worksheet.Columns[7]).Cells[i].Value = ActualData[(i++) - 11];
                ((Range)worksheet.Columns[5]).Delete(XlDeleteShiftDirection.xlShiftToLeft);
                ((Range)worksheet.Columns[1]).Cells[5].Value = "СБОРКА " + ((Range)worksheet.Columns[1]).Cells[5].Value;
                // Сохраняем изменения
                workbook.Save();
                worksheet.PrintOut(Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                // Закрываем книгу
                workbook.Close();

                // Закрываем приложение Excel
                excelApp.Quit();

                MessageBox.Show("Успешно завершено!");
            }
            else
            {
                MessageBox.Show("Файл не скачался! Или слишком долго скачивался!\nПопробуйте снова\nИли зайдите в прочее и выберите файл!");
            }
        }
    }
}
