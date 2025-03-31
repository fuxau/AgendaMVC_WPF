﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AgendaMVC_WPF.DAO;

namespace AgendaMVC_WPF.view
{
    /// <summary>
    /// Logique d'interaction pour Contactes.xaml
    /// </summary>
    public partial class Contactes : UserControl
    {
        public Contactes()
        {
            InitializeComponent();
            LV_Artists.ItemsSource = new DAO.Agenda_DAO().GetAllContacts();
        }
    }
}
