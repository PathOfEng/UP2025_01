using Aspose.BarCode.Generation;
using Aspose.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.RegularExpressions;
using System.Net;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;


namespace UP2025_01
{
    /// <summary>
    /// Логика взаимодействия для Cabinet.xaml
    /// </summary>
    public partial class Cabinet : Window
    {
        //TimeSpan.FromHours(2) + 
        //Таймер для лаборантов
        TimeSpan remainingTime = TimeSpan.FromHours(2) + TimeSpan.FromMinutes(30);
        //Массив для манипуляции с услугами из бд
        List<услуги_> услуги = MainWindow.dbEntities.услуги_.ToList();
        //Массив для работы с заказами
        ObservableCollection<услуги_> gridServices = new ObservableCollection<услуги_>();
        //Массив для манипуляции с данными пациентов
        List<пользователи_>пациенты = MainWindow.dbEntities.пользователи_.Where(x => x.код_роли == 4).ToList();
        //Массив для получения истории входов
        List<история_входа_> история = MainWindow.dbEntities.история_входа_.ToList();
        //Массив для получения всех ролей в бд
        List<роль_> роли = MainWindow.dbEntities.роль_.ToList();
        //Переменная, чтобы при окончании таймера, не вылезало сообщение
        private bool autoExit;
        //Массив для работы лаборанта-исследователя
        List<подробности_заказа_анализатор_> работаАнализатора = new List<подробности_заказа_анализатор_>();
        List<Анализатор_> анализаторы = MainWindow.dbEntities.Анализатор_.ToList();
        string name = "";
        /// <summary>
        /// Создания формы кабинет
        /// </summary>
        public Cabinet()
        {
            InitializeComponent();
            autoExit = false;
            HideAllGrids();
            HideAllTabs();
            string sourcePath;
            if (MainWindow.currentUser.код_роли == 3)
            {
                adm_users_grid.ItemsSource = история;
                sourcePath = Environment.CurrentDirectory.ToString() + @"\Администратор.png";
                ShowAdmTabs();
            }
            else if (MainWindow.currentUser.код_роли == 1)
            {
                ShowLB1Tabs();
                lb1_patient.ItemsSource = пациенты;
                lb1_service.ItemsSource = услуги;
                lb1_serviceGrid.ItemsSource = gridServices;
                sourcePath = Environment.CurrentDirectory.ToString() + @"\laborant_1.jpeg";

            }
            else if(MainWindow.currentUser.код_роли == 2)
            {
                ShowBuchTabs();
                sourcePath = Environment.CurrentDirectory.ToString() + @"\Бухгалтер.jpeg";
            }
            else
            {
                ShowLB2Tabs();
                sourcePath = Environment.CurrentDirectory.ToString() + @"\laborant_2.png";
                AnalizatorDataGrid.ItemsSource = работаАнализатора;
                analizatorsComboBox.ItemsSource = анализаторы;
            }
            ImageSourceValueSerializer s = new ImageSourceValueSerializer();
            avatar.Source = (ImageSource)s.ConvertFromString(sourcePath, null);
            FIO.Content = MainWindow.currentUser.наименование;
            role.Content = MainWindow.currentUser.роль_.наименование;
            if(MainWindow.currentUser.код_роли == 5 || MainWindow.currentUser.код_роли == 1)
            {
                StartTimer();
            }
        }

        /// <summary>
        /// Метод таймер, который ограничивает работу лаборантов
        /// </summary>
        private void StartTimer()
        {
            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (remainingTime.TotalSeconds == 60)
                    {
                        MessageBox.Show("Осталось 15 минут до кварцевания");
                    }
                    if (remainingTime.TotalHours == 0  && remainingTime.TotalMinutes == 0 && remainingTime.TotalSeconds == 0)
                    {
                        история_входа_ история = new история_входа_();
                        история.статус_входа = 4;
                        история.код_пользователя = MainWindow.currentUser.код;
                        история.дата = DateTime.Now;
                        история.ip = MainWindow.GetLocalIpAddress();
                        MainWindow.dbEntities.история_входа_.Add(история);
                        MainWindow.dbEntities.SaveChanges();
                        timer.AutoReset = false;
                        autoExit = true;
                        timer.Stop();
                        Application.Current.Shutdown();
                    }
                    remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
                    apex.Title = "Времени осталось: " + remainingTime.ToString(@"hh\:mm\:ss");
                });
            };
            timer.AutoReset = true;
            timer.Start();
        }

        /// <summary>
        /// Метод для сокрытия всех вкладок
        /// </summary>
        private void HideAllTabs()
        {
            supplies_tab.Visibility = Visibility.Collapsed;
            graphics_tab.Visibility = Visibility.Collapsed;
            users_tab.Visibility = Visibility.Collapsed;
            create_report_tab.Visibility = Visibility.Collapsed;
            add_user_tab.Visibility = Visibility.Collapsed;
            create_insurance.Visibility = Visibility.Collapsed;
            reports_tab.Visibility = Visibility.Collapsed;
            analisator_job.Visibility = Visibility.Collapsed;
            order_tab.Visibility = Visibility.Collapsed;

        }
        /// <summary>
        /// Метод для сокрытия всех сеток данных
        /// </summary>
        private void HideAllGrids()
        {
            supplies_tab_grid.Visibility = Visibility.Collapsed;
            graphics_tab_grid.Visibility = Visibility.Collapsed;
            users_tab_grid.Visibility = Visibility.Collapsed;
            create_report_tab_grid.Visibility = Visibility.Collapsed;
            create_insurance_grid.Visibility = Visibility.Collapsed;
            reports_tab_grid.Visibility = Visibility.Collapsed;
            analisator_job_grid.Visibility = Visibility.Collapsed;
            order_tab_grid.Visibility = Visibility.Collapsed;
            add_user_tab_grid.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Метод для показа вкладок и сетки данных администратора
        /// </summary>
        private void ShowAdmTabs()
        {
            supplies_tab.Visibility = Visibility.Visible;
            graphics_tab.Visibility = Visibility.Visible;
            users_tab.Visibility = Visibility.Visible;
            create_report_tab.Visibility = Visibility.Visible;
            add_user_tab.Visibility = Visibility.Visible;

            supplies_tab_grid.Visibility = Visibility.Visible;
            graphics_tab_grid.Visibility = Visibility.Visible;
            users_tab_grid.Visibility = Visibility.Visible;
            create_report_tab_grid.Visibility = Visibility.Visible;
            add_user_tab_grid.Visibility = Visibility.Visible;

            roles.ItemsSource = роли;
        }

        /// <summary>
        /// Метод для показа вкладок и сетки данных бухгалтера(в разработке)
        /// </summary>
        private void ShowBuchTabs()
        {
            create_insurance.Visibility = Visibility.Visible;
            reports_tab.Visibility = Visibility.Visible;

            create_insurance_grid.Visibility = Visibility.Visible;
            reports_tab_grid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Метод для показа вкладок и сетки данных лаборанта-исследователя
        /// </summary>
        private void ShowLB2Tabs()
        {
            analisator_job.Visibility = Visibility.Visible;

            analisator_job_grid.Visibility = Visibility.Visible;
        }
        /// <summary>
        /// Метод для показа вкладок и сетки данных лаборанта
        /// </summary>
        private void ShowLB1Tabs()
        {
            order_tab.Visibility = Visibility.Visible;

            order_tab_grid.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Обработка закрытия формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (autoExit)
            {

            }
            else
            {
                var exit = MessageBox.Show("Вы действительно хотите закрыть приложение?", "Выход из приложения", MessageBoxButton.YesNo);
                if (exit == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Кнопка, которая возвращает на форму авторизации
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void inTheMainWindow_Click(object sender, RoutedEventArgs e)
        {
            autoExit = true;
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        /// <summary>
        /// Метод, который предотвращает возможность дублирования услуг на форме формирования заказа лаборантом
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lb1_add_service_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = gridServices.Where(x => x.наименование.Contains(услуги[lb1_service.SelectedIndex].наименование)).FirstOrDefault();
                if (item == null)
                {
                    gridServices.Add(услуги[lb1_service.SelectedIndex]);
                }  
            }
            catch (Exception ex)
            {
                if (lb1_service.SelectedIndex == -1)
                {

                }
            }
        }

        private void lb1_patient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void lb1_service_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Метод для формирования заказа лаборантом на вкладке "Сформировать заказ"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lb1_add_order_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lb1_patient.SelectedItem != null && gridServices.Count > 0)
                {
                    //Добавление записи о заказе в бд
                    заказ_ заказ = new заказ_();
                    заказ.код_пользователя = пациенты[lb1_patient.SelectedIndex].код;
                    заказ.статус_заказа = 1;
                    заказ.время_выполнения = gridServices.Sum(x => x.срок_выполнения).ToString();
                    MainWindow.dbEntities.заказ_.Add(заказ);
                    MainWindow.dbEntities.SaveChanges();

                    //Добавление записей в таблицу подробности заказа без даты выполнения
                    for (int i = 0; i < gridServices.Count; i++)
                    {
                        подробности_заказа_ подробности_заказа = new подробности_заказа_();
                        int k = MainWindow.dbEntities.заказ_.ToList().LastOrDefault().код;
                        подробности_заказа.код_заказа = k;
                        подробности_заказа.код_услуги = gridServices[i].код;
                        подробности_заказа.статус_услуги = 1; //Ожидает выполнения
                        MainWindow.dbEntities.подробности_заказа_.Add(подробности_заказа);
                        MainWindow.dbEntities.SaveChanges();
                        подробности_заказа_анализатор_ анализатор = new подробности_заказа_анализатор_();
                        анализатор.код_подробности = MainWindow.dbEntities.подробности_заказа_.ToList().LastOrDefault().код;
                        анализатор.дата_поступления = DateTime.Now;
                        MainWindow.dbEntities.подробности_заказа_анализатор_.Add(анализатор);
                        MainWindow.dbEntities.SaveChanges();
                    }
                    Random rand = new Random();
                    string uniqueCode = rand.Next(100000, 1000000).ToString();

                    string currentDate = DateTime.Now.ToString("ddMMyyyy");

                    string orderId = MainWindow.dbEntities.заказ_.ToList().LastOrDefault().код.ToString();
                    string barcodeText = orderId + currentDate + uniqueCode;

                    // Создание штрих-кода
                    string imagePath = Environment.CurrentDirectory.ToString() + "barcode.Jpeg";

                    var imageFormat = (BarCodeImageFormat)Enum.Parse(typeof(BarCodeImageFormat), "Jpeg");

                    BarcodeGenerator barcodeGenerator = new BarcodeGenerator(EncodeTypes.Code128, barcodeText);

                    barcodeGenerator.Parameters.Resolution = 300;

                    barcodeGenerator.Save(imagePath, imageFormat);

                    Document doc = new Document();

                    int lowerLeftX = 0;
                    int lowerLeftY = 750;
                    int upperRightX = 170;
                    int upperRightY = 850;

                    Aspose.Pdf.Page page = doc.Pages.Add();

                    Aspose.Pdf.Page page1 = doc.Pages[1];

                    FileStream image = new FileStream(imagePath, FileMode.Open);

                    page1.Resources.Images.Add(image);

                    page1.Contents.Add(new Aspose.Pdf.Operators.GSave());

                    Aspose.Pdf.Rectangle rectangle = new Aspose.Pdf.Rectangle(lowerLeftX, lowerLeftY, upperRightX, upperRightY);
                    Aspose.Pdf.Matrix matrix = new Aspose.Pdf.Matrix(new double[] { rectangle.URX - rectangle.LLX, 0, 0, rectangle.URY - rectangle.LLY, rectangle.LLX, rectangle.LLY });

                    page.Contents.Add(new Aspose.Pdf.Operators.ConcatenateMatrix(matrix));
                    XImage ximage = page.Resources.Images[page.Resources.Images.Count];

                    page.Contents.Add(new Aspose.Pdf.Operators.Do(ximage.Name));

                    page.Contents.Add(new Aspose.Pdf.Operators.GRestore());

                    doc.Save(Environment.CurrentDirectory.ToString() + barcodeText +".pdf");

                    

                    MessageBox.Show("Заказ успешно сформирован");
                }
                else if (lb1_patient.SelectedItem == null && gridServices.Count == 0)
                {
                    MessageBox.Show("Вы не выбрали пациента и услуги");
                }
                else if (gridServices.Count < 0)
                {
                    MessageBox.Show("Вы не выбрали услуги");
                }
                else if (lb1_patient.SelectedItem == null)
                {
                    AddPatient addPatient = new AddPatient();
                    addPatient.ShowDialog();
                    lb1_patient.Items.Refresh();
                    lb1_patient.SelectedIndex = lb1_patient.Items.Count - 1;
                    //MessageBox.Show("Выберите клиента(пациента)");
                }
            }
            catch (Exception ex)
            {
                if (lb1_service.SelectedIndex == -1)
                {

                }
                if (lb1_patient.SelectedIndex == -1)
                {

                }
            }
        }

        /// <summary>
        /// Метод для фильтрации данных по логину пользователя во вкладке "История входа"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void userComboBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            история = MainWindow.dbEntities.история_входа_.Where(x => x.пользователи_.логин.Contains(userBox.Text)).ToList();
            adm_users_grid.ItemsSource = история;
        }

        /// <summary>
        /// Метод, который предотвращает возможность сортировки по все столбцам, кроме "даты и времени", во вкладке "История входа"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void adm_users_grid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            if (e.Column.Header.ToString() != "дата и время")
            {
                e.Handled = true;

                e.Column.SortDirection = null;
            }
        }

        /// <summary>
        /// Метод для очистки полей на вкладке "Добавить сотрудника"
        /// </summary>
        private void ClearNewUserData()
        {
            new_fio.Text = string.Empty;
            login.Text = string.Empty;
            password.Text = string.Empty;
            telephoneNumber.Text = string.Empty;
            roles.SelectedIndex = -1;
            email.Text = string.Empty;
        }
        private void AddPatienToDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (new_fio.Text == "")
                {
                    throw new Exception("ФИО не заполнено");
                }
                else if (login.Text == "")
                {
                    throw new Exception("Логин не заполнен");
                }
                else if (password.Text == "")
                {
                    if (MainWindow.dbEntities.пользователи_.Where(x => x.логин == login.Text).FirstOrDefault() != null)
                    {
                        throw new Exception("Этот логин занят");
                    }
                    throw new Exception("Пароль не заполнен");
                }
                else if (email.Text == "")
                {
                    throw new Exception("Почта не заполнена");
                }
                else if (telephoneNumber.Text == "")
                {
                    throw new Exception("Телефон не заполнен");
                }
                else if (roles.SelectedIndex == -1)
                {
                    throw new Exception("Не выбрана страховая компания");
                }
                else
                {
                    пользователи_ пользователь = new пользователи_();

                    if (Mail_LIB.Validation.Check_login(login.Text))
                    {
                        пользователь.логин = login.Text;
                    }
                    else
                    {
                        throw new Exception("Введенный логин имеет неверный формат.");
                    }
                    if (Mail_LIB.Validation.Check_password(password.Text))
                    {
                        пользователь.пароль = password.Text;
                    }
                    else
                    {
                        throw new Exception("Пароль имеет неверный формат.");
                    }

                    if (Mail_LIB.Validation.Check_mail(email.Text))
                    {
                        пользователь.электронная_почта = email.Text;
                    }
                    else
                    {
                        throw new Exception("Введенная почта имеет неверный формат");
                    }
                    пользователь.код_роли = roles.SelectedIndex + 1;
                    пользователь.наименование = new_fio.Text;
                    Regex checkNumber = new Regex(@"^[1-9]{1}[0-9]{10}$");
                    if (checkNumber.IsMatch(telephoneNumber.Text))
                    {
                        пользователь.телефон = telephoneNumber.Text;
                    }
                    else
                    {
                        throw new Exception("Телефон имеет неверный формат");
                    }
                    MainWindow.dbEntities.пользователи_.Add(пользователь);
                    MessageBox.Show("Новый сотрудник успешно добавлен в систему");
                    //Очищаем все поля
                    ClearNewUserData();
                }
            }
            catch (Exception ex)
            {
                if (roles.SelectedIndex == -1)
                {

                }
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Метод для фильтрации данных в таблице в зависимости от анализатора
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void analizatorsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (анализаторы[analizatorsComboBox.SelectedIndex].код == 2)
            {
                работаАнализатора = MainWindow.dbEntities.подробности_заказа_анализатор_.Where(
                    x => (x.подробности_заказа_.статус_услуги_.код == 1 ||
                    x.подробности_заказа_.статус_услуги_.код == 4) &&
                    (
                        x.подробности_заказа_.услуги_.код == 619 ||
                        x.подробности_заказа_.услуги_.код == 548 ||
                        x.подробности_заказа_.услуги_.код == 258 ||
                        x.подробности_заказа_.услуги_.код == 176 ||
                        x.подробности_заказа_.услуги_.код == 543 ||
                        x.подробности_заказа_.услуги_.код == 855 ||
                        x.подробности_заказа_.услуги_.код == 836 ||
                        x.подробности_заказа_.услуги_.код == 659 ||
                        x.подробности_заказа_.услуги_.код == 797 ||
                        x.подробности_заказа_.услуги_.код == 287
                    )
                    ).OrderBy(x => x.подробности_заказа_.заказ_.пользователи_.наименование).ToList();
                barProgressBiorad.Visibility = Visibility.Visible;
                barProgressLedetect.Visibility = Visibility.Hidden;
                for (int i = 0; i < работаАнализатора.Count; i++)
                {
                    работаАнализатора[i].код_анализатора = 2;
                }
                MainWindow.dbEntities.SaveChanges();
                AnalizatorDataGrid.ItemsSource = работаАнализатора;
                
                AnalizatorDataGrid.Items.Refresh();
            }
            else if (анализаторы[analizatorsComboBox.SelectedIndex].код == 1)
            {
                работаАнализатора = MainWindow.dbEntities.подробности_заказа_анализатор_.Where(
                    x => (x.подробности_заказа_.статус_услуги_.код == 1 ||
                    x.подробности_заказа_.статус_услуги_.код == 4) &&
                    (
                        x.подробности_заказа_.услуги_.код == 619 ||
                        x.подробности_заказа_.услуги_.код == 311 ||
                        x.подробности_заказа_.услуги_.код == 258 ||
                        x.подробности_заказа_.услуги_.код == 501 ||
                        x.подробности_заказа_.услуги_.код == 543 ||
                        x.подробности_заказа_.услуги_.код == 557 ||
                        x.подробности_заказа_.услуги_.код == 229 ||
                        x.подробности_заказа_.услуги_.код == 415 ||
                        x.подробности_заказа_.услуги_.код == 323 ||
                        x.подробности_заказа_.услуги_.код == 346 ||
                        x.подробности_заказа_.услуги_.код == 659
                    )
                    ).OrderBy(x => x.подробности_заказа_.заказ_.пользователи_.наименование).ToList();
                barProgressBiorad.Visibility = Visibility.Hidden;
                barProgressLedetect.Visibility = Visibility.Visible;
                for (int i = 0; i < работаАнализатора.Count; i++)
                {
                    работаАнализатора[i].код_анализатора = 1;
                }
                MainWindow.dbEntities.SaveChanges();
                AnalizatorDataGrid.ItemsSource = работаАнализатора;
                AnalizatorDataGrid.Items.Refresh();
            }
            name = анализаторы[analizatorsComboBox.SelectedIndex].наименование;
        }
        
        /// <summary>
        /// Асинхронный метод для проверки(одобрения или неодобрения) результата
        /// </summary>
        /// <param name="getAnalizator"></param>
        /// <returns></returns>
        async Task BiomaterialCheck(GetAnalizator getAnalizator)
        {
            for (int i = 0; i < getAnalizator.services.Count; i++)
            {
                string result = getAnalizator.services[i].result.ToString();
                var changeElement = sendToCheckArray.Where(x => x.подробности_заказа_.услуги_.код == getAnalizator.services[i].serviceCode).ToList().FirstOrDefault();
                double temp;
                bool isConvertToDouble = Double.TryParse(result.Replace(".", ","), out temp);
                //Проверка на результат выполнения работы API
                if (isConvertToDouble)
                {
                    double lowerBorder = Convert.ToDouble(changeElement.подробности_заказа_.услуги_.нижняя_граница_нормы);
                    double hightBorder = Convert.ToDouble(changeElement.подробности_заказа_.услуги_.верхняя_граница_нормы);
                    double middleBorder = ((hightBorder + lowerBorder) * 1.0) / 2;
                    if (middleBorder * 5 >= temp || temp <= middleBorder / 5)
                    {
                        var isReady = MessageBox.Show("Результат исследования - " + changeElement.подробности_заказа_.услуги_.наименование + ", отклоняется от нормы в 5 раз\n" + "Вы одобряете резульат?", "Предупреждение", MessageBoxButton.YesNo);
                        if (isReady == MessageBoxResult.Yes)
                        {
                            changeElement.результат = temp.ToString();
                            changeElement.подробности_заказа_.статус_услуги = 2;
                        }
                        else
                        {
                            changeElement.подробности_заказа_.статус_услуги = 4;
                        }
                    }
                    else
                    {
                        changeElement.результат = temp.ToString();
                        changeElement.подробности_заказа_.статус_услуги = 2;
                    }
                }
                else
                {
                    changeElement.результат = result;
                    changeElement.подробности_заказа_.статус_услуги = 2;
                }
                MainWindow.dbEntities.SaveChanges();
                AnalizatorDataGrid.ItemsSource = работаАнализатора.Where(x => x.подробности_заказа_.статус_услуги_.код == 1 ||
                                    x.подробности_заказа_.статус_услуги_.код == 4).ToList();
                AnalizatorDataGrid.Items.Refresh();
            }

        }


        /// <summary>
        /// Асинхронный отдельный таймер для получения данных из API LEDETECT
        /// </summary>
        /// <returns></returns>
        async Task StartTimerLedetect()
        {
            var timeRemaining_1 = TimeSpan.FromSeconds(30);
            var timer_1 = new System.Timers.Timer(1000);
            barProgressLedetect.Value = 0;
            timer_1.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    timeRemaining_1 = timeRemaining_1.Subtract(TimeSpan.FromSeconds(1));
                    barProgressLedetect.Value += 1;
                    if (timeRemaining_1.TotalSeconds == 0)
                    {
                        GetAnalizator getAnalizator_1 = new GetAnalizator();
                        var httpWebRequest_1 = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                        httpWebRequest_1.ContentType = "application/json";
                        httpWebRequest_1.Method = "GET";
                        try
                        {
                            var httpResponse_1 = (HttpWebResponse)httpWebRequest_1.GetResponse();
                            if (httpResponse_1.StatusCode == HttpStatusCode.OK)
                            {
                                using (Stream stream_1 = httpResponse_1.GetResponseStream())
                                {
                                    StreamReader reader = new StreamReader(stream_1);
                                    string json_1 = reader.ReadToEnd();
                                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                                    getAnalizator_1 = serializer.Deserialize<GetAnalizator>(json_1);
                                    await BiomaterialCheck(getAnalizator_1);
                                    barProgressLedetect.Value = 0;
                                    timer_1.Enabled = false;
                                    MessageBox.Show("биоматериалы проанализированы");
                                }
                            }
                        }catch (Exception ex)
                        {
                            barProgressLedetect.Value = 0;
                            MessageBox.Show(ex.Message);
                        }
                        timer_1.AutoReset = false;
                        timer_1.Stop();
                    }
                });
            };
            timer_1.AutoReset = true;
            timer_1.Start();
        }

        /// <summary>
        /// Асинхронный отдельный таймер для получения данных из API BIORAD
        /// </summary>
        /// <returns></returns>
        async Task StartTimerBiorad()
        {
            var timeRemaining = TimeSpan.FromSeconds(30);
            var timer = new System.Timers.Timer(1000);
            barProgressBiorad.Value = 0;
            timer.Elapsed += (sender, e) =>
            {
                Dispatcher.Invoke(async () =>
                {
                    timeRemaining = timeRemaining.Subtract(TimeSpan.FromSeconds(1));
                    barProgressBiorad.Value += 1;
                    if (timeRemaining.TotalSeconds == 0)
                    {
                        GetAnalizator getAnalizator = new GetAnalizator();

                        var httpWebRequest = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                        httpWebRequest.ContentType = "application/json";
                        httpWebRequest.Method = "GET";

                        try
                        {
                            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                            if (httpResponse.StatusCode == HttpStatusCode.OK)
                            {
                                using (Stream stream = httpResponse.GetResponseStream())
                                {
                                    StreamReader reader = new StreamReader(stream);
                                    string json = reader.ReadToEnd();
                                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                                    getAnalizator = serializer.Deserialize<GetAnalizator>(json);
                                    await BiomaterialCheck(getAnalizator);

                                    barProgressBiorad.Value = 0;
                                    timer.Enabled = false;
                                    MessageBox.Show("биоматериалы проанализированы");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            barProgressBiorad.Value = 0;
                            MessageBox.Show(ex.Message);
                        }

                        timer.AutoReset = false;
                        timer.Stop();
                    }
                });
            };
            timer.AutoReset = true;
            timer.Start();
        }
        /// <summary>
        /// Инициализация глобального массива, которых хранит в себе элементы, которые "отправляются" в анализатор
        /// </summary>
        List<подробности_заказа_анализатор_> sendToCheckArray = new List<подробности_заказа_анализатор_>();
        /// <summary>
        /// Асинхронный метод нажатия кнопки для отправки данных в API
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SendToCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = Environment.CurrentDirectory.ToString() + "\\Analyzer\\Analyzer\\LIMSAnalyzers.exe";
                sendToCheckArray = работаАнализатора.Where(x => x.isSelected == true).ToList();
                var countPatients = sendToCheckArray.GroupBy(x => x.подробности_заказа_.заказ_.код_пользователя).ToList();
                if (countPatients.Count > 1)
                {
                    throw new Exception("Вы не можете выбрать нескольких пациентов");
                }
                //else if (работаАнализатора.Where(x => x.подробности_заказа_.статус_услуги == 3).ToList().LastOrDefault() != null)
                //{
                //    throw new Exception("На проверку можно отправлять только услуги со статусом - Ожидает и необходим повторный забор биоматериала");
                //}
                List<Services> services = new List<Services>();
                for (int i = 0; i < sendToCheckArray.Count; i++)
                {
                    Services services1 = new Services();
                    services1.serviceCode = sendToCheckArray[i].подробности_заказа_.услуги_.код;
                    services.Add(services1);
                }

                string patient = sendToCheckArray[0].подробности_заказа_.заказ_.код_пользователя.ToString();
                var s = Process.Start(path);
                if (analizatorsComboBox.SelectedIndex == 0)
                {
                    var httpWebRequest_1 = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                    httpWebRequest_1.ContentType = "application/json";
                    httpWebRequest_1.Method = "POST";

                    using (var streamWriter_1 = new StreamWriter(httpWebRequest_1.GetRequestStream()))
                    {

                        string json_1 = new JavaScriptSerializer().Serialize(new
                        {
                            patient,
                            services
                        });

                        streamWriter_1.Write(json_1);
                    }

                    HttpWebResponse httpResponse_1 = (HttpWebResponse)httpWebRequest_1.GetResponse();

                    if (httpResponse_1.StatusCode == HttpStatusCode.OK)
                    {
                        for (int i = 0; i < sendToCheckArray.Count; i++)
                        {
                            sendToCheckArray[i].код_лаборанта = MainWindow.currentUser.код;
                            sendToCheckArray[i].подробности_заказа_.статус_услуги = 3;
                        }
                        MainWindow.dbEntities.SaveChanges();
                        AnalizatorDataGrid.ItemsSource = работаАнализатора.Where(x => x.подробности_заказа_.статус_услуги_.код == 1 ||
                            x.подробности_заказа_.статус_услуги_.код == 4).ToList();
                        AnalizatorDataGrid.Items.Refresh();
                        await StartTimerLedetect();
                        MessageBox.Show("Услуги успешно отправлены");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка отправки!");
                    }
                }
                else if (analizatorsComboBox.SelectedIndex == 1)
                {
                    var httpWebRequest_2 = (HttpWebRequest)WebRequest.Create($"http://localhost:5000/api/analyzer/{name}");
                    httpWebRequest_2.ContentType = "application/json";
                    httpWebRequest_2.Method = "POST";

                    using (var streamWriter_2 = new StreamWriter(httpWebRequest_2.GetRequestStream()))
                    {

                        string json_2 = new JavaScriptSerializer().Serialize(new
                        {
                            patient,
                            services
                        });

                        streamWriter_2.Write(json_2);
                    }

                    HttpWebResponse httpResponse_2 = (HttpWebResponse)httpWebRequest_2.GetResponse();

                    if (httpResponse_2.StatusCode == HttpStatusCode.OK)
                    {
                        for (int i = 0; i < sendToCheckArray.Count; i++)
                        {
                            sendToCheckArray[i].код_лаборанта = MainWindow.currentUser.код;
                            sendToCheckArray[i].подробности_заказа_.статус_услуги = 3;
                        }
                        MainWindow.dbEntities.SaveChanges();
                        AnalizatorDataGrid.ItemsSource = работаАнализатора.Where(x => x.подробности_заказа_.статус_услуги_.код == 1 ||
                            x.подробности_заказа_.статус_услуги_.код == 4).ToList();
                        AnalizatorDataGrid.Items.Refresh();
                        await StartTimerBiorad();
                        MessageBox.Show("Услуги успешно отправлены");
                    }
                    else
                    {
                        MessageBox.Show("Ошибка отправки!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
