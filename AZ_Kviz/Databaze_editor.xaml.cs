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
                // Zavoláme novou metodu z tvé třídy Database
                var data = Database.GetAllQuestions();

                // Naplníme tabulku
                Datagrid.ItemsSource = data;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Chyba při načítání dat: " + ex.Message);
            }
        }

        private void Konec_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
