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

namespace AZ_Kviz
{
    /// <summary>
    /// Interakční logika pro Konec_hry.xaml
    /// </summary>
    public partial class Konec_hry : Window
    {
        public Konec_hry(string message)
        {
            InitializeComponent();

            string messageWinner = message;
            label1.Content = messageWinner;

        }

        // Spuštění nové hry
        private void Nova_Click(object sender, RoutedEventArgs e)
        {
            Nastaveni_hracu nastaveni_Hracu = new Nastaveni_hracu();
            nastaveni_Hracu.ShowDialog();
            this.Close();
        }

        // Zavření hry
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            
            this.Close();           
        }
    }
}
