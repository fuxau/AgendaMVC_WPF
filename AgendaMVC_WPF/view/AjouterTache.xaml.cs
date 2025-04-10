using System;
using System.Windows;
using System.Windows.Controls;
using AgendaMVC_WPF.agendaDB;
using AgendaMVC_WPF.DAO;

namespace AgendaMVC_WPF.view
{
    public partial class AjouterTache : Window
    {
        public bool TacheAjoutee { get; private set; } = false;
        private Tach _tacheToEdit;
        private Agenda_DAO _dao;

        public AjouterTache(Tach tacheToEdit = null)
        {
            InitializeComponent();
            _dao = new Agenda_DAO();

            // Charger les catégories
            var categories = _dao.GetAllCategories();
            foreach (var categorie in categories)
            {
                CategorieBox.Items.Add(new ComboBoxItem { Content = categorie.Nom, Tag = categorie });
            }

            // Sélectionner la première catégorie par défaut
            if (CategorieBox.Items.Count > 0)
            {
                CategorieBox.SelectedIndex = 0;
            }

            // Sélectionner "À faire" par défaut
            StatutBox.SelectedIndex = 0;

            // Si on est en mode édition
            if (tacheToEdit != null)
            {
                _tacheToEdit = tacheToEdit;
                WindowTitle.Text = "Modifier une tâche";

                // Remplir les champs avec les données de la tâche
                TitreBox.Text = tacheToEdit.Titre;
                DescriptionBox.Text = tacheToEdit.Description;

                // Convertir DateOnly en DateTime pour le DatePicker
                if (tacheToEdit.DateEcheance.HasValue)
                {
                    var dateEcheance = new DateTime(
                        tacheToEdit.DateEcheance.Value.Year,
                        tacheToEdit.DateEcheance.Value.Month,
                        tacheToEdit.DateEcheance.Value.Day);
                    DateEcheanceBox.SelectedDate = dateEcheance;
                }

                // Sélectionner la catégorie
                if (tacheToEdit.CategorieId.HasValue)
                {
                    foreach (ComboBoxItem item in CategorieBox.Items)
                    {
                        if (item.Tag is Category categorie && categorie.Id == tacheToEdit.CategorieId.Value)
                        {
                            CategorieBox.SelectedItem = item;
                            break;
                        }
                    }
                }

                // Sélectionner le statut
                foreach (ComboBoxItem item in StatutBox.Items)
                {
                    if (item.Content.ToString() == tacheToEdit.Statut)
                    {
                        StatutBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void Valider_Click(object sender, RoutedEventArgs e)
        {
            // Vérification des champs obligatoires
            if (string.IsNullOrWhiteSpace(TitreBox.Text))
            {
                MessageBox.Show("Veuillez saisir un titre pour la tâche", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Récupérer la catégorie sélectionnée
            Category selectedCategorie = null;
            if (CategorieBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is Category categorie)
            {
                selectedCategorie = categorie;
            }

            // Récupérer le statut sélectionné
            string selectedStatut = "À faire";
            if (StatutBox.SelectedItem is ComboBoxItem statutItem)
            {
                selectedStatut = statutItem.Content.ToString();
            }

            // Convertir DateTime en DateOnly pour la base de données
            DateOnly? dateEcheance = null;
            if (DateEcheanceBox.SelectedDate.HasValue)
            {
                dateEcheance = new DateOnly(
                    DateEcheanceBox.SelectedDate.Value.Year,
                    DateEcheanceBox.SelectedDate.Value.Month,
                    DateEcheanceBox.SelectedDate.Value.Day);
            }

            bool success;

            // Création ou mise à jour de la tâche
            if (_tacheToEdit == null)
            {
                // Nouvelle tâche
                var tache = new Tach
                {
                    Titre = TitreBox.Text,
                    Description = DescriptionBox.Text,
                    DateEcheance = dateEcheance,
                    CategorieId = selectedCategorie?.Id,
                    Statut = selectedStatut,
                    DateCreation = DateTime.Now
                };

                success = _dao.AddTache(tache);
            }
            else
            {
                // Mise à jour d'une tâche existante
                _tacheToEdit.Titre = TitreBox.Text;
                _tacheToEdit.Description = DescriptionBox.Text;
                _tacheToEdit.DateEcheance = dateEcheance;
                _tacheToEdit.CategorieId = selectedCategorie?.Id;
                _tacheToEdit.Statut = selectedStatut;

                success = _dao.UpdateTache(_tacheToEdit);
            }

            if (success)
            {
                TacheAjoutee = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Erreur lors de l'enregistrement de la tâche", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

