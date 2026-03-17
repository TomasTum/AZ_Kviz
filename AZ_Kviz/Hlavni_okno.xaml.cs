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
    }
}
