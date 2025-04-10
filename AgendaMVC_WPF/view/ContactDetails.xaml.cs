using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AgendaMVC_WPF.agendaDB;
using AgendaMVC_WPF.DAO;
using MahApps.Metro.IconPacks;

namespace AgendaMVC_WPF.view
{
    public partial class ContactDetails : Window
    {
        private Contact _contact;
        private Agenda_DAO _dao;

        public ContactDetails(Contact contact)
        {
            InitializeComponent();
            _dao = new Agenda_DAO();
            _contact = contact;
            LoadContactDetails();
        }

        private void LoadContactDetails()
        {
            // Initialiser les initiales pour l'avatar
            string initiales = "";
            if (!string.IsNullOrEmpty(_contact.Prenom))
                initiales += _contact.Prenom[0];
            if (!string.IsNullOrEmpty(_contact.Nom))
                initiales += _contact.Nom[0];
            InitialesTextBlock.Text = initiales.ToUpper();

            // Remplir les informations de base
            NomCompletTextBlock.Text = $"{_contact.Prenom} {_contact.Nom}";
            EmailTextBlock.Text = _contact.Email;

            // Remplir les détails
            TelephoneTextBlock.Text = string.IsNullOrEmpty(_contact.Telephone) ? "Non renseigné" : _contact.Telephone;
            EmailDetailTextBlock.Text = string.IsNullOrEmpty(_contact.Email) ? "Non renseigné" : _contact.Email;
            AdresseTextBlock.Text = string.IsNullOrEmpty(_contact.Adresse) ? "Non renseignée" : _contact.Adresse;

            // Formater la date de naissance
            if (_contact.DateNaissance.HasValue)
            {
                var dateNaissance = new DateTime(
                    _contact.DateNaissance.Value.Year,
                    _contact.DateNaissance.Value.Month,
                    _contact.DateNaissance.Value.Day);
                DateNaissanceTextBlock.Text = dateNaissance.ToString("dd MMMM yyyy");
            }
            else
            {
                DateNaissanceTextBlock.Text = "Non renseignée";
            }

            // Ajouter les réseaux sociaux
            if (_contact.ReseauxSociaux != null && _contact.ReseauxSociaux.Count > 0)
            {
                ReseauxSociauxBorder.Visibility = Visibility.Visible;

                foreach (var reseau in _contact.ReseauxSociaux)
                {
                    var button = new Button
                    {
                        Style = (Style)FindResource("SocialMediaButton"),
                        ToolTip = $"{reseau.Plateforme}: {reseau.Lien}"
                    };

                    // Définir l'icône en fonction de la plateforme
                    PackIconMaterial icon = new PackIconMaterial();
                    switch (reseau.Plateforme.ToLower())
                    {
                        case "facebook":
                            icon.Kind = PackIconMaterialKind.Facebook;
                            icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3b5998"));
                            break;
                        case "twitter":
                            icon.Kind = PackIconMaterialKind.Twitter;
                            icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1da1f2"));
                            break;
                        case "instagram":
                            icon.Kind = PackIconMaterialKind.Instagram;
                            icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#e1306c"));
                            break;
                        case "linkedin":
                            icon.Kind = PackIconMaterialKind.Linkedin;
                            icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0077b5"));
                            break;
                        default:
                            icon.Kind = PackIconMaterialKind.Web;
                            icon.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C63FF"));
                            break;
                    }

                    icon.Width = 18;
                    icon.Height = 18;
                    button.Content = icon;

                    // Ajouter un événement pour ouvrir le lien
                    button.Click += (sender, e) =>
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = reseau.Lien,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Impossible d'ouvrir le lien: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };

                    ReseauxSociauxPanel.Children.Add(button);
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var fenetre = new AjouterContact(_contact);
            fenetre.ShowDialog();

            if (fenetre.ContactAjoute)
            {
                // Recharger les détails du contact
                _contact = _dao.GetContactById(_contact.Id);

                // Effacer les réseaux sociaux existants
                ReseauxSociauxPanel.Children.Clear();

                // Recharger les détails
                LoadContactDetails();
            }
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Ajout du gestionnaire d'événements pour permettre de déplacer la fenêtre
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}

