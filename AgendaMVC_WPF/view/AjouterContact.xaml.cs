using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AgendaMVC_WPF.agendaDB;
using AgendaMVC_WPF.DAO;

namespace AgendaMVC_WPF.view
{
    public partial class AjouterContact : Window
    {
        public bool ContactAjoute { get; private set; } = false;
        private Contact _contactToEdit;
        private ObservableCollection<ReseauxSociaux> _reseauxSociaux = new ObservableCollection<ReseauxSociaux>();

        public AjouterContact(Contact contactToEdit = null)
        {
            InitializeComponent();

            // Initialiser la liste des plateformes de réseaux sociaux
            PlateformeComboBox.Items.Add("Facebook");
            PlateformeComboBox.Items.Add("Twitter");
            PlateformeComboBox.Items.Add("Instagram");
            PlateformeComboBox.Items.Add("LinkedIn");
            PlateformeComboBox.Items.Add("Autre");
            PlateformeComboBox.SelectedIndex = 0;

            // Lier la liste des réseaux sociaux au DataGrid
            ReseauxSociauxDataGrid.ItemsSource = _reseauxSociaux;

            // Si on est en mode édition
            if (contactToEdit != null)
            {
                _contactToEdit = contactToEdit;
                WindowTitle.Text = "Modifier un contact";

                // Remplir les champs avec les données du contact
                NomBox.Text = contactToEdit.Nom;
                PrenomBox.Text = contactToEdit.Prenom;
                EmailBox.Text = contactToEdit.Email;
                TelBox.Text = contactToEdit.Telephone;

                // Si vous avez ajouté ces champs à votre formulaire
                if (AdresseBox != null && contactToEdit.Adresse != null)
                    AdresseBox.Text = contactToEdit.Adresse;

                if (DateNaissanceBox != null && contactToEdit.DateNaissance.HasValue)
                {
                    // Convertir DateOnly en DateTime pour le DatePicker
                    var dateNaissance = new DateTime(
                        contactToEdit.DateNaissance.Value.Year,
                        contactToEdit.DateNaissance.Value.Month,
                        contactToEdit.DateNaissance.Value.Day);
                    DateNaissanceBox.SelectedDate = dateNaissance;
                }

                // Charger les réseaux sociaux du contact
                if (contactToEdit.ReseauxSociaux != null)
                {
                    foreach (var rs in contactToEdit.ReseauxSociaux)
                    {
                        _reseauxSociaux.Add(rs);
                    }
                }
            }
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            // Vérification des champs obligatoires
            if (string.IsNullOrWhiteSpace(NomBox.Text) ||
                string.IsNullOrWhiteSpace(PrenomBox.Text))
            {
                MessageBox.Show("Veuillez remplir au moins le nom et le prénom", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dao = new Agenda_DAO();
            bool success;

            // Convertir DateTime en DateOnly pour la base de données
            DateOnly? dateNaissance = null;
            if (DateNaissanceBox.SelectedDate.HasValue)
            {
                dateNaissance = new DateOnly(
                    DateNaissanceBox.SelectedDate.Value.Year,
                    DateNaissanceBox.SelectedDate.Value.Month,
                    DateNaissanceBox.SelectedDate.Value.Day);
            }

            // Création ou mise à jour du contact
            if (_contactToEdit == null)
            {
                // Nouveau contact
                var contact = new Contact
                {
                    Nom = NomBox.Text,
                    Prenom = PrenomBox.Text,
                    Email = EmailBox.Text,
                    Telephone = TelBox.Text,
                    DateNaissance = dateNaissance,
                    Adresse = AdresseBox.Text,
                    DateCreation = DateTime.Now
                };

                success = dao.AddContact(contact);

                if (success)
                {
                    // Récupérer l'ID du contact nouvellement créé
                    var newContact = dao.GetContactByEmail(EmailBox.Text);
                    if (newContact != null)
                    {
                        // Ajouter les réseaux sociaux
                        foreach (var rs in _reseauxSociaux)
                        {
                            rs.ContactId = newContact.Id;
                            dao.AddReseauSocial(rs);
                        }
                    }
                }
            }
            else
            {
                // Mise à jour d'un contact existant
                _contactToEdit.Nom = NomBox.Text;
                _contactToEdit.Prenom = PrenomBox.Text;
                _contactToEdit.Email = EmailBox.Text;
                _contactToEdit.Telephone = TelBox.Text;
                _contactToEdit.DateNaissance = dateNaissance;
                _contactToEdit.Adresse = AdresseBox.Text;

                success = dao.UpdateContact(_contactToEdit);

                if (success)
                {
                    // Supprimer les anciens réseaux sociaux
                    var existingReseauxSociaux = dao.GetReseauxSociauxByContactId(_contactToEdit.Id);
                    foreach (var rs in existingReseauxSociaux)
                    {
                        dao.DeleteReseauSocial(rs.Id);
                    }

                    // Ajouter les nouveaux réseaux sociaux
                    foreach (var rs in _reseauxSociaux)
                    {
                        rs.ContactId = _contactToEdit.Id;
                        dao.AddReseauSocial(rs);
                    }
                }
            }

            if (success)
            {
                ContactAjoute = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Erreur lors de l'enregistrement du contact", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AjouterReseauSocial_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LienTextBox.Text))
            {
                MessageBox.Show("Veuillez saisir un lien pour le réseau social", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var reseauSocial = new ReseauxSociaux
            {
                Plateforme = PlateformeComboBox.SelectedItem.ToString(),
                Lien = LienTextBox.Text
            };

            _reseauxSociaux.Add(reseauSocial);

            // Réinitialiser les champs
            LienTextBox.Text = string.Empty;
            PlateformeComboBox.SelectedIndex = 0;
        }

        private void SupprimerReseauSocial_Click(object sender, RoutedEventArgs e)
        {
            if (ReseauxSociauxDataGrid.SelectedItem is ReseauxSociaux reseauSocial)
            {
                _reseauxSociaux.Remove(reseauSocial);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un réseau social à supprimer", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

