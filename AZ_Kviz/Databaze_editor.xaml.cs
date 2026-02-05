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
    /// Interakční logika pro Databaze_editor.xaml
    /// </summary>
    public partial class Databaze_editor : Window
    {
        public Databaze_editor()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var data = Database.GetAllQuestions();
                Datagrid.ItemsSource = data;

                // Zabrání výchozímu mazání řádku DataGridem a připojí vlastní obsluhu kláves
                Datagrid.CanUserDeleteRows = false;
                Datagrid.PreviewKeyDown += Datagrid_PreviewKeyDown;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat: " + ex.Message, "Chyba");
            }
        }

        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Odebrat_Click(object sender, RoutedEventArgs e)
        {
            DeleteQuestion();
        }

        // Použití PreviewKeyDown, aby výchozí chování DataGridu (smazání řádku) nebylo provedeno
        private void Datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteQuestion();
                e.Handled = true;
            }
        }

        private void Pridat_Click(object sender, RoutedEventArgs e)
        {
            Pridat_otazku pridat_Otazku = new Pridat_otazku();
            pridat_Otazku.ShowDialog();
            // Aktualizace datagridu po přidání otázky
            var data = Database.GetAllQuestions();
            Datagrid.ItemsSource = data;

        }

        private void Aktualizovat_Click(object sender, RoutedEventArgs e)
        {
            var data = Database.GetAllQuestions();
            Datagrid.ItemsSource = data;
        }

        private void Upravit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteQuestion()
        {
            if (Datagrid.SelectedItem is Question vybranyRadek)
            {
                MessageBoxResult result = MessageBox.Show($"Chcete opravdu smazat otázku: {vybranyRadek.Otazka}?", "Upozornění", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Database.DeleteQuestion(vybranyRadek.Id);
                        try
                        {
                            var data = Database.GetAllQuestions();
                            Datagrid.ItemsSource = data;
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show("Chyba při načítání dat: " + ex.Message, "Chyba");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Chyba při mazání: " + ex.Message, "Chyba");
                    }
                }
            }
            else
            {
                MessageBox.Show("Prosím, označte nejprve řádek, který chcete smazat.", "Upozornění");
            }
        }
    }
    
}
