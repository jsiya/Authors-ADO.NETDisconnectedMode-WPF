using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Authors_ADO.NETDisconnectedMode_WPF
{

    public partial class MainWindow : Window
    {
        DataTable? table = null;
        SqlConnection? connection = null;
        SqlDataAdapter? adapter = null;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            string connectionString = "Data Source=DESKTOP-UCO0K2D;Initial Catalog=Library;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            connection = new SqlConnection(connectionString);
        }

        public void ReadDataFromTable(string selectQuery = "SELECT * FROM Authors")
        {
            try
            {
                table = new DataTable();

                adapter = new SqlDataAdapter(selectQuery, connection);

                adapter.Fill(table);

                AuthorsDataGridView.DataContext = table;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        public void InsertData() //raw sql
        {
            try
            {
                if (AuthorsDataGridView.SelectedItem != null)
                {
                    DataRowView selectedRow = (DataRowView)AuthorsDataGridView.SelectedItem;

                    int id = Convert.ToInt32(selectedRow["Id"]);
                    string newAuthorFirstName = selectedRow["FirstName"].ToString();
                    string newAuthorLastName = selectedRow["LastName"].ToString();

                    string insertQuery = "INSERT INTO Authors (Id, FirstName, LastName) " +
                                         "VALUES (@Id, @FirstName, @LastName)";

                    using SqlCommand sqlCommand = new(insertQuery, connection);
                    sqlCommand.Parameters.AddWithValue("@Id", id);
                    sqlCommand.Parameters.AddWithValue("@FirstName", newAuthorFirstName);
                    sqlCommand.Parameters.AddWithValue("@LastName", newAuthorLastName);

                    adapter.InsertCommand = sqlCommand;
                    adapter.Update(table);
                }
                else
                {
                    MessageBox.Show("select a row to insert.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        public void UpdateTable() //commandbuilder 
        {
            try
            {
                if (AuthorsDataGridView.SelectedItem != null)
                {
                    DataRowView selectedRow = (DataRowView)AuthorsDataGridView.SelectedItem;
                    int authorId = Convert.ToInt32(selectedRow["Id"]);

                    DataRow[] rowsToUpdate = table.Select($"Id = {authorId}");
                    if (rowsToUpdate.Length > 0)
                    {
                        DataRow rowToUpdate = rowsToUpdate[0];
                        rowToUpdate["FirstName"] = selectedRow["FirstName"];
                        rowToUpdate["LastName"] = selectedRow["LastName"];

                        using (SqlCommandBuilder sqlCommandBuilder = new SqlCommandBuilder(adapter))
                        {
                            adapter.Update(table);
                        }
                        table.AcceptChanges();
                    }
                    else
                    {
                        MessageBox.Show("Author not found in the DataTable.");
                    }
                }
                else
                {
                    MessageBox.Show("Select a row to update.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void DeleteFromTable(int authorId) //stored procedure
        {
            //delete command
            SqlCommand deleteCommand = new SqlCommand("DeleteAuthor", connection);
            deleteCommand.CommandType = CommandType.StoredProcedure;

            deleteCommand.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int, 0, "Id"));

            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.DeleteCommand = deleteCommand;

            DataRow[] rowsToDelete = table?.Select($"Id = {authorId}"); 
            foreach (DataRow row in rowsToDelete)
            {
                row.Delete();
            }

            adapter.Update(table);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ReadDataFromTable();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)AuthorsDataGridView.SelectedItem;

            int authorId = Convert.ToInt32(selectedRow["Id"]);

            DeleteFromTable(authorId);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateTable();
        }

        private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SearchBar.Text.Length > 0)
            {
                ReadDataFromTable($"SELECT * FROM Authors WHERE FirstName LIKE '%{SearchBar.Text}%'");
            }
            else
                ReadDataFromTable();
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            InsertData();
        }
    }
}
