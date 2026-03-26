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
    public partial class Menu : UserControl
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void Hra_Click(object sender, RoutedEventArgs e)
        {
            Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Nastaveni_hracu());
        }

        private void Nastaveni_Click(object sender, RoutedEventArgs e)
        {
            Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Nastaveni());
        }

        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}