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
    public partial class Nastaveni : UserControl
    {
        public Nastaveni()
        {
            InitializeComponent();
        }

        private void Databaze_Click(object sender, RoutedEventArgs e)
        {
            Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Databaze_editor());
        }
        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Menu());
        }

        private void Databaze2_Click(object sender, RoutedEventArgs e)
        {
            Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Databaze2_editor());
        }
    }
}