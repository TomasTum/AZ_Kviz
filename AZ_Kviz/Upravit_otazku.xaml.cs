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
    /// Interakční logika pro Upravit_otazku.xaml
    /// </summary>
    public partial class Upravit_otazku : Window
    {
        private (string Otazka, string Odpoved, string Zkratka, string Kategorie)? question;
        int id;

        public Upravit_otazku(int index)
        {
            InitializeComponent();

            id = index;
            question = Database.GetQuestionById(index);
            if (question.HasValue)
            {
                textbox1.Text = question.Value.Otazka;
                textbox2.Text = question.Value.Odpoved;
                combobox1.Text = question.Value.Kategorie;
            }
            else
            {
                MessageBox.Show("Otázka s daným ID nebyla nalezena.", "Chyba");
                this.Close();
            }
        }

        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Upravit_Click(object sender, RoutedEventArgs e)
        {
            string otazka = textbox1.Text.Trim();
            string odpoved = textbox2.Text.Trim();
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

                    // Přidání otazníku na konec otázky, pokud tam není
                    if (!otazka.EndsWith("?"))
                    {
                        otazka += "?";
                    }

                    // Vytvoření zkratky z první písmen každého slova v odpovědi
                    string[] slova = odpoved.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    string zkratka = "";

                    foreach (string slovo in slova)
                    {
                        zkratka += char.ToUpper(slovo[0]);
                    }

                    Database.UpdateQuestion(id, otazka, odpoved, zkratka, kategorie);
                    labelvysledek.Content = "Otázka byla úspěšně upravena.";
                    labelvysledek.Background = Brushes.LightGreen;
                    textbox1.Clear();
                    textbox2.Clear();
                    await Task.Delay(1000);
                    this.Close();
                }
                catch (Exception ex)
                {
                    labelvysledek.Content = "Chyba při úpravě otázky: " + ex.Message;
                    labelvysledek.Background = Brushes.Red;
                }
            }
        }
    }
}
