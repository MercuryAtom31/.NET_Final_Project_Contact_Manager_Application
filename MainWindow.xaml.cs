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
            //Show method allows us to display the window.
            //newContactWindow.Show();
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
                // Read all lines from the CSV file
                string[] lines = File.ReadAllLines(filePath);

                // Process each line (assuming a simple CSV structure: Name,Email,Phone)
                foreach (string line in lines.Skip(1)) // Skip header line
                {
                    string[] values = line.Split(',');

                    // Check if the array has enough elements (Name, Email, Phone)
                    if (values.Length >= 3)
                    {
                        // Create a new contact object and populate its properties
                        Contacts newContact = new Contacts
                        {
                            Name = values[0],
                            Email = values[1],
                            Phone = values[2]
                        };

                        // Add the new contact to your contact list or database
                        using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.databasePath))
                        {
                            conn.CreateTable<Contacts>();
                            conn.Insert(newContact);
                        }
                    }
                    else
                    {
                        // Log or display a message about the malformed row
                        MessageBox.Show($"Skipping malformed row: {line}", "Import Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                // Inform the user that the import was successful
                MessageBox.Show("Contacts imported successfully!", "Import Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the UI to display the imported contacts
                ReadDatabase();
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file not found, incorrect format)
                MessageBox.Show($"Error during import: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void ImportContactsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to allow the user to select a CSV file
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV Files (*.csv)|*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Call the method to import contacts from the selected CSV file
                ImportContactsFromCsv(filePath);
            }
        }

        private void ExportContactsToCsv(string filePath)
        {
            try
            {
                // Create a StringBuilder to store the CSV content
                StringBuilder csvContent = new StringBuilder();

                // Add header row
                csvContent.AppendLine("Name,Email,Phone");

                // Add data rows
                foreach (Contacts contact in contacts)
                {
                    csvContent.AppendLine($"{contact.Name},{contact.Email},{contact.Phone}");
                }

                // Write the content to the CSV file
                File.WriteAllText(filePath, csvContent.ToString());

                // Inform the user that the export was successful
                MessageBox.Show("Contacts exported successfully!", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., file not found, permission issues)
                MessageBox.Show($"Error during export: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportContactsButton_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog to allow the user to select a location to save the CSV file
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                // Call the method to export contacts to the selected CSV file
                ExportContactsToCsv(filePath);
            }
        }


    }
}
