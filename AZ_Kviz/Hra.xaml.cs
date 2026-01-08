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
using System.Globalization;

namespace AZ_Kviz
{
    /// <summary>
    /// Interakční logika pro Hra.xaml
    /// </summary>
    public partial class Hra : Window
    {
        private Board board;
        private Cell activeCell;
        private (string Otazka, string Odpoved, string Zkratka)? currentQuestion;
        private bool isQuestionActive = false;

        // Seznam použitých otázek (ID)
        private HashSet<int> usedQuestionIds = new HashSet<int>();
        // Seznam všech dostupných otázek (ID)
        private List<int> allAvailableIds = new List<int>();       
        private Random rnd = new Random();

        public Hra()
        {
            InitializeComponent();
            board = new Board(GameBoard);
            board.OnCellClicked += Board_OnCellClicked;
            board.GenerateBoard();
            LoadAllQuestionIds();
        }

        // Načtení všech ID otázek z databáze
        private void LoadAllQuestionIds()
        {
            allAvailableIds = Database.GetAllQuestions().Select(q => q.Id).ToList();
        }

        private void Board_OnCellClicked(Cell clickedCell)
        {
            // Kontrola, nepoužitých otázek
            if (usedQuestionIds.Count >= allAvailableIds.Count)
            {
                MessageBox.Show("Došly otázky v databázi!", "Konec otázek");
                return;
            }

            if (isQuestionActive) return;
            isQuestionActive = true;

            activeCell = clickedCell;

            int randomId;

            // Generování náhodného ID otázky, která ještě nebyla použita
            do
            {
                int index = rnd.Next(0, allAvailableIds.Count);
                randomId = allAvailableIds[index];
            }
            while (usedQuestionIds.Contains(randomId));

            // Přidání vybrané ID do seznamu použitých
            usedQuestionIds.Add(randomId);

            // Načtení konkrétní otázky z DB
            currentQuestion = Database.GetQuestionById(randomId);

            // Zobrazení UI
            if (currentQuestion.HasValue)
            {
                isQuestionActive = true;
                TxtQuestion.Text = currentQuestion.Value.Otazka;
                TxtHint.Text = currentQuestion.Value.Zkratka;
                TxtAnswer.Text = ""; // Vyčistit textbox
                QuestionArea.Visibility = Visibility.Visible; // Ukázat panel
                TxtAnswer.Focus(); // Nastavit kurzor do pole
            }
        }

        //kontrola odpovědi
        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (!currentQuestion.HasValue) return;

            //odstranění diakritiky
            string cleanUserAnswer = RemoveDiacritics(TxtAnswer.Text.Trim());
            string cleanCorrectAnswer = RemoveDiacritics(currentQuestion.Value.Odpoved.Trim());

            // porovnání odpovědí
            if (cleanUserAnswer == cleanCorrectAnswer)
            {
                // SPRÁVNĚ
                activeCell.Button.Background = Brushes.Orange;
                activeCell.State = CellState.Player1;
                TxtAnswer.Background = Brushes.LightGreen;

            }
            else
            {
                // ŠPATNĚ
                activeCell.Button.Background = Brushes.Gray;
                activeCell.State = CellState.Black;
                TxtAnswer.Background = Brushes.IndianRed;
            }

            await Task.Delay(1000);

            // Vyčistit a skrýt panel
            QuestionArea.Visibility = Visibility.Collapsed;
            TxtAnswer.Background = Brushes.White;
            TxtAnswer.Clear();
            currentQuestion = null;
            isQuestionActive = false;
            Cell.IsAnyCellActive = false;
        }


        //odstranění diakritiky z textu
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Rozloží znaky na základní písmeno + diakritické znaménko (FormD)
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                // Pokud znak není diakritické znaménko (NonSpacingMark), přidáme ho
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Vrátíme text zpět do kompaktní podoby a převedeme na malá písmena pro snadné porovnání
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }
    }
}
