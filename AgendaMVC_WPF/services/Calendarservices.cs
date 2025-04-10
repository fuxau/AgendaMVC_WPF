// Renommez votre classe pour éviter le conflit avec Google.Apis.Calendar.v3.CalendarService
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace AgendaMVC_WPF.services
{
    // Renommé pour éviter le conflit avec Google.Apis.Calendar.v3.CalendarService
    public class GoogleCalendarService
    {
        // Singleton instance
        private static GoogleCalendarService _instance;

        // Informations d'authentification OAuth 2.0
        private const string CLIENT_ID = "125571402554-4ra6l4aaii5u6garp1i8424jij5i3ffp.apps.googleusercontent.com";
        private const string CLIENT_SECRET = "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";
        private const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob"; // URI de redirection pour application de bureau

        // Clé API (pour les opérations de lecture seule)
        private readonly string _apiKey;

        // Informations d'authentification OAuth
        private string _accessToken;
        private DateTime _tokenExpiration;
        private string _refreshToken;

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://www.googleapis.com/calendar/v3";
        private readonly string _calendarId = "primary"; // Utilisez "primary" pour le calendrier principal de l'utilisateur

        private bool _isOAuthAuthenticated = false;

        // Chemin du fichier de tokens
        private readonly string _tokenFilePath;

        // Clé de chiffrement pour les tokens (simple protection)
        private static readonly byte[] EncryptionKey = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };

        // Constructeur privé pour le singleton
        private GoogleCalendarService(string apiKey = null)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();

            // Définir le chemin du fichier de tokens dans le dossier AppData de l'utilisateur
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AgendaMVC_WPF");

            // Créer le dossier s'il n'existe pas
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _tokenFilePath = Path.Combine(appDataPath, "google_tokens.dat");

            // Charger les tokens existants
            LoadTokens();
        }

        // Méthode pour obtenir l'instance singleton
        public static GoogleCalendarService GetInstance(string apiKey = null)
        {
            if (_instance == null)
            {
                _instance = new GoogleCalendarService(apiKey);
            }
            return _instance;
        }

        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                // Si nous avons déjà un token valide, on le réutilise
                if (!string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiration)
                {
                    _isOAuthAuthenticated = true;
                    return true;
                }

                // Si nous avons un refresh token, essayons de l'utiliser pour obtenir un nouveau token d'accès
                if (!string.IsNullOrEmpty(_refreshToken))
                {
                    bool refreshSuccess = await RefreshAccessTokenAsync();
                    if (refreshSuccess)
                    {
                        _isOAuthAuthenticated = true;
                        return true;
                    }
                }

                // Sinon, on essaie l'authentification OAuth complète
                bool oauthSuccess = await AuthenticateWithOAuthAsync();

                if (oauthSuccess)
                {
                    _isOAuthAuthenticated = true;
                    return true;
                }

                // Si OAuth échoue et qu'on a une clé API, on essaie avec la clé API
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    var url = $"{_baseUrl}/calendars/{_calendarId}/events?key={_apiKey}&maxResults=1";
                    var response = await _httpClient.GetAsync(url);

                    _isOAuthAuthenticated = false;
                    return response.IsSuccessStatusCode;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d'authentification: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                var tokenRequestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", CLIENT_ID),
                    new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),
                    new KeyValuePair<string, string>("refresh_token", _refreshToken),
                    new KeyValuePair<string, string>("grant_type", "refresh_token")
                });

                var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestContent);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                    var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenJson);

                    _accessToken = tokenData.AccessToken;
                    _tokenExpiration = DateTime.Now.AddSeconds(tokenData.ExpiresIn);

                    // Sauvegarder les tokens
                    SaveTokens();

                    return true;
                }
                else
                {
                    // Si le refresh token ne fonctionne plus, on le supprime
                    _refreshToken = null;
                    SaveTokens();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du rafraîchissement du token: {ex.Message}");
                return false;
            }
        }

        // Méthode pour authentifier explicitement avec OAuth (pour la page de paramètres)
        public async Task<bool> AuthenticateWithOAuthAsync(bool forceAuth = false)
        {
            try
            {
                // Si nous avons déjà un token valide et qu'on ne force pas l'authentification, on le réutilise
                if (!forceAuth && !string.IsNullOrEmpty(_accessToken) && DateTime.Now < _tokenExpiration)
                {
                    _isOAuthAuthenticated = true;
                    return true;
                }

                // Étape 1: Afficher l'URL d'autorisation à l'utilisateur
                string authUrl = $"https://accounts.google.com/o/oauth2/auth" +
                                $"?client_id={CLIENT_ID}" +
                                $"&redirect_uri={HttpUtility.UrlEncode(REDIRECT_URI)}" +
                                $"&scope=https://www.googleapis.com/auth/calendar https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email" +
                                $"&response_type=code" +
                                $"&access_type=offline" +
                                $"&prompt=consent"; // Force à demander un refresh token

                // Afficher l'URL à l'utilisateur et lui demander de s'authentifier
                var authWindow = new System.Windows.Window
                {
                    Title = "Authentification Google Calendar",
                    Width = 500,
                    Height = 300,
                    WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen
                };

                var stackPanel = new System.Windows.Controls.StackPanel
                {
                    Margin = new System.Windows.Thickness(20)
                };

                stackPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "Veuillez vous authentifier sur Google Calendar en suivant ces étapes:",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "1. Cliquez sur le bouton ci-dessous pour ouvrir la page d'authentification Google",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                });

                var openBrowserButton = new System.Windows.Controls.Button
                {
                    Content = "Ouvrir la page d'authentification",
                    Padding = new System.Windows.Thickness(10, 5, 10, 5),
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                };

                openBrowserButton.Click += (s, e) =>
                {
                    try
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = authUrl,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Erreur lors de l'ouverture du navigateur: {ex.Message}\n\nURL: {authUrl}",
                            "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    }
                };

                stackPanel.Children.Add(openBrowserButton);

                stackPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "2. Connectez-vous et autorisez l'application",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                });

                stackPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = "3. Copiez le code d'autorisation et collez-le ci-dessous:",
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                });

                var codeTextBox = new System.Windows.Controls.TextBox
                {
                    Padding = new System.Windows.Thickness(5),
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                };

                stackPanel.Children.Add(codeTextBox);

                var validateButton = new System.Windows.Controls.Button
                {
                    Content = "Valider",
                    Padding = new System.Windows.Thickness(10, 5, 10, 5),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right
                };

                string authCode = null;

                validateButton.Click += (s, e) =>
                {
                    authCode = codeTextBox.Text.Trim();
                    authWindow.DialogResult = true;
                    authWindow.Close();
                };

                stackPanel.Children.Add(validateButton);

                authWindow.Content = stackPanel;

                bool? result = authWindow.ShowDialog();

                if (result != true || string.IsNullOrEmpty(authCode))
                {
                    return false;
                }

                // Étape 2: Échanger le code d'autorisation contre un token d'accès
                var tokenRequestContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("code", authCode),
                    new KeyValuePair<string, string>("client_id", CLIENT_ID),
                    new KeyValuePair<string, string>("client_secret", CLIENT_SECRET),
                    new KeyValuePair<string, string>("redirect_uri", REDIRECT_URI),
                    new KeyValuePair<string, string>("grant_type", "authorization_code")
                });

                var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequestContent);

                if (tokenResponse.IsSuccessStatusCode)
                {
                    var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                    var tokenData = JsonSerializer.Deserialize<TokenResponse>(tokenJson);

                    _accessToken = tokenData.AccessToken;
                    _tokenExpiration = DateTime.Now.AddSeconds(tokenData.ExpiresIn);

                    // Stocker le refresh token s'il est fourni
                    if (!string.IsNullOrEmpty(tokenData.RefreshToken))
                    {
                        _refreshToken = tokenData.RefreshToken;
                    }

                    // Sauvegarder les tokens
                    SaveTokens();

                    _isOAuthAuthenticated = true;
                    return true;
                }
                else
                {
                    var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                    System.Windows.MessageBox.Show($"Erreur lors de l'échange du code: {tokenResponse.StatusCode}\n\n{errorContent}",
                        "Erreur d'authentification", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Erreur d'authentification OAuth: {ex.Message}",
                    "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                Console.WriteLine($"Erreur d'authentification OAuth: {ex.Message}");
                return false;
            }
        }

        // Méthode pour sauvegarder les tokens dans un fichier
        private void SaveTokens()
        {
            try
            {
                var tokenData = new
                {
                    AccessToken = _accessToken,
                    RefreshToken = _refreshToken,
                    Expiration = _tokenExpiration
                };

                string json = JsonSerializer.Serialize(tokenData);

                // Chiffrer les données avant de les sauvegarder
                byte[] encryptedData = EncryptData(Encoding.UTF8.GetBytes(json));

                File.WriteAllBytes(_tokenFilePath, encryptedData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des tokens: {ex.Message}");
            }
        }

        // Méthode pour charger les tokens depuis un fichier
        private void LoadTokens()
        {
            try
            {
                if (File.Exists(_tokenFilePath))
                {
                    byte[] encryptedData = File.ReadAllBytes(_tokenFilePath);

                    // Déchiffrer les données
                    byte[] decryptedData = DecryptData(encryptedData);

                    string json = Encoding.UTF8.GetString(decryptedData);
                    var tokenData = JsonSerializer.Deserialize<TokenData>(json);

                    _accessToken = tokenData.AccessToken;
                    _refreshToken = tokenData.RefreshToken;
                    _tokenExpiration = tokenData.Expiration;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des tokens: {ex.Message}");
                // En cas d'erreur, on supprime le fichier de tokens
                try
                {
                    if (File.Exists(_tokenFilePath))
                    {
                        File.Delete(_tokenFilePath);
                    }
                }
                catch { }
            }
        }

        // Méthode pour chiffrer les données
        private byte[] EncryptData(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    // Écrire l'IV au début du fichier
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        // Méthode pour déchiffrer les données
        private byte[] DecryptData(byte[] encryptedData)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;

                // Lire l'IV depuis le début des données chiffrées
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, iv.Length, encryptedData.Length - iv.Length);
                        cs.FlushFinalBlock();
                    }

                    return ms.ToArray();
                }
            }
        }

        public async Task<List<CalendarEvent>> GetEventsAsync(DateTime startDate, DateTime endDate, int maxResults = 100)
        {
            var events = new List<CalendarEvent>();

            try
            {
                // Formater les dates au format ISO 8601
                string timeMin = startDate.ToUniversalTime().ToString("o");
                string timeMax = endDate.ToUniversalTime().ToString("o");

                // Construire l'URL avec les paramètres
                string url;

                if (_isOAuthAuthenticated)
                {
                    url = $"{_baseUrl}/calendars/{_calendarId}/events" +
                          $"?timeMin={HttpUtility.UrlEncode(timeMin)}" +
                          $"&timeMax={HttpUtility.UrlEncode(timeMax)}" +
                          $"&maxResults={maxResults}" +
                          $"&singleEvents=true" +
                          $"&orderBy=startTime";

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                }
                else
                {
                    url = $"{_baseUrl}/calendars/{_calendarId}/events?key={_apiKey}" +
                          $"&timeMin={HttpUtility.UrlEncode(timeMin)}" +
                          $"&timeMax={HttpUtility.UrlEncode(timeMax)}" +
                          $"&maxResults={maxResults}" +
                          $"&singleEvents=true" +
                          $"&orderBy=startTime";

                    _httpClient.DefaultRequestHeaders.Authorization = null;
                }

                // Exécuter la requête
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var eventsResult = JsonSerializer.Deserialize<GoogleCalendarEventsResponse>(content);

                    if (eventsResult?.Items != null)
                    {
                        foreach (var item in eventsResult.Items)
                        {
                            DateTime start;
                            DateTime end;

                            if (item.Start.DateTime.HasValue)
                            {
                                start = item.Start.DateTime.Value;
                            }
                            else if (!string.IsNullOrEmpty(item.Start.Date))
                            {
                                start = DateTime.Parse(item.Start.Date);
                            }
                            else
                            {
                                // Si ni DateTime ni Date n'est disponible, utiliser la date actuelle
                                start = DateTime.Now;
                            }

                            if (item.End.DateTime.HasValue)
                            {
                                end = item.End.DateTime.Value;
                            }
                            else if (!string.IsNullOrEmpty(item.End.Date))
                            {
                                end = DateTime.Parse(item.End.Date);
                            }
                            else
                            {
                                // Si ni DateTime ni Date n'est disponible, utiliser start + 1 heure
                                end = start.AddHours(1);
                            }

                            events.Add(new CalendarEvent
                            {
                                Id = item.Id,
                                Summary = item.Summary ?? "Sans titre",
                                Description = item.Description,
                                Location = item.Location,
                                StartTime = start,
                                EndTime = end,
                                IsAllDay = !item.Start.DateTime.HasValue,
                                ColorId = item.ColorId
                            });
                        }
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erreur lors de la récupération des événements: {response.StatusCode}\n{errorContent}");
                }

                return events;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des événements: {ex.Message}");
                System.Windows.MessageBox.Show($"Erreur lors de la récupération des événements: {ex.Message}",
                    "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<CalendarEvent>();
            }
        }

        // Remplacer la méthode CreateEventAsync par celle-ci pour corriger l'erreur de fuseau horaire
        public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
        {
            if (!_isOAuthAuthenticated)
            {
                throw new InvalidOperationException("L'authentification OAuth est requise pour créer des événements.");
            }

            try
            {
                // Créer l'objet événement au format JSON
                object eventObject;
                if (calendarEvent.IsAllDay)
                {
                    eventObject = new
                    {
                        summary = calendarEvent.Summary,
                        description = calendarEvent.Description,
                        location = calendarEvent.Location,
                        colorId = calendarEvent.ColorId,
                        start = new { date = calendarEvent.StartTime.ToString("yyyy-MM-dd") },
                        end = new { date = calendarEvent.EndTime.ToString("yyyy-MM-dd") }
                    };
                }
                else
                {
                    // Utiliser "Europe/Paris" comme fuseau horaire explicite pour la France
                    string timeZone = "Europe/Paris";

                    eventObject = new
                    {
                        summary = calendarEvent.Summary,
                        description = calendarEvent.Description,
                        location = calendarEvent.Location,
                        colorId = calendarEvent.ColorId,
                        start = new
                        {
                            dateTime = calendarEvent.StartTime.ToUniversalTime().ToString("o"),
                            timeZone = timeZone
                        },
                        end = new
                        {
                            dateTime = calendarEvent.EndTime.ToUniversalTime().ToString("o"),
                            timeZone = timeZone
                        }
                    };
                }

                var json = JsonSerializer.Serialize(eventObject);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Définir l'en-tête d'autorisation
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                // Envoyer la requête
                var response = await _httpClient.PostAsync($"{_baseUrl}/calendars/{_calendarId}/events", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var createdEvent = JsonSerializer.Deserialize<GoogleCalendarEvent>(responseContent);

                    // Mettre à jour l'ID de l'événement
                    calendarEvent.Id = createdEvent.Id;

                    return calendarEvent;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erreur lors de la création de l'événement: {response.StatusCode}\n{errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création de l'événement: {ex.Message}");
                System.Windows.MessageBox.Show($"Erreur lors de la création de l'événement: {ex.Message}",
                    "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }

        // Remplacer également la méthode UpdateEventAsync pour utiliser le même fuseau horaire
        public async Task<bool> UpdateEventAsync(CalendarEvent calendarEvent)
        {
            if (!_isOAuthAuthenticated)
            {
                throw new InvalidOperationException("L'authentification OAuth est requise pour mettre à jour des événements.");
            }

            try
            {
                // Créer l'objet événement au format JSON
                object eventObject;
                if (calendarEvent.IsAllDay)
                {
                    eventObject = new
                    {
                        summary = calendarEvent.Summary,
                        description = calendarEvent.Description,
                        location = calendarEvent.Location,
                        colorId = calendarEvent.ColorId,
                        start = new { date = calendarEvent.StartTime.ToString("yyyy-MM-dd") },
                        end = new { date = calendarEvent.EndTime.ToString("yyyy-MM-dd") }
                    };
                }
                else
                {
                    // Utiliser "Europe/Paris" comme fuseau horaire explicite pour la France
                    string timeZone = "Europe/Paris";

                    eventObject = new
                    {
                        summary = calendarEvent.Summary,
                        description = calendarEvent.Description,
                        location = calendarEvent.Location,
                        colorId = calendarEvent.ColorId,
                        start = new
                        {
                            dateTime = calendarEvent.StartTime.ToUniversalTime().ToString("o"),
                            timeZone = timeZone
                        },
                        end = new
                        {
                            dateTime = calendarEvent.EndTime.ToUniversalTime().ToString("o"),
                            timeZone = timeZone
                        }
                    };
                }

                var json = JsonSerializer.Serialize(eventObject);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Définir l'en-tête d'autorisation
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                // Envoyer la requête
                var response = await _httpClient.PutAsync($"{_baseUrl}/calendars/{_calendarId}/events/{calendarEvent.Id}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erreur lors de la mise à jour de l'événement: {response.StatusCode}\n{errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour de l'événement: {ex.Message}");
                System.Windows.MessageBox.Show($"Erreur lors de la mise à jour de l'événement: {ex.Message}",
                    "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteEventAsync(string eventId)
        {
            if (!_isOAuthAuthenticated)
            {
                throw new InvalidOperationException("L'authentification OAuth est requise pour supprimer des événements.");
            }

            try
            {
                // Définir l'en-tête d'autorisation
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                // Envoyer la requête
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/calendars/{_calendarId}/events/{eventId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erreur lors de la suppression de l'événement: {response.StatusCode}\n{errorContent}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression de l'événement: {ex.Message}");
                System.Windows.MessageBox.Show($"Erreur lors de la suppression de l'événement: {ex.Message}",
                    "Erreur", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return false;
            }
        }

        // Méthode pour se déconnecter
        public async Task<bool> SignOutAsync()
        {
            try
            {
                // Révoquer le token d'accès si disponible
                if (!string.IsNullOrEmpty(_accessToken))
                {
                    var revokeUrl = $"https://oauth2.googleapis.com/revoke?token={_accessToken}";
                    var response = await _httpClient.PostAsync(revokeUrl, null);

                    // Même si la révocation échoue, on continue pour supprimer les tokens localement
                }

                // Supprimer les tokens
                _accessToken = null;
                _refreshToken = null;
                _tokenExpiration = DateTime.MinValue;
                _isOAuthAuthenticated = false;

                // Supprimer le fichier de tokens
                if (File.Exists(_tokenFilePath))
                {
                    File.Delete(_tokenFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la déconnexion: {ex.Message}");
                return false;
            }
        }

        // Classe pour les informations utilisateur
        public class UserInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Picture { get; set; }
        }

        // Méthode pour récupérer les informations de l'utilisateur
        public async Task<UserInfo> GetUserInfoAsync()
        {
            if (!_isOAuthAuthenticated)
            {
                return null;
            }

            try
            {
                // Définir l'en-tête d'autorisation
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                // Récupérer les informations de l'utilisateur
                var response = await _httpClient.GetAsync("https://www.googleapis.com/oauth2/v2/userinfo");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonSerializer.Deserialize<UserInfo>(content);
                    return userInfo;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Erreur lors de la récupération des informations utilisateur: {response.StatusCode}\n{errorContent}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des informations utilisateur: {ex.Message}");
                return null;
            }
        }
    }

    // Classes pour la désérialisation JSON
    public class TokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }

    // Classe pour stocker les tokens
    public class TokenData
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }

    public class GoogleCalendarEventsResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        public string Kind { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("etag")]
        public string Etag { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("summary")]
        public string Summary { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string Description { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("updated")]
        public string Updated { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("accessRole")]
        public string AccessRole { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("items")]
        public List<GoogleCalendarEvent> Items { get; set; }
    }

    public class GoogleCalendarEvent
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("status")]
        public string Status { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("summary")]
        public string Summary { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("description")]
        public string Description { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("location")]
        public string Location { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("colorId")]
        public string ColorId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("start")]
        public GoogleEventDateTime Start { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("end")]
        public GoogleEventDateTime End { get; set; }
    }

    public class GoogleEventDateTime
    {
        [System.Text.Json.Serialization.JsonPropertyName("dateTime")]
        public DateTime? DateTime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("date")]
        public string Date { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("timeZone")]
        public string TimeZone { get; set; }
    }

    public class CalendarEvent
    {
        public string Id { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAllDay { get; set; }
        public string ColorId { get; set; }
    }
}
