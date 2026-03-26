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
    public partial class Databaze_editor : UserControl
    {
        public Databaze_editor()
        {
            InitializeComponent();
        }

        // Načtení dat
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateData();

                // Úprava zakladního chování DataGridu
                Datagrid.CanUserDeleteRows = false;
                Datagrid.PreviewKeyDown += Datagrid_PreviewKeyDown;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat: " + ex.Message, "Chyba");
            }
        }

        // Konec
        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
            mainWin.SwitchView(new Nastaveni());
        }

        // Odebrat otázku
        private void Odebrat_Click(object sender, RoutedEventArgs e)
        {
            DeleteQuestion();
        }

        // Kliknutí na klávesu
        private void Datagrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                DeleteQuestion();
                e.Handled = true;
            }
        }

        // Přidat otázku
        private void Pridat_Click(object sender, RoutedEventArgs e)
        {
            Pridat_otazku pridat_Otazku = new Pridat_otazku();
            pridat_Otazku.ShowDialog();
            UpdateData();
        }

        // Aktualizovat DataGrid
        private void Aktualizovat_Click(object sender, RoutedEventArgs e)
        {
            UpdateData();
        }

        // Upravit otázku
        private void Upravit_Click(object sender, RoutedEventArgs e)
        {
            if (Datagrid.SelectedItem is Question vybranyRadek)
            {
                Upravit_otazku upravit_Otazku = new Upravit_otazku(vybranyRadek.Id);
                upravit_Otazku.ShowDialog();
                UpdateData();
            }
            else
            {
                MessageBox.Show("Prosím, označte nejprve řádek, který chcete upravit.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Aktualizace dat DataGridu
        private void UpdateData()
        {
            List<Question> data = Database.GetAllQuestions();
            Datagrid.ItemsSource = data;
        }

        // Smazání otázky
        private void DeleteQuestion()
        {
            if (Datagrid.SelectedItem is Question vybranyRadek)
            {
                MessageBoxResult result = MessageBox.Show($"Chcete opravdu smazat otázku: {vybranyRadek.Otazka}", "Upozornění", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        Database.DeleteQuestion(vybranyRadek.Id);
                        try
                        {
                            UpdateData();
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
                MessageBox.Show("Prosím, označte nejprve řádek, který chcete smazat.", "Upozornění", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}