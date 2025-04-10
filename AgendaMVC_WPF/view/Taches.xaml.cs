using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AgendaMVC_WPF.agendaDB;
using AgendaMVC_WPF.DAO;
using MahApps.Metro.IconPacks;

namespace AgendaMVC_WPF.view
{
    public partial class Taches : UserControl
    {
        private Agenda_DAO _dao;
        private string _currentFilter = "Toutes";

        public Taches()
        {
            InitializeComponent();
            _dao = new Agenda_DAO();
            RefreshTaches();
        }

        public void RefreshTaches()
        {
            // Récupérer les tâches selon le filtre actuel
            List<Tach> taches;
            switch (_currentFilter)
            {
                case "À faire":
                    taches = _dao.GetTachesByStatus("À faire");
                    break;
                case "En cours":
                    taches = _dao.GetTachesByStatus("En cours");
                    break;
                case "Terminées":
                    taches = _dao.GetTachesByStatus("Terminé");
                    break;
                default:
                    taches = _dao.GetAllTaches();
                    break;
            }

            // Mettre à jour le compteur
            TachesCountTextBlock.Text = $"{taches.Count} tâches";

            // Mettre à jour la source de données
            tachesDataGrid.ItemsSource = taches;
        }

        private void ADD_BTN_Click(object sender, RoutedEventArgs e)
        {
            var fenetre = new AjouterTache();
            fenetre.ShowDialog();

            if (fenetre.TacheAjoutee)
            {
                RefreshTaches();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tach tache)
            {
                var fenetre = new AjouterTache(tache);
                fenetre.ShowDialog();

                if (fenetre.TacheAjoutee)
                {
                    RefreshTaches();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tach tache)
            {
                var result = MessageBox.Show($"Êtes-vous sûr de vouloir supprimer la tâche '{tache.Titre}' ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = _dao.DeleteTache(tache.Id);
                    if (success)
                    {
                        RefreshTaches();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de la suppression de la tâche", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is Tach tache)
            {
                if (comboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    string newStatus = selectedItem.Content.ToString();
                    if (tache.Statut != newStatus)
                    {
                        tache.Statut = newStatus;
                        _dao.UpdateTache(tache);
                        RefreshTaches();
                    }
                }
            }
        }

        private void ToutesButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(ToutesButton);
            _currentFilter = "Toutes";
            RefreshTaches();
        }

        private void AFaireButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(AFaireButton);
            _currentFilter = "À faire";
            RefreshTaches();
        }

        private void EnCoursButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(EnCoursButton);
            _currentFilter = "En cours";
            RefreshTaches();
        }

        private void TermineesButton_Click(object sender, RoutedEventArgs e)
        {
            SetActiveTab(TermineesButton);
            _currentFilter = "Terminées";
            RefreshTaches();
        }

        private void SetActiveTab(Button activeButton)
        {
            // Réinitialiser tous les boutons
            ToutesButton.Style = (Style)FindResource("TabButtonStyle");
            AFaireButton.Style = (Style)FindResource("TabButtonStyle");
            EnCoursButton.Style = (Style)FindResource("TabButtonStyle");
            TermineesButton.Style = (Style)FindResource("TabButtonStyle");

            // Activer le bouton sélectionné
            activeButton.Style = (Style)FindResource("ActiveTabButtonStyle");
        }
    }
}

