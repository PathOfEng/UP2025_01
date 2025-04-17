using System;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Windows;

namespace UP2025_01
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static laboratoryEntities dbEntities = new laboratoryEntities();
        private bool isEntryFalse = false;
        private string captcha_text;
        private string[] alphabet =
        {
            "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я",
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9"
        };
        private bool isPasswordHide = true;
        public static пользователи_ currentUser;
        public MainWindow()
        {
            InitializeComponent();
            HideCapthca();
            GenerateCaptcha();
            passwordTextBox.Visibility = Visibility.Hidden;
        }
        private void HideCapthca()
        {
            c1.Visibility = Visibility.Hidden;
            c2.Visibility = Visibility.Hidden;
            c3.Visibility = Visibility.Hidden;
            c4.Visibility = Visibility.Hidden;
            captcha.Visibility = Visibility.Hidden;
            capthcaLabel.Visibility = Visibility.Hidden;
            captchaText.Visibility = Visibility.Hidden;
            FillCaptcha.Visibility = Visibility.Hidden;
            radio.Visibility = Visibility.Hidden;
        }
        private void ShowCapthca()
        {
            c1.Visibility = Visibility.Visible;
            c2.Visibility = Visibility.Visible;
            c3.Visibility = Visibility.Visible;
            c4.Visibility = Visibility.Visible;
            captcha.Visibility = Visibility.Visible;
            capthcaLabel.Visibility = Visibility.Visible;
            captchaText.Visibility = Visibility.Visible;
            FillCaptcha.Visibility = Visibility.Visible;
            isEntryFalse = true;
            radio.Visibility = Visibility.Visible;
        }


        /// <summary>
        /// генерация капчи
        /// </summary>
        private void GenerateCaptcha()
        {
            Random rand = new Random();
            c1.Content = alphabet[rand.Next(0,alphabet.Length)];
            c2.Content = alphabet[rand.Next(0, alphabet.Length)];
            c3.Content = alphabet[rand.Next(0, alphabet.Length)];
            c4.Content = alphabet[rand.Next(0, alphabet.Length)];
            captcha_text = c1.Content.ToString() + c2.Content.ToString() + c3.Content.ToString() + c4.Content.ToString();
        }


        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString(); // Вернёт локальный IP
                }
            }
            return "0.0.0.0";
        }

        private void StartTimer()
        {
            input.IsEnabled = false;
            var timer = new System.Timers.Timer(6000);
            timer.Elapsed += (sender,e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    input.IsEnabled = true;
                });
                timer.Stop();
            };
            timer.Start();
        }

        private void input_Click(object sender, RoutedEventArgs e)
        {
            var userAcc = dbEntities.пользователи_.Where(x => x.логин == login.Text).FirstOrDefault();
            история_входа_ история_Входа_ = new история_входа_();
            if (userAcc!=null)
            {
                история_Входа_.ip = GetLocalIpAddress();
                история_Входа_.дата = DateTime.Now;
                история_Входа_.код_пользователя = userAcc.код;
                if (userAcc.пароль == passwordBox.Password)
                {
                    if (!isEntryFalse || (isEntryFalse && captchaText.Text == captcha_text))
                    {
                        //история_Входа_.статус_входа = 1;
                        //dbEntities.история_входа_.Add(история_Входа_);
                        //dbEntities.SaveChanges();
                        if (userAcc.код_роли == 1 || userAcc.код_роли == 5)
                        {
                            var time = dbEntities.история_входа_.Where(x => x.статус_входа == 4).ToList().LastOrDefault();
                            if (time != null)
                            {
                                TimeSpan timeRemaining = (TimeSpan)(DateTime.Now - time.дата);

                                if (timeRemaining.TotalMinutes < 2)
                                {
                                    MessageBox.Show("Ошибка. Кварцевание еще не окончено. Должно пройти 30 минут\n" +
                                        "Прошло " + timeRemaining.TotalMinutes.ToString("00") + " минут");
                                    return;
                                }
                            }
                            
                        }
                        currentUser = userAcc;
                        Cabinet cabinet = new Cabinet();
                        cabinet.Show();
                        Close();
                    }
                    else 
                    {
                        история_Входа_.статус_входа = 2;
                        dbEntities.история_входа_.Add(история_Входа_);
                        dbEntities.SaveChanges();
                        MessageBox.Show("Капча не пройдена, подождите 10 секунд");
                        StartTimer();
                    }
                }
                else
                {
                    if (isEntryFalse && (captchaText.Text != captcha_text))
                    {
                        MessageBox.Show("Капча не пройдена, подождите 10 секунд");
                        StartTimer();
                    }
                    else
                    {
                        MessageBox.Show("Пароль неверный");
                    }
                    история_Входа_.статус_входа = 2;
                    dbEntities.история_входа_.Add(история_Входа_);
                    dbEntities.SaveChanges();
                    ShowCapthca();
                }
            }
            else
            {

            }
        }

        private void FillCaptcha_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }

        private void showPassword_Click(object sender, RoutedEventArgs e)
        {
            if (isPasswordHide)
            {
                passwordTextBox.Text = passwordBox.Password;
                passwordTextBox.Visibility = Visibility.Visible;
                passwordBox.Visibility = Visibility.Hidden;
                isPasswordHide = false;
                showPassword.Content = "Скрыть";
            }
            else
            {
                passwordTextBox.Visibility = Visibility.Hidden;
                passwordBox.Visibility = Visibility.Visible;
                isPasswordHide = true;
                showPassword.Content = "Показать";
            }
        }

        private void passwordTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            passwordBox.Password = passwordTextBox.Text;
        }
    }
}
