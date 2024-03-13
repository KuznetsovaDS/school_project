using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using System.Windows.Shapes;

namespace WpfApp1
{
     public partial class LoginWindow : Window
    {
        public string Server { get; set; }
        public string Catalog { get; set; }
        public string Username { get; set; }
        public bool IsValid { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            
        }
          private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            Server = tbServer.Text;
            Catalog = tbDatabase.Text;
            Username = tbUsername.Text;
            if (!string.IsNullOrEmpty(Server) && !string.IsNullOrEmpty(Catalog) && !string.IsNullOrEmpty(Username))
            {
                IsValid = true;
                this.Close();
            }
            else
            {
                IsValid = false;
                MessageBox.Show("Пожалуйста, введите данные для подключения.");
            }
           this.Close();
        }
    }
}