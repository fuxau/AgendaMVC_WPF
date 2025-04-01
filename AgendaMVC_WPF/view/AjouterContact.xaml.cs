using System;
using System.Windows;
using AgendaMVC_WPF.agendaDB;
using AgendaMVC_WPF.DAO;

namespace AgendaMVC_WPF.view
{
    public partial class AjouterContact : Window
    {
        private Contactes _parent;
        public bool ContactAjoute { get; private set; } = false;

        public AjouterContact()
        {
            InitializeComponent();
            _parent = parent;
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            var nouveauContact = new Contact
            {
                Nom = NomBox.Text,
                Prenom = PrenomBox.Text,
                Email = EmailBox.Text,
                Telephone = TelBox.Text,
                DateCreation = DateTime.Now
            };

            string result = new Agenda_DAO().AddContact(nouveauContact);
            MessageBox.Show(result);

            // Rafraîchir le UserControl parent
            _parent.RefreshContacts();

            this.Close(); ;
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
