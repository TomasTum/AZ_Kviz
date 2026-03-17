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
    /// Interakční logika pro Nastaveni_hracu.xaml
    /// </summary>
    public partial class Nastaveni_hracu : UserControl
    {
        public Nastaveni_hracu()
        {
            InitializeComponent();
        }

        private void Zpet_Click(object sender, RoutedEventArgs e)
        {
            var mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Menu());
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string player1Name = jmeno1.Text;
            string player2Name = jmeno2.Text;

            // Počet otázek potřebných pro hru
            int requiredQuestions = 28;
            int requiredSubQuestions = 28;

            // Získání vybrané kategorie z ComboBoxu
            var selectedItem = kategorieselect.SelectedItem as ComboBoxItem;
            string selectedCategory = selectedItem?.Tag?.ToString() ?? "Vse";

            if (string.IsNullOrWhiteSpace(player1Name) || string.IsNullOrWhiteSpace(player2Name))
            {
                MessageBox.Show("Prosím, zadejte jména obou hráčů.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                int totalQuestions = Database.GetAllQuestions(selectedCategory).Count;
                int totalSubQuestions = Database.GetAllSubQuestions(selectedCategory).Count;

                if (totalQuestions < requiredQuestions)
                {
                    MessageBox.Show($"Není dostatek otázek v databázi pro zahájení hry. Požadováno: {requiredQuestions} otázek, dostupných: {totalQuestions} otázek.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                    
                }
                else if (totalSubQuestions < requiredSubQuestions)
                {
                    MessageBox.Show($"Není dostatek náhradních otázek v databázi pro zahájení hry. Požadováno: {requiredSubQuestions} otázek, dostupných: {totalSubQuestions} otázek.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Player player1 = new Player(player1Name, Brushes.Orange, CellState.Player1);
                    Player player2 = new Player(player2Name, Brushes.DeepSkyBlue, CellState.Player2);

                    Hra hra = new Hra(player1, player2, selectedCategory);

                    var mainWin = (Hlavni_okno)Window.GetWindow(this);
                    mainWin.SwitchView(hra);
                }
            }
        }
    }
}
