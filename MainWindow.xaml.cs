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
using System.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private SqlConnection sqlConnection;
        private ObservableCollection<string> subjectsCollection = new ObservableCollection<string>();
        public MainWindow()
        {
            InitializeComponent();
        }
        private SqlConnection CreateConnection(string connectionString)
        {
            try
            {
                sqlConnection = new SqlConnection(connectionString);
                sqlConnection.Open();

                if (sqlConnection.State == ConnectionState.Open)
                {
                    MessageBox.Show("Подключение к базе данных успешно установлено.");
                    return sqlConnection;
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к базе данных.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при подключении к базе данных: " + ex.Message);
                return null;
            }
        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.ShowDialog();

            if (loginWindow.IsValid)
            {
                string server = loginWindow.Server;
                string catalog = loginWindow.Catalog;
                string username = loginWindow.Username;
                string connectionString = $@"Data Source={server};Initial Catalog={catalog};Integrated Security=True;User ID={username};";
                sqlConnection = CreateConnection(connectionString);
               if (sqlConnection != null)
                {
                LoadSubjects(sqlConnection);
                }
                else
                {
                    MessageBox.Show("Не удалось установить подключение к базе данных.");
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите корректные данные для подключения.");
            }
        }
        private void btnStudentInform_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string firstName = FirstName.Text;
                string lastName = LastName.Text;
                string groupNum = GroupNum.Text;

                string query = "SELECT Email, ContactNumber FROM students WHERE FirstName = @FirstName AND LastName = @LastName AND GroupNumber = @GroupNumber";

                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@GroupNumber", groupNum);

                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    string email = dataTable.Rows[0]["Email"].ToString();
                    string contactNumber = dataTable.Rows[0]["ContactNumber"].ToString();

                    StudentInfo.Text = $"Email: {email}\nContact Number: {contactNumber}";
                }
                else
                {
                    StudentInfo.Text = "Студент не найден.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void LoadSubjects(SqlConnection connection)
        {
            try
            {
                string query = "SELECT SubjectName FROM subjects";
                SqlCommand command = new SqlCommand(query, connection);
                command.Connection = connection;
                 SqlDataReader reader = command.ExecuteReader();
                 while (reader.Read())
                {
                    string subjectName = reader["SubjectName"].ToString();
                    subjectsCollection.Add(subjectName);
                }
                 reader.Close();
                SubjectComboBox.ItemsSource = subjectsCollection;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnAddMark_Click(object sender, RoutedEventArgs e)
        {
            DateTime currentDate = DateTime.Today;
            string formattedDate = currentDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
            string firstName = FirstName.Text;
            string lastName = LastName.Text;
            string groupNum = GroupNum.Text;
            string selectedSubject = SubjectComboBox.SelectedItem.ToString();
            int marks = int.Parse(markInput.Text);
            try
            {
                int studentID = GetStudentID(firstName, lastName, groupNum);
                int subjectID = GetSubjectID(selectedSubject);

                if (studentID != -1 && subjectID != -1)
                {
                    string query = "INSERT INTO marks (StudentID, SubjectID, Marks, Date) VALUES (@StudentID, @SubjectID, @Marks, @Date)";
                    SqlCommand command = new SqlCommand(query, sqlConnection);
                    command.Parameters.AddWithValue("@StudentID", studentID);
                    command.Parameters.AddWithValue("@SubjectID", subjectID);
                    command.Parameters.AddWithValue("@Marks", marks);
                    command.Parameters.AddWithValue("@Date", currentDate);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Оценка успешно добавлена.");
                    }
                    else
                    {
                        MessageBox.Show("Не удалось добавить оценку.");
                    }
                }
                else
                {
                    MessageBox.Show("Студент или предмет не найден.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private int GetSubjectID(string subjectName)
        {
            int subjectID = -1;
            try
            {
                string query = "SELECT SubjectID FROM subjects WHERE SubjectName = @SubjectName";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@SubjectName", subjectName);
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    subjectID = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return subjectID;
        }
        private int GetStudentID(string firstName, string lastName, string groupNum)
        {
            int studentID = -1;

            try
            {
                string query = @"SELECT StudentID FROM students WHERE FirstName = @FirstName AND LastName = @LastName AND GroupNumber = @GroupNumber";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);
                command.Parameters.AddWithValue("@GroupNumber", groupNum);
                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    studentID = Convert.ToInt32(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return studentID;
        }
        private void StudentMarksBySubject(int studentID, int subjectID)
        {
            try
            {
                string query = "EXEC GetStudentMarksBySubject @StudentID, @SubjectID";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@StudentID", studentID);
                command.Parameters.AddWithValue("@SubjectID", subjectID);

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int marks = Convert.ToInt32(reader["Marks"]);
                    DateTime date = Convert.ToDateTime(reader["Date"]);

                    richTextBox.AppendText($"Оценка: {marks}, Дата: {date}\n\n");
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }
        private void btnMarks_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Document.Blocks.Clear();
            try
            {
                int studentID = GetStudentID(FirstName.Text, LastName.Text, GroupNum.Text);
                string selectedSubject = SubjectComboBox.SelectedItem.ToString();
                int subjectID = GetSubjectID(selectedSubject);

                StudentMarksBySubject(studentID, subjectID);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            string newGroupNumber = newGroupNum.Text;
            string firstName = FirstName.Text;
            string lastName = LastName.Text;
            UpdateStudentGroupNumber(firstName, lastName, newGroupNumber);
        }
        private void UpdateStudentGroupNumber(string firstName, string lastName, string newGroupNumber)
        {
            try
            {
                string query = "UPDATE students SET GroupNumber = @NewGroupNumber WHERE FirstName = @FirstName AND LastName = @LastName";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                command.Parameters.AddWithValue("@NewGroupNumber", newGroupNumber);
                command.Parameters.AddWithValue("@FirstName", firstName);
                command.Parameters.AddWithValue("@LastName", lastName);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    GroupNum.Text = newGroupNumber;
                    MessageBox.Show("Номер группы успешно изменен.");
                }
                else
                {
                    MessageBox.Show("Не удалось изменить номер группы.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
         public void LoadTeacherData()
        {
            try
            {
                string query = "SELECT FirstName, LastName, Email, ContactNumber FROM teachers";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                 if (dataTable.Rows.Count > 0)
                {
                    dataGrid.ItemsSource = dataTable.DefaultView;
                }
                else
                {
                    MessageBox.Show("Нет данных для отображения.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
         private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            sqlConnection.Close();
            Application.Current.Shutdown();
        }
        private void btnDisplayData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string query = "SELECT FirstName, LastName, Email, ContactNumber FROM teachers";
                SqlCommand command = new SqlCommand(query, sqlConnection);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                DataTable dataTable = new DataTable();
                dataAdapter.Fill(dataTable);
                if (dataTable.Rows.Count > 0)
                {
                    dataGrid.ItemsSource = dataTable.DefaultView;
                }
                else
                {
                    MessageBox.Show("Нет данных для отображения.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }     
    }
}


    