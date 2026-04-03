using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using ProizvPR.DBConn;

namespace ProizvPR.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
       public RegisterPage()
        {
            InitializeComponent();
        }
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtLogin.Text) || string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("ТЫ ВСЕ ПОЛЯ НЕ МОЖЕШЬ ЧТО-ЛИ ЗАПОЛНИТЬ!!!!");
                return;
            }
           if (txtPassword.Password.Length < 3)
            {
                MessageBox.Show("ПАРОЛЬ ДОЛЖЕН БЫТЬ МИНИМУМ 3 СИМВОЛА");
                return;
            }
            if (txtPassword.Password != txtConfirmPassword.Password)
            {
                MessageBox.Show("ПАРОЛИ НЕ СОВПАДАЮТ");
                return;
            }
            var loginExists = Conn.db.Logins.FirstOrDefault(l => l.login == txtLogin.Text);
            if (loginExists != null)
            {
                MessageBox.Show("ЛОГИН ЗАНЯТ");
                return;
            }
            try
            {
                var newLogin = new Logins();
                newLogin.login = txtLogin.Text;
                newLogin.password = txtPassword.Password;
                Conn.db.Logins.Add(newLogin);
                Conn.db.SaveChanges();
                var newUser = new Users();
                newUser.full_name = txtFullName.Text;
                newUser.email = txtEmail.Text;
                newUser.phone = txtPhone.Text;
                newUser.id_role = 3;
                newUser.id_login = newLogin.id_login;
                Conn.db.Users.Add(newUser);
                Conn.db.SaveChanges();
                MessageBox.Show("РЕГИСТРАЦИЯ УСПЕШНА!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ОШИБКА: " + ex.Message);
            }
        }
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}