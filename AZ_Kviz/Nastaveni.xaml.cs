using System;
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
using Microsoft.Data.Sqlite;
using System.IO;

namespace AZ_Kviz
{
    /// <summary>
    /// Interakční logika pro Nastaveni.xaml
    /// </summary>
    public partial class Nastaveni : Window
    {
        public Nastaveni()
        {
            InitializeComponent();
        }

        private void Databaze_Click(object sender, RoutedEventArgs e)
        {
            Databaze_editor databaze_Editor = new Databaze_editor();
            databaze_Editor.Show();
            this.Close();
        }
        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
            this.Close();
        }
    }
}
