using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AgendaMVC_WPF.DAO;
using AgendaMVC_WPF.view;
using MahApps.Metro.IconPacks;
using System.Diagnostics;

namespace AgendaMVC_WPF
{
    public partial class MainWindow : Window
    {
        private Button _currentActiveButton;

        public MainWindow()
        {
            InitializeComponent();
            ContentControl.Content = new Contactes();
            _currentActiveButton = BTN_Contacts;
            SetActiveButton(BTN_Contacts);
            var dao = new Agenda_DAO();
        }

        private bool IsMaximize = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximize)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    IsMaximize = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximize = true;
                }
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void BTN_Contacts_Click(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Contactes();
            SetActiveButton(BTN_Contacts);
        }

        private void BTN_Taches_Click(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Taches();
            SetActiveButton(BTN_Taches);
        }

        private void BTN_Evenements_Click(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Evenements();
            SetActiveButton(BTN_Evenements);
        }

        private void BTN_Parametres_Click(object sender, RoutedEventArgs e)
        {
            ContentControl.Content = new Parametres();
            SetActiveButton(BTN_Parametres);
        }

        private void SetActiveButton(Button button)
        {
            // Réinitialiser le bouton actif précédent
            if (_currentActiveButton != null)
            {
                _currentActiveButton.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#623ED0");
            }

            // Définir le nouveau bouton actif
            button.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#7B5CD6");
            _currentActiveButton = button;
        }




        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MaximizeIcon.Kind = PackIconMaterialKind.WindowMaximize;
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizeIcon.Kind = PackIconMaterialKind.WindowRestore;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
    }
}
