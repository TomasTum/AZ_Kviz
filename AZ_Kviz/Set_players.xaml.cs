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
    /// Interakční logika pro Set_players.xaml
    /// </summary>
    public partial class Set_players : Window
    {
        public Set_players()
        {
            InitializeComponent();
        }

        private void Zpet_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string player1 = jmeno1.Text;
            string player2 = jmeno2.Text;

            if(string.IsNullOrWhiteSpace(player1) || string.IsNullOrWhiteSpace(player2))
            {
                MessageBox.Show("Prosím, zadejte jména obou hráčů.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                Hra hra = new Hra();
                
                this.Close();
                hra.ShowDialog();
            }
        }
    }
}
