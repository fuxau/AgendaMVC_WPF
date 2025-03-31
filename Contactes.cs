public partial class Contactes : UserControl
{
    public Contactes()
    {
        InitializeComponent(); // Ensure this is called first
        LV_Artists.ItemsSource = new DAO.Agenda_DAO().GetAllContacts();
    }
}
