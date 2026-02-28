using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AZ_Kviz
{
    /// <summary>
    /// Interakční logika pro Pridat_otazku.xaml
    /// </summary>
    public partial class Pridat_otazku : Window
    {
        public Pridat_otazku()
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

                    //Přidání otazníku na konec otázky, pokud tam není
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

                    if (Database.GetQuestionByQuestion(otazka))
                    {
                        labelvysledek.Content = "Tato otázka již existuje.";
                        labelvysledek.Background = Brushes.Red;
                    }
                    else
                    {
                        Database.AddQuestion(otazka, odpoved, zkratka, kategorie);
                        labelvysledek.Content = "Otázka byla úspěšně přidána.";
                        labelvysledek.Background = Brushes.LightGreen;
                        textbox1.Clear();
                        textbox2.Clear();
                    }

                    
                }
                catch (Exception ex)
                {
                    labelvysledek.Content = "Chyba při přidávání otázky: " + ex.Message;
                    labelvysledek.Background = Brushes.Red;
                }
            }
        }
    }
}
