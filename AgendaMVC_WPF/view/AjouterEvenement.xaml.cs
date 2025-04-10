using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AgendaMVC_WPF.services;
using System.Diagnostics;

namespace AgendaMVC_WPF.view
{
    public partial class AjouterEvenement : Window
    {
        private GoogleCalendarService _calendarService;
        private CalendarEvent _eventToEdit;
        public bool EventAdded { get; private set; } = false;

        // Constructeur pour un nouvel événement
        public AjouterEvenement(DateTime date, GoogleCalendarService calendarService)
        {
            InitializeComponent();

            // Définir le style de la fenêtre pour qu'elle soit toujours au-dessus
            this.Topmost = true;

            _calendarService = calendarService;

            // Initialiser les dates
            DateDebutPicker.SelectedDate = date;
            DateFinPicker.SelectedDate = date;

            // Initialiser les heures et minutes
            InitializeTimeComboBoxes();

            // Sélectionner l'heure actuelle
            DateTime now = DateTime.Now;
            HeureDebutComboBox.SelectedIndex = now.Hour;
            MinuteDebutComboBox.SelectedIndex = now.Minute / 5; // Arrondir aux 5 minutes

            // Sélectionner l'heure actuelle + 1 heure
            DateTime later = now.AddHours(1);
            HeureFinComboBox.SelectedIndex = later.Hour;
            MinuteFinComboBox.SelectedIndex = later.Minute / 5; // Arrondir aux 5 minutes

            // Initialiser les couleurs
            InitializeColorPicker();

            WindowTitle.Text = "Ajouter un événement";

            Debug.WriteLine("Fenêtre d'ajout d'événement initialisée");
        }

        // Constructeur pour modifier un événement existant
        public AjouterEvenement(CalendarEvent eventToEdit, GoogleCalendarService calendarService)
        {
            InitializeComponent();

            // Définir le style de la fenêtre pour qu'elle soit toujours au-dessus
            this.Topmost = true;

            _calendarService = calendarService;
            _eventToEdit = eventToEdit;

            // Initialiser les heures et minutes
            InitializeTimeComboBoxes();

            // Remplir les champs avec les données de l'événement
            TitreBox.Text = eventToEdit.Summary;
            DescriptionBox.Text = eventToEdit.Description;
            LieuBox.Text = eventToEdit.Location;

            // Définir les dates
            DateDebutPicker.SelectedDate = eventToEdit.StartTime.Date;
            DateFinPicker.SelectedDate = eventToEdit.EndTime.Date;

            // Définir les heures si ce n'est pas un événement sur toute la journée
            if (!eventToEdit.IsAllDay)
            {
                HeureDebutComboBox.SelectedIndex = eventToEdit.StartTime.Hour;
                MinuteDebutComboBox.SelectedIndex = eventToEdit.StartTime.Minute / 5;

                HeureFinComboBox.SelectedIndex = eventToEdit.EndTime.Hour;
                MinuteFinComboBox.SelectedIndex = eventToEdit.EndTime.Minute / 5;
            }
            else
            {
                JourneeEntiereCheckBox.IsChecked = true;
            }

            // Initialiser les couleurs
            InitializeColorPicker();

            // Sélectionner la couleur de l'événement
            if (!string.IsNullOrEmpty(eventToEdit.ColorId))
            {
                foreach (RadioButton rb in CouleurStackPanel.Children)
                {
                    if (rb.Tag.ToString() == eventToEdit.ColorId)
                    {
                        rb.IsChecked = true;
                        break;
                    }
                }
            }

            // Afficher le bouton de suppression
            SupprimerButton.Visibility = Visibility.Visible;

            WindowTitle.Text = "Modifier un événement";

            Debug.WriteLine($"Fenêtre de modification d'événement initialisée pour: {eventToEdit.Summary}");
        }

        // Méthode pour initialiser les ComboBox d'heures et de minutes
        private void InitializeTimeComboBoxes()
        {
            // Vider les ComboBox avant de les remplir (pour éviter les doublons)
            HeureDebutComboBox.Items.Clear();
            HeureFinComboBox.Items.Clear();
            MinuteDebutComboBox.Items.Clear();
            MinuteFinComboBox.Items.Clear();

            // Remplir les ComboBox des heures (0-23)
            for (int i = 0; i < 24; i++)
            {
                HeureDebutComboBox.Items.Add(i.ToString("00"));
                HeureFinComboBox.Items.Add(i.ToString("00"));
            }

            // Remplir les ComboBox des minutes (0-55 par pas de 5)
            for (int i = 0; i < 60; i += 5)
            {
                MinuteDebutComboBox.Items.Add(i.ToString("00"));
                MinuteFinComboBox.Items.Add(i.ToString("00"));
            }

            // Sélectionner les valeurs par défaut
            HeureDebutComboBox.SelectedIndex = 0;
            MinuteDebutComboBox.SelectedIndex = 0;
            HeureFinComboBox.SelectedIndex = 1;
            MinuteFinComboBox.SelectedIndex = 0;
        }

        private void InitializeColorPicker()
        {
            // Vider le panel avant d'ajouter les couleurs (pour éviter les doublons)
            CouleurStackPanel.Children.Clear();

            // Ajouter les couleurs disponibles
            AddColorOption("1", "#7986CB"); // Lavande
            AddColorOption("2", "#33B679"); // Sauge
            AddColorOption("3", "#8E24AA"); // Raisin
            AddColorOption("4", "#E67C73"); // Flamant
            AddColorOption("5", "#F6BF26"); // Banane
            AddColorOption("6", "#F4511E"); // Mandarine
            AddColorOption("7", "#039BE5"); // Paon
            AddColorOption("8", "#616161"); // Graphite
            AddColorOption("9", "#3F51B5"); // Bleuet
            AddColorOption("10", "#0B8043"); // Basilic
            AddColorOption("11", "#D50000"); // Tomate

            // Sélectionner la première couleur par défaut
            if (CouleurStackPanel.Children.Count > 0 && CouleurStackPanel.Children[0] is RadioButton firstRb)
            {
                firstRb.IsChecked = true;
            }
        }

        private void AddColorOption(string colorId, string colorHex)
        {
            RadioButton rb = new RadioButton
            {
                GroupName = "EventColor",
                Tag = colorId,
                Width = 24,
                Height = 24,
                Margin = new Thickness(5),
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorHex))
            };

            // Style du RadioButton
            rb.Style = (Style)FindResource("ColorRadioButtonStyle");

            CouleurStackPanel.Children.Add(rb);
        }

        private void JourneeEntiereCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Désactiver les sélecteurs d'heure si l'événement est sur toute la journée
            HeureDebutComboBox.IsEnabled = false;
            MinuteDebutComboBox.IsEnabled = false;
            HeureFinComboBox.IsEnabled = false;
            MinuteFinComboBox.IsEnabled = false;
        }

        private void JourneeEntiereCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Activer les sélecteurs d'heure si l'événement n'est pas sur toute la journée
            HeureDebutComboBox.IsEnabled = true;
            MinuteDebutComboBox.IsEnabled = true;
            HeureFinComboBox.IsEnabled = true;
            MinuteFinComboBox.IsEnabled = true;
        }

        // Méthode pour valider et enregistrer l'événement
        private async void Valider_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Clic sur le bouton Valider");

                // Vérifier les champs obligatoires
                if (string.IsNullOrWhiteSpace(TitreBox.Text))
                {
                    MessageBox.Show("Veuillez saisir un titre pour l'événement", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!DateDebutPicker.SelectedDate.HasValue || !DateFinPicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Veuillez sélectionner des dates de début et de fin", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Récupérer la couleur sélectionnée
                string colorId = "1"; // Couleur par défaut
                foreach (RadioButton rb in CouleurStackPanel.Children)
                {
                    if (rb.IsChecked == true)
                    {
                        colorId = rb.Tag.ToString();
                        break;
                    }
                }

                // Créer l'objet CalendarEvent
                CalendarEvent calendarEvent = new CalendarEvent
                {
                    Summary = TitreBox.Text,
                    Description = DescriptionBox.Text,
                    Location = LieuBox.Text,
                    ColorId = colorId,
                    IsAllDay = JourneeEntiereCheckBox.IsChecked == true
                };

                // Définir les dates et heures
                if (calendarEvent.IsAllDay)
                {
                    calendarEvent.StartTime = DateDebutPicker.SelectedDate.Value;
                    calendarEvent.EndTime = DateFinPicker.SelectedDate.Value.AddDays(1); // Ajouter un jour pour inclure la journée entière
                }
                else
                {
                    // Vérifier que les heures sont sélectionnées
                    if (HeureDebutComboBox.SelectedItem == null || MinuteDebutComboBox.SelectedItem == null ||
                        HeureFinComboBox.SelectedItem == null || MinuteFinComboBox.SelectedItem == null)
                    {
                        // Si les heures ne sont pas sélectionnées, utiliser les valeurs par défaut
                        int heureDebut = HeureDebutComboBox.SelectedIndex >= 0 ? int.Parse(HeureDebutComboBox.SelectedItem.ToString()) : 0;
                        int minuteDebut = MinuteDebutComboBox.SelectedIndex >= 0 ? int.Parse(MinuteDebutComboBox.SelectedItem.ToString()) : 0;
                        int heureFin = HeureFinComboBox.SelectedIndex >= 0 ? int.Parse(HeureFinComboBox.SelectedItem.ToString()) : 1;
                        int minuteFin = MinuteFinComboBox.SelectedIndex >= 0 ? int.Parse(MinuteFinComboBox.SelectedItem.ToString()) : 0;

                        DateTime startDateTime = DateDebutPicker.SelectedDate.Value.Date.AddHours(heureDebut).AddMinutes(minuteDebut);
                        DateTime endDateTime = DateFinPicker.SelectedDate.Value.Date.AddHours(heureFin).AddMinutes(minuteFin);

                        calendarEvent.StartTime = startDateTime;
                        calendarEvent.EndTime = endDateTime;
                    }
                    else
                    {
                        // Utiliser les valeurs sélectionnées
                        int heureDebut = int.Parse(HeureDebutComboBox.SelectedItem.ToString());
                        int minuteDebut = int.Parse(MinuteDebutComboBox.SelectedItem.ToString());
                        int heureFin = int.Parse(HeureFinComboBox.SelectedItem.ToString());
                        int minuteFin = int.Parse(MinuteFinComboBox.SelectedItem.ToString());

                        DateTime startDateTime = DateDebutPicker.SelectedDate.Value.Date.AddHours(heureDebut).AddMinutes(minuteDebut);
                        DateTime endDateTime = DateFinPicker.SelectedDate.Value.Date.AddHours(heureFin).AddMinutes(minuteFin);

                        calendarEvent.StartTime = startDateTime;
                        calendarEvent.EndTime = endDateTime;
                    }
                }

                // Afficher l'indicateur de chargement
                LoadingIndicator.Visibility = Visibility.Visible;

                bool success;

                // Créer ou mettre à jour l'événement
                if (_eventToEdit == null)
                {
                    // Créer un nouvel événement
                    Debug.WriteLine("Création d'un nouvel événement");
                    var createdEvent = await _calendarService.CreateEventAsync(calendarEvent);
                    success = createdEvent != null;
                }
                else
                {
                    // Mettre à jour l'événement existant
                    Debug.WriteLine($"Mise à jour de l'événement: {_eventToEdit.Id}");
                    calendarEvent.Id = _eventToEdit.Id;
                    success = await _calendarService.UpdateEventAsync(calendarEvent);
                }

                if (success)
                {
                    Debug.WriteLine("Opération réussie");
                    EventAdded = true;
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    Debug.WriteLine("Échec de l'opération");
                    MessageBox.Show("Erreur lors de l'enregistrement de l'événement", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception lors de la validation: {ex.Message}");
                MessageBox.Show($"Une erreur s'est produite: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Masquer l'indicateur de chargement
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void Supprimer_Click(object sender, RoutedEventArgs e)
        {
            if (_eventToEdit == null)
                return;

            var result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer cet événement ?",
                "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Debug.WriteLine($"Suppression de l'événement: {_eventToEdit.Id}");

                    // Afficher l'indicateur de chargement
                    LoadingIndicator.Visibility = Visibility.Visible;

                    // Supprimer l'événement
                    bool success = await _calendarService.DeleteEventAsync(_eventToEdit.Id);

                    if (success)
                    {
                        Debug.WriteLine("Suppression réussie");
                        EventAdded = true; // Pour déclencher le rechargement des événements
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        Debug.WriteLine("Échec de la suppression");
                        MessageBox.Show("Erreur lors de la suppression de l'événement", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception lors de la suppression: {ex.Message}");
                    MessageBox.Show($"Une erreur s'est produite: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    // Masquer l'indicateur de chargement
                    LoadingIndicator.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Annuler_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Annulation");
            this.DialogResult = false;
            this.Close();
        }
    }
}
