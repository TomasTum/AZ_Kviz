using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// Interakční logika pro Upravit2_otazku.xaml
    /// </summary>
    public partial class Upravit2_otazku : Window
    {

        (string Otazka, string Odpoved, string Kategorie)? question;
        int id;

        public Upravit2_otazku(int index)
        {
            InitializeComponent();

            id = index;
            question = Database.GetSubQuestionById(index);
            if (question.HasValue)
            {
                textbox1.Text = question.Value.Otazka;
                combobox2.Text = question.Value.Odpoved;
                combobox1.Text = question.Value.Kategorie;
            }
            else
            {
                MessageBox.Show("Otázka s daným ID nebyla nalezena.", "Chyba");
                Close();
            }
        }

        // Konec
        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // Upravení otázky
        private async void Upravit_Click(object sender, RoutedEventArgs e)
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

                    // Přidání otazníku
                    if (!otazka.EndsWith("?"))
                    {
                        otazka += "?";
                    }

                    Database.UpdateSubQuestion(id, otazka, odpoved, kategorie);
                    labelvysledek.Content = "Otázka byla úspěšně upravena.";
                    labelvysledek.Background = Brushes.LightGreen;
                    textbox1.Clear();
                    await Task.Delay(1000);
                    Close();
                }
                catch (Exception ex)
                {
                    labelvysledek.Content = "Chyba při úpravě otázky: " + ex.Message;
                    labelvysledek.Background = Brushes.Red;
                }
            }
        }

        // Posouvání okna
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        // Kluknití ESC
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}