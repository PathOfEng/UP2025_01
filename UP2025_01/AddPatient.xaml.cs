using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace UP2025_01
{
    /// <summary>
    /// Логика взаимодействия для AddPatient.xaml
    /// </summary>
    public partial class AddPatient : Window
    {
        List<страховая_компания_> Companies = MainWindow.dbEntities.страховая_компания_.ToList();
        List<тип_полиса_> PolicyTypes = MainWindow.dbEntities.тип_полиса_.ToList();
        /// <summary>
        /// Отдельное окно для добавления пациента
        /// </summary>
        public AddPatient()
        {
            InitializeComponent();
            policyType.ItemsSource = PolicyTypes;
            companies.ItemsSource = Companies;
        }

        /// <summary>
        /// Добавление пациента, метод проверяет заполненность всех полей формы, а также совершает валидацию номера телефона, почты, пароля, логина и паспорта(серия и номер)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddPatienToDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FIO.Text == "")
                {
                    throw new Exception("ФИО не заполнено");
                }
                else if (login.Text == "")
                {
                    throw new Exception("Логин не заполнен");
                }
                else if (password.Text == "")
                {
                    throw new Exception("Пароль не заполнен");
                }
                else if (dateBorn.Text == "")
                {
                    throw new Exception("Дата рождения не заполнена");
                }
                else if (passport.Text == "")
                {
                    throw new Exception("Серия и номер паспорта не заполнены");
                }
                else if (telephoneNumber.Text == "")
                {
                    throw new Exception("Телефон не заполнен");
                }
                else if (companies.SelectedIndex == -1)
                {
                    throw new Exception("Не выбрана страховая компания");
                }
                else if (policyType.SelectedIndex == -1)
                {
                    throw new Exception("Не выбран тип страхового полиса");
                }
                else if (policyNumber.Text == "")
                {
                    throw new Exception("Номер полиса не заполнен");
                }
                else
                {
                    пользователи_ пациент = new пользователи_();

                    if (Mail_LIB.Validation.Check_login(login.Text))
                    {
                        пациент.логин = login.Text;
                    }
                    else
                    {
                        throw new Exception("Введенный логин имеет неверный формат.");
                    }
                    if (Mail_LIB.Validation.Check_password(password.Text))
                    {
                        пациент.пароль = password.Text;
                    }
                    else
                    {
                        throw new Exception("Пароль имеет неверный формат.");
                    }

                    if (Mail_LIB.Validation.Check_mail(email.Text))
                    {
                        пациент.электронная_почта = email.Text;
                    }
                    else
                    {
                        throw new Exception("Введенная почта имеет неверный формат");
                    }
                    пациент.код_роли = 4;
                    пациент.наименование = FIO.Text;
                    пациент.дата_рождения = Convert.ToDateTime(dateBorn.Text);
                    Regex checkPassport = new Regex(@"^[0-9]{10}$");
                    if (checkPassport.IsMatch(passport.Text))
                    {
                        if (Convert.ToInt32(passport.Text) < 1000100000)
                        {
                            throw new Exception("Паспорт не может содерждать столько нулей!!!!");
                        }
                        пациент.паспорт = passport.Text;
                    }
                    else
                    {
                        throw new Exception("Паспорт имеет неверный формат\n" +
                            "Первые четыре цифры - номер, вторые 6 цифр - серия");
                    }
                    Regex checkNumber = new Regex(@"^[1-9]{1}[0-9]{10}$");
                    if (checkNumber.IsMatch(telephoneNumber.Text))
                    {
                        пациент.телефон = telephoneNumber.Text;
                    }
                    else
                    {
                        throw new Exception("Телефон имеет неверный формат");
                    }
                    пациент.номер_полиса = policyNumber.Text;
                    пациент.код_страховой = companies.SelectedIndex + 1;
                    пациент.тип_страхового_полиса = policyType.SelectedIndex + 1;
                    Regex regex = new Regex(@"^[0-9]+$");
                    if (regex.IsMatch(policyNumber.Text))
                    {
                        пациент.номер_полиса = policyNumber.Text;
                    }
                    else
                    {
                        throw new Exception("В номере полиса не может быть букв");
                    }
                    MainWindow.dbEntities.пользователи_.Add(пациент);
                    MainWindow.dbEntities.SaveChanges();
                    MessageBox.Show("Пациент успешно добавлен в систему");
                    Close();
                }
            }
            catch (Exception ex)
            {
                if (companies.SelectedIndex == -1 || policyType.SelectedIndex == -1)
                {

                }
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
