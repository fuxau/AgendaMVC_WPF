using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AgendaMVC_WPF.agendaDB;
using AgendaMVC_WPF.DAO;

namespace AgendaMVC_WPF.view
{
    public partial class Contactes : UserControl
    {
        private Agenda_DAO _dao;

        public Contactes()
        {
            InitializeComponent();
            _dao = new Agenda_DAO();
            RefreshContacts();

            // Ajouter l'événement de recherche
            SearchBox.TextChanged += SearchBox_TextChanged;

            // Ajouter l'événement de double-clic pour ouvrir les détails du contact
            membersDataGrid.MouseLeftButtonUp += MembersDataGrid_MouseDoubleClick;
        }

        private void MembersDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (membersDataGrid.SelectedItem is Contact contact)
            {
                // Charger le contact avec ses réseaux sociaux
                var contactWithDetails = _dao.GetContactById(contact.Id);
                var detailsWindow = new ContactDetails(contactWithDetails);
                detailsWindow.ShowDialog();

                // Rafraîchir la liste après fermeture de la fenêtre de détails
                RefreshContacts();
            }
        }

        public void RefreshContacts()
        {
            var contacts = _dao.GetAllContacts();
            membersDataGrid.ItemsSource = contacts;

            if (contacts != null && contacts.Count > 0)
                DernierIdTextBlock.Text = $"{contacts.Count} contacts";
            else
                DernierIdTextBlock.Text = "Aucun contact";
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchTerm = SearchBox.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                RefreshContacts();
            }
            else
            {
                var filteredContacts = _dao.SearchContacts(searchTerm);
                membersDataGrid.ItemsSource = filteredContacts;

                if (filteredContacts != null && filteredContacts.Count > 0)
                    DernierIdTextBlock.Text = $"{filteredContacts.Count} contacts trouvés";
                else
                    DernierIdTextBlock.Text = "Aucun contact trouvé";
            }
        }

        private void ADD_BTN_Click(object sender, RoutedEventArgs e)
        {
            var fenetre = new AjouterContact();
            fenetre.ShowDialog();

            if (fenetre.ContactAjoute)
            {
                RefreshContacts();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Contact contact)
            {
                // Charger le contact avec ses réseaux sociaux
                var contactWithDetails = _dao.GetContactById(contact.Id);
                var fenetre = new AjouterContact(contactWithDetails);
                fenetre.ShowDialog();

                if (fenetre.ContactAjoute)
                {
                    RefreshContacts();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Contact contact)
            {
                var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer le contact {contact.Prenom} {contact.Nom} ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _dao.DeleteContact(contact.Id);
                    if (success)
                    {
                        RefreshContacts();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la suppression du contact", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}

