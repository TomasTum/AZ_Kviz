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
using Microsoft.Data.Sqlite;
using System.IO;

namespace AZ_Kviz
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Hra_Click(object sender, RoutedEventArgs e)
        {
            Set_players setPlayers = new Set_players();
            setPlayers.ShowDialog();
        }

        private void Nastaveni_Click(object sender, RoutedEventArgs e)
        {
            Nastaveni nastaveni = new Nastaveni();
            nastaveni.ShowDialog();
        }

        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}