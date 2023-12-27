using System;
using System.Collections.Generic;
using System.IO;
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
using ContactManagerApp1.Classes;
using Microsoft.Win32;
using SQLite;

namespace ContactManagerApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Contacts> contacts;

        public MainWindow()
        {
            InitializeComponent();

            contacts = new List<Contacts>();

            ReadDatabase();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            NewContactWindow newContactWindow = new NewContactWindow();
            
            newContactWindow.ShowDialog();

            ReadDatabase();
        }

        void ReadDatabase()
        {

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.databasePath))
            {
                conn.CreateTable<Contacts>();
                contacts = (conn.Table<Contacts>().ToList()).OrderBy(c => c.Name).ToList();
            }

            if (contacts != null)
            {
                contactsListView.ItemsSource = contacts;
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox searchTextBox = sender as TextBox;

            var filteredList = contacts.Where(c => c.Name.ToLower().Contains(searchTextBox.Text.ToLower())).ToList();

            contactsListView.ItemsSource = filteredList;
        }

        private void ContactsListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Contacts selectedContact = (Contacts)contactsListView.SelectedItem;

            if (selectedContact != null)
            {
                ContactDetailsWindow contactDetailsWindow = new ContactDetailsWindow(selectedContact);
                contactDetailsWindow.ShowDialog();

                ReadDatabase();
            }
        }

        private void ImportContactsFromCsv(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines.Skip(1))
                {
                    string[] values = line.Split(',');

                    if (values.Length >= 3)
                    {
                        Contacts newContact = new Contacts
                        {
                            Name = values[0],
                            Email = values[1],
                            Phone = values[2]
                        };

                        using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.databasePath))
                        {
                            conn.CreateTable<Contacts>();
                            conn.Insert(newContact);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Skipping malformed row: {line}", "Import Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                MessageBox.Show("Contacts imported successfully!", "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                ReadDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during import: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ImportContactsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                ImportContactsFromCsv(filePath);
            }
        }

        private void ExportContactsToCsv(string filePath)
        {
            try
            {
                StringBuilder csvContent = new StringBuilder();

                csvContent.AppendLine("Name,Email,Phone");

                foreach (Contacts contact in contacts)
                {
                    csvContent.AppendLine($"{contact.Name},{contact.Email},{contact.Phone}");
                }

                File.WriteAllText(filePath, csvContent.ToString());

                MessageBox.Show("Contacts exported successfully!", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during export: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportContactsButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                ExportContactsToCsv(filePath);
            }
        }


    }
}
