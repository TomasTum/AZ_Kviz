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
    /// Interakční logika pro Pridat2_otazku.xaml
    /// </summary>
    public partial class Pridat2_otazku : Window
    {
        public Pridat2_otazku()
        {
            InitializeComponent();
        }

        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Pridat_Click(object sender, RoutedEventArgs e)
        {
            string otazka = textbox1.Text.Trim();
            string odpoved = combobox2.Text.Trim();
            string kategorie = combobox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(otazka) || string.IsNullOrWhiteSpace(odpoved) || string.IsNullOrWhiteSpace(kategorie))
            {
                MessageBox.Show("Otázka, správná odpověď a kategorie nesmí být prázdné.", "Chyba");
            }
            else
            {
                try
                {
                    // První písmeno otázky, odpovědi velké
                    otazka = char.ToUpper(otazka[0]) + otazka.Substring(1);
                    odpoved = char.ToUpper(odpoved[0]) + odpoved.Substring(1);
                    if (!string.IsNullOrWhiteSpace(kategorie)) kategorie = char.ToUpper(kategorie[0]) + kategorie.Substring(1);

                    //Přidání otazníku na konec otázky, pokud tam není
                    if (!otazka.EndsWith("?"))
                    {
                        otazka += "?";
                    }

                    if (Database.GetSubQuestionByQuestion(otazka))
                    {
                        labelvysledek.Content = "Tato otázka již existuje.";
                        labelvysledek.Background = Brushes.Red;
                    }
                    else
                    {
                        Database.AddSubQuestion(otazka, odpoved, kategorie);
                        labelvysledek.Content = "Otázka byla úspěšně přidána.";
                        labelvysledek.Background = Brushes.LightGreen;
                        textbox1.Clear();
                    }


                }
                catch (Exception ex)
                {
                    labelvysledek.Content = "Chyba při přidávání otázky: " + ex.Message;
                    labelvysledek.Background = Brushes.Red;
                }
            }
        }

        // Posouvání okna
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }
    }
}
