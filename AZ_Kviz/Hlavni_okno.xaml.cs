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
    /// Interakční logika pro Hlavni_okno.xaml
    /// </summary>
    public partial class Hlavni_okno : Window
    {
        public Hlavni_okno()
        {
            InitializeComponent();
            SwitchView(new Menu());
        }

        public void SwitchView(UserControl newView)
        {
            MainContent.Content = newView;
        }

        // Kliknití ESC
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (MainContent.Content is Hra)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Opravdu chcete ukončit rozehranou hru?",
                        "Ukončení hry",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        SwitchView(new Menu());
                    }
                    e.Handled = true;
                }
                else if (MainContent.Content is Nastaveni)
                {
                    SwitchView(new Menu());
                    e.Handled = true;
                }
                else if (MainContent.Content is Databaze_editor)
                {
                    SwitchView(new Nastaveni());
                    e.Handled = true;
                }
                else if (MainContent.Content is Databaze2_editor)
                {
                    SwitchView(new Nastaveni());
                    e.Handled = true;
                }
                else if (MainContent.Content is Nastaveni_hracu)
                {
                    SwitchView(new Menu());
                    e.Handled = true;
                }
                else if (MainContent.Content is Menu)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Opravdu chcete ukončit aplikaci?",
                        "Ukončení aplikace",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes)
                    {
                        Application.Current.Shutdown();
                    }
                    e.Handled = true;
                }
            }
        }
    }
}