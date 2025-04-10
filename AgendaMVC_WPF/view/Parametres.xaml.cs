using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AgendaMVC_WPF.services;
using System.Windows.Data;
using System.Diagnostics;

namespace AgendaMVC_WPF.view
{
    public partial class Parametres : UserControl
    {
        private GoogleCalendarService _calendarService;
        private string _profileImageUrl;
        private bool _isLoading = false;

        public Parametres()
        {
            InitializeComponent();

            // Récupérer l'instance du service de calendrier
            _calendarService = GoogleCalendarService.GetInstance();

            // Charger les informations du compte
            LoadAccountInfoAsync();
        }

        private async void LoadAccountInfoAsync()
        {
            try
            {
                _isLoading = true;
                LoadingIndicator.Visibility = Visibility.Visible;

                // Vérifier si l'utilisateur est authentifié
                bool isAuthenticated = await _calendarService.AuthenticateAsync();

                if (isAuthenticated)
                {
                    // Récupérer les informations du compte
                    var userInfo = await _calendarService.GetUserInfoAsync();

                    if (userInfo != null)
                    {



                        // Afficher le panneau du compte connecté
                        ConnectedPanel.Visibility = Visibility.Visible;
                        NotConnectedPanel.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Debug.WriteLine("Impossible de récupérer les informations de l'utilisateur");
                        // Afficher le panneau de connexion
                        ConnectedPanel.Visibility = Visibility.Collapsed;
                        NotConnectedPanel.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    Debug.WriteLine("L'utilisateur n'est pas authentifié");
                    // Afficher le panneau de connexion
                    ConnectedPanel.Visibility = Visibility.Collapsed;
                    NotConnectedPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception lors du chargement des informations du compte: {ex.Message}");
                MessageBox.Show($"Erreur lors du chargement des informations du compte: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);

                // Afficher le panneau de connexion en cas d'erreur
                ConnectedPanel.Visibility = Visibility.Collapsed;
                NotConnectedPanel.Visibility = Visibility.Visible;
            }
            finally
            {
                _isLoading = false;
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

       

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isLoading = true;
                LoadingIndicator.Visibility = Visibility.Visible;

                // Se connecter à Google Calendar
                bool isAuthenticated = await _calendarService.AuthenticateWithOAuthAsync(true);

                if (isAuthenticated)
                {
                    // Recharger les informations du compte
                    await Task.Delay(1000); // Attendre un peu pour s'assurer que les informations sont disponibles
                    LoadAccountInfoAsync();
                }
                else
                {
                    MessageBox.Show("Impossible de se connecter à Google Calendar. Veuillez réessayer.",
                        "Erreur de connexion", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la connexion: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Êtes-vous sûr de vouloir vous déconnecter de Google Calendar ?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _isLoading = true;
                    LoadingIndicator.Visibility = Visibility.Visible;

                    // Se déconnecter de Google Calendar
                    await _calendarService.SignOutAsync();

                    // Réinitialiser l'interface
                    ConnectedPanel.Visibility = Visibility.Collapsed;
                    NotConnectedPanel.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la déconnexion: {ex.Message}",
                    "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isLoading = false;
                LoadingIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
