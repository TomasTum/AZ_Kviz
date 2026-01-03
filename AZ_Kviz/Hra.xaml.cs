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
using Microsoft.Data.Sqlite;

namespace AZ_Kviz
{
    /// <summary>
    /// Interakční logika pro Hra.xaml
    /// </summary>
    public partial class Hra : Window
    {
        private Board board;
        private Cell activeCell;

        public Hra()
        {
            InitializeComponent();
            board = new Board(GameBoard);
            board.OnCellClicked += Board_OnCellClicked;
            board.GenerateBoard();
        }

        private void Board_OnCellClicked(Cell clickedCell)
        {
            activeCell = clickedCell;

            int maxId = Database.GetAllQuestions().Max(q => q.Id);
            Random rnd = new Random();
            var question = Database.GetQuestionById(rnd.Next(1, maxId + 1));

            // Zobrazení v UI
            if (question != null)
            {
                TxtQuestion.Text = question.Value.Otazka;
                TxtAnswer.Text = ""; // Vyčistit předchozí odpověď
                QuestionArea.Visibility = Visibility.Visible; // Ukázat panel
                TxtAnswer.Focus(); // Nastavit kurzor do pole
            }
        }

        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Tady pak budeš kontrolovat správnost odpovědi
            string answer = TxtAnswer.Text;
            // Příklad: Pokud je odpověď správná, obarvíme políčko na oranžovo
            activeCell.Button.Background = Brushes.Orange;
            // Skrýt panel s otázkou
            QuestionArea.Visibility = Visibility.Collapsed;
        }
    }
}
