using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using AgendaMVC_WPF.services;
using MahApps.Metro.IconPacks;
using System.Diagnostics;

namespace AgendaMVC_WPF.view
{
    public partial class Evenements : UserControl
    {
        private GoogleCalendarService _calendarService;
        private List<CalendarEvent> _events;
        private DateTime _currentMonth;
        private const string API_KEY = "AIzaSyDQZMuVkQqF9UQIlHNEWxrIjUXwxkfFWiQ"; // Votre clé API Google

        public Evenements()
        {
            InitializeComponent();

            try
            {
                // Utiliser l'instance singleton du CalendarService
                _calendarService = GoogleCalendarService.GetInstance(API_KEY);
                _currentMonth = DateTime.Now;

                // Initialiser le calendrier
                InitializeCalendar();

                // Charger les événements
                LoadEventsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeCalendar()
        {
            try
            {
                // Afficher le mois et l'année en cours
                MonthYearTextBlock.Text = _currentMonth.ToString("MMMM yyyy");

                // Effacer les jours existants
                CalendarGrid.Children.Clear();
                CalendarGrid.RowDefinitions.Clear();

                // Ajouter les définitions de lignes pour les en-têtes et les semaines
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(40) });

                // Ajouter les en-têtes des jours de la semaine
                string[] daysOfWeek = { "Lun", "Mar", "Mer", "Jeu", "Ven", "Sam", "Dim" };
                for (int i = 0; i < 7; i++)
                {
                    TextBlock dayHeader = new TextBlock
                    {
                        Text = daysOfWeek[i],
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(5),
                        Foreground = new SolidColorBrush(Colors.Gray)
                    };

                    Grid.SetRow(dayHeader, 0);
                    Grid.SetColumn(dayHeader, i);
                    CalendarGrid.Children.Add(dayHeader);
                }

                // Déterminer le premier jour du mois
                DateTime firstDayOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);

                // Déterminer le jour de la semaine du premier jour (0 = lundi, 6 = dimanche)
                int firstDayOfWeek = ((int)firstDayOfMonth.DayOfWeek + 6) % 7;

                // Calculer le nombre de semaines nécessaires
                int weeksNeeded = (int)Math.Ceiling((daysInMonth + firstDayOfWeek) / 7.0);

                // Ajouter les définitions de lignes pour les semaines
                for (int i = 0; i < weeksNeeded; i++)
                {
                    CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                }

                // Ajouter les jours du mois
                int currentDay = 1;
                for (int week = 0; week < weeksNeeded; week++)
                {
                    for (int day = 0; day < 7; day++)
                    {
                        if ((week == 0 && day < firstDayOfWeek) || currentDay > daysInMonth)
                        {
                            // Case vide
                            continue;
                        }

                        // Créer un conteneur pour le jour
                        Border dayBorder = new Border
                        {
                            Background = new SolidColorBrush(Colors.White),
                            CornerRadius = new CornerRadius(8),
                            Margin = new Thickness(2),
                            Padding = new Thickness(5),
                            Tag = new DateTime(_currentMonth.Year, _currentMonth.Month, currentDay),
                            Cursor = Cursors.Hand // Ajouter un curseur main pour indiquer que c'est cliquable
                        };

                        // Ajouter une ombre
                        dayBorder.Effect = new System.Windows.Media.Effects.DropShadowEffect
                        {
                            BlurRadius = 5,
                            ShadowDepth = 1,
                            Direction = 270,
                            Opacity = 0.2,
                            Color = Colors.Gray
                        };

                        // Créer un StackPanel pour contenir le numéro du jour et les événements
                        StackPanel dayPanel = new StackPanel();

                        // Ajouter le numéro du jour
                        TextBlock dayNumber = new TextBlock
                        {
                            Text = currentDay.ToString(),
                            FontWeight = FontWeights.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 0, 0, 5)
                        };

                        // Mettre en évidence le jour actuel
                        if (_currentMonth.Year == DateTime.Now.Year &&
                            _currentMonth.Month == DateTime.Now.Month &&
                            currentDay == DateTime.Now.Day)
                        {
                            dayNumber.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C63FF"));
                            dayNumber.FontWeight = FontWeights.Bold;
                        }

                        dayPanel.Children.Add(dayNumber);

                        // Ajouter le StackPanel au Border
                        dayBorder.Child = dayPanel;

                        // Ajouter le Border au Grid
                        Grid.SetRow(dayBorder, week + 1); // +1 car la première ligne contient les en-têtes
                        Grid.SetColumn(dayBorder, day);
                        CalendarGrid.Children.Add(dayBorder);

                        // Ajouter des gestionnaires d'événements pour le clic et le survol
                        dayBorder.MouseLeftButtonDown += DayBorder_MouseLeftButtonDown;
                        dayBorder.MouseEnter += DayBorder_MouseEnter;
                        dayBorder.MouseLeave += DayBorder_MouseLeave;

                        currentDay++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'initialisation du calendrier: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Gestionnaire d'événement pour le survol de la souris (entrée)
        private void DayBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Changer la couleur de fond au survol
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F5F9FF"));

                // Ajouter une bordure pour mettre en évidence
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C63FF"));
            }
        }

        // Gestionnaire d'événement pour le survol de la souris (sortie)
        private void DayBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Rétablir la couleur de fond d'origine
                border.Background = new SolidColorBrush(Colors.White);

                // Supprimer la bordure
                border.BorderThickness = new Thickness(0);
            }
        }

        private async void LoadEventsAsync()
        {
            try
            {
                // Afficher l'indicateur de chargement
                LoadingIndicator.Visibility = Visibility.Visible;

                // Authentifier avec Google Calendar
                bool isAuthenticated = await _calendarService.AuthenticateAsync();

                if (isAuthenticated)
                {
                    // Définir la plage de dates pour le mois en cours
                    DateTime startDate = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
                    DateTime endDate = startDate.AddMonths(1).AddDays(-1);

                    // Récupérer les événements
                    _events = await _calendarService.GetEventsAsync(startDate, endDate);

                    // Afficher les événements sur le calendrier
                    DisplayEvents();
                }
                else
                {
                    MessageBox.Show("Impossible de se connecter à Google Calendar. Veuillez vérifier vos identifiants.",
                        "Erreur d'authentification", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur s'est produite: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Masquer l'indicateur de chargement
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void DisplayEvents()
        {
            if (_events == null || _events.Count == 0)
                return;

            // Parcourir tous les jours du calendrier
            foreach (var child in CalendarGrid.Children)
            {
                if (child is Border dayBorder && dayBorder.Tag is DateTime date)
                {
                    // Récupérer le StackPanel contenant les événements
                    if (dayBorder.Child is StackPanel dayPanel)
                    {
                        // Trouver les événements pour cette date
                        var eventsForDay = _events.FindAll(e =>
                            (e.StartTime.Date <= date.Date && e.EndTime.Date >= date.Date));

                        // Ajouter les événements au StackPanel
                        foreach (var evt in eventsForDay)
                        {
                            // Limiter à 3 événements par jour pour éviter de surcharger l'affichage
                            if (dayPanel.Children.Count > 3)
                            {
                                TextBlock moreEvents = new TextBlock
                                {
                                    Text = $"+ {eventsForDay.Count - 3} autres",
                                    FontSize = 10,
                                    Foreground = new SolidColorBrush(Colors.Gray),
                                    HorizontalAlignment = HorizontalAlignment.Center
                                };
                                dayPanel.Children.Add(moreEvents);
                                break;
                            }

                            // Créer un Border pour l'événement
                            Border eventBorder = new Border
                            {
                                Background = GetEventColor(evt.ColorId),
                                CornerRadius = new CornerRadius(4),
                                Padding = new Thickness(5, 2, 5, 2),
                                Margin = new Thickness(0, 2, 0, 2),
                                Tag = evt,
                                Cursor = Cursors.Hand // Ajouter un curseur main pour indiquer que c'est cliquable
                            };

                            // Ajouter le titre de l'événement
                            TextBlock eventTitle = new TextBlock
                            {
                                Text = evt.Summary,
                                TextTrimming = TextTrimming.CharacterEllipsis,
                                FontSize = 10,
                                Foreground = new SolidColorBrush(Colors.White)
                            };

                            eventBorder.Child = eventTitle;
                            dayPanel.Children.Add(eventBorder);

                            // Ajouter un gestionnaire d'événements pour le clic et le survol
                            eventBorder.MouseLeftButtonDown += EventBorder_MouseLeftButtonDown;
                            eventBorder.MouseEnter += EventBorder_MouseEnter;
                            eventBorder.MouseLeave += EventBorder_MouseLeave;
                        }
                    }
                }
            }
        }

        // Gestionnaire d'événement pour le survol de la souris sur un événement (entrée)
        private void EventBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Ajouter une bordure pour mettre en évidence
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = new SolidColorBrush(Colors.White);

                // Augmenter légèrement l'opacité
                border.Opacity = 0.9;
            }
        }

        // Gestionnaire d'événement pour le survol de la souris sur un événement (sortie)
        private void EventBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                // Supprimer la bordure
                border.BorderThickness = new Thickness(0);

                // Rétablir l'opacité d'origine
                border.Opacity = 1.0;
            }
        }

        private SolidColorBrush GetEventColor(string colorId)
        {
            // Couleurs par défaut pour les événements Google Calendar
            switch (colorId)
            {
                case "1": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7986CB")); // Lavande
                case "2": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33B679")); // Sauge
                case "3": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8E24AA")); // Raisin
                case "4": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E67C73")); // Flamant
                case "5": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F6BF26")); // Banane
                case "6": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4511E")); // Mandarine
                case "7": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#039BE5")); // Paon
                case "8": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#616161")); // Graphite
                case "9": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3F51B5")); // Bleuet
                case "10": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0B8043")); // Basilic
                case "11": return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D50000")); // Tomate
                default: return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6C63FF")); // Couleur par défaut
            }
        }

        // Méthode pour gérer le clic sur un jour du calendrier
        private void DayBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border dayBorder && dayBorder.Tag is DateTime date)
            {
                try
                {
                    // Empêcher la propagation de l'événement
                    e.Handled = true;
                    Debug.WriteLine($"Clic sur le jour: {date.ToShortDateString()}");

                    // Créer et afficher la fenêtre d'ajout d'événement directement
                    Window mainWindow = Application.Current.MainWindow;
                    AjouterEvenement window = new AjouterEvenement(date, _calendarService);

                    // Configurer la fenêtre pour qu'elle soit modale et toujours au-dessus
                    window.Owner = mainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Topmost = true;
                    window.ShowInTaskbar = false;

                    // Afficher la fenêtre de manière modale
                    bool? result = window.ShowDialog();

                    // Recharger les événements si un événement a été ajouté
                    if (window.EventAdded)
                    {
                        LoadEventsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception lors de l'ouverture de la fenêtre d'ajout: {ex.Message}");
                    MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre d'ajout: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Méthode pour gérer le clic sur un événement
        private void EventBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border eventBorder && eventBorder.Tag is CalendarEvent calendarEvent)
            {
                try
                {
                    // Empêcher la propagation de l'événement
                    e.Handled = true;
                    Debug.WriteLine($"Clic sur l'événement: {calendarEvent.Summary}");

                    // Créer et afficher la fenêtre de modification d'événement directement
                    Window mainWindow = Application.Current.MainWindow;
                    AjouterEvenement window = new AjouterEvenement(calendarEvent, _calendarService);

                    // Configurer la fenêtre pour qu'elle soit modale et toujours au-dessus
                    window.Owner = mainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    window.Topmost = true;
                    window.ShowInTaskbar = false;

                    // Afficher la fenêtre de manière modale
                    bool? result = window.ShowDialog();

                    // Recharger les événements si un événement a été modifié
                    if (window.EventAdded)
                    {
                        LoadEventsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception lors de l'ouverture de la fenêtre de modification: {ex.Message}");
                    MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre de modification: {ex.Message}",
                        "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Méthode pour le bouton d'ajout d'événement
        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine("Clic sur le bouton d'ajout d'événement");

                // Créer et afficher la fenêtre d'ajout d'événement directement
                Window mainWindow = Application.Current.MainWindow;
                AjouterEvenement window = new AjouterEvenement(DateTime.Now, _calendarService);

                // Configurer la fenêtre pour qu'elle soit modale et toujours au-dessus
                window.Owner = mainWindow;
                window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                window.Topmost = true;
                window.ShowInTaskbar = false;

                // Afficher la fenêtre de manière modale
                bool? result = window.ShowDialog();

                // Recharger les événements si un événement a été ajouté
                if (window.EventAdded)
                {
                    LoadEventsAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception lors de l'ouverture de la fenêtre d'ajout: {ex.Message}");
                MessageBox.Show($"Erreur lors de l'ouverture de la fenêtre d'ajout: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(-1);
            InitializeCalendar();
            LoadEventsAsync();
        }

        private void NextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = _currentMonth.AddMonths(1);
            InitializeCalendar();
            LoadEventsAsync();
        }

        private void Today_Click(object sender, RoutedEventArgs e)
        {
            _currentMonth = DateTime.Now;
            InitializeCalendar();
            LoadEventsAsync();
        }
    }
}
