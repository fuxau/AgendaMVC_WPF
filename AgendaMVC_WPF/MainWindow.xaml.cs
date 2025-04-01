using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AgendaMVC_WPF.DAO;
using AgendaMVC_WPF.view;
using MahApps.Metro.Controls;

namespace AgendaMVC_WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    public Contactes _contactesUC;
    public MainWindow()
    {
        InitializeComponent();
        _contactesUC = new Contactes();
        ContentControl.Content = _contactesUC;
        ContentControl.Content = new Contactes();
        var dao = new Agenda_DAO();
        var contacts = dao.GetAllContacts();
        if (contacts != null && contacts.Any())
        {
            int dernierId = contacts.Max(c => c.Id);
            DernierIdTextBlock.Text = $"{dernierId} total contacts";
        }
        else
        {
            DernierIdTextBlock.Text = "Aucun contact";
        }


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
        
    }

    private void ADD_BTN_Click(object sender, RoutedEventArgs e)
    {
        var fenetre = new AjouterContact();
        fenetre.ShowDialog();

        if (fenetre.ContactAjoute)
        {
            _contactesUC.RefreshContacts();
        }
    }
    

}
