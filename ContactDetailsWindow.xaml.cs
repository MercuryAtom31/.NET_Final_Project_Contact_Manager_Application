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
using System.Windows.Shapes;
using ContactManagerApp1.Classes;
using SQLite;

namespace ContactManagerApp1
{
    /// <summary>
    /// Interaction logic for ContactDetailsWindow.xaml
    /// </summary>
    public partial class ContactDetailsWindow : Window
    {
        private Contacts contacts;

        public ContactDetailsWindow(Contacts contact)
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            this.contacts = contact;
            nameTextBox.Text = contact.Name;
            phoneTextBox.Text = contact.Phone;
            emailTextBox.Text = contact.Email;
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            contacts.Name = nameTextBox.Text;
            contacts.Phone = phoneTextBox.Text;
            contacts.Email = emailTextBox.Text;

            using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
            {
                connection.CreateTable<Contacts>();
                connection.Update(contacts);
            }

            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmationDialog = new ConfirmationMessageBox();
            confirmationDialog.Owner = this;

            // Set dialog position
            confirmationDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            confirmationDialog.ShowDialog();

            if (confirmationDialog.Result)
            {
                using (SQLiteConnection connection = new SQLiteConnection(App.databasePath))
                {
                    connection.CreateTable<Contacts>();
                    connection.Delete(contacts);
                }

                Close();
            }
            // If the user clicks "No" or closes the dialog, do nothing.
        }
    }
}
