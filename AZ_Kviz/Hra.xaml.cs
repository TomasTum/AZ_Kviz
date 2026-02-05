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
        private (string Otazka, string Odpoved, string Zkratka, string Kategorie)? currentQuestion;
        private bool isQuestionActive = false;

        private Player player1;
        private Player player2;
        private Player currentPlayer; // Aktuální hráč na tahu

        // Seznam všech dostupných otázek (ID)
        private List<int> allAvailableIds = new List<int>();
        
        private Random rnd = new Random();

        string selectedCategory;

        public Hra(Player player1, Player player2, string kategorie)
        {
            InitializeComponent();

            this.player1 = player1;
            this.player2 = player2;
            this.currentPlayer = player1; // Začíná hráč 1
            // Nastavení vizuální indikace tahu
            UpdateTurnVisuals();

            //Kategorie otázek pro hru
            selectedCategory = kategorie;

            TxtPlayer1Name.Text = player1.Name;
            TxtPlayer2Name.Text = player2.Name;

            board = new Board(GameBoard);
            board.OnCellClicked += Board_OnCellClicked;
            board.GenerateBoard();

            LoadAllQuestionIds();
        }

        // Načtení všech ID otázek z databáze
        private void LoadAllQuestionIds()
        {
            allAvailableIds = Database.GetAllQuestions(selectedCategory).Select(q => q.Id).ToList();
        }

        private void Board_OnCellClicked(Cell clickedCell)
        {
            // Kontrola, nepoužitých otázek
            if (allAvailableIds.Count == 0)
            {
                MessageBox.Show("Došly otázky v databázi!", "Konec otázek");
                return;
            }

            if (isQuestionActive) return;
            isQuestionActive = true;

            activeCell = clickedCell;

            int randomId;

            // Generování náhodného ID otázky, která je v seznamu dostupných a nebyla použita
            int index = rnd.Next(0, allAvailableIds.Count);
            randomId = allAvailableIds[index];

            // Načtení konkrétní otázky z DB
            currentQuestion = Database.GetQuestionById(randomId);

            // Smazání použitého ID z dostupných otázek
            allAvailableIds.Remove(randomId);

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
            // Pokud není aktivní otázka, nic se neděje
            if (!currentQuestion.HasValue) return;

            // Kliknutí jen jednou, zamezení opakovanému klikání
            BtnSubmit.IsEnabled = false;

            //odstranění diakritiky
            string cleanUserAnswer = RemoveDiacritics(TxtAnswer.Text.Trim());
            string cleanCorrectAnswer = RemoveDiacritics(currentQuestion.Value.Odpoved.Trim());

            int delay;
            // porovnání odpovědí
            if (cleanUserAnswer == cleanCorrectAnswer)
            {
                // SPRÁVNĚ
                activeCell.SetState(currentPlayer.State, currentPlayer.PlayerColor);
                TxtAnswer.Background = Brushes.LightGreen;
                delay = 1000;
            }
            else
            {
                // ŠPATNĚ
                activeCell.SetState(CellState.Black, Brushes.Gray);
                TxtAnswer.Background = Brushes.IndianRed;
                // Zobrazení správné odpovědi
                TxtCorrectAnswer.Text = $"Správná odpověď: {currentQuestion.Value.Odpoved}";
                TxtCorrectAnswer.Visibility = Visibility.Visible;
                delay = 3000;
            }

            await Task.Delay(delay);

            // Vyčistit a skrýt panel
            QuestionArea.Visibility = Visibility.Collapsed;
            TxtAnswer.Background = Brushes.White;
            TxtAnswer.Clear();
            TxtCorrectAnswer.Text = "";
            TxtCorrectAnswer.Visibility = Visibility.Collapsed;
            currentQuestion = null;
            isQuestionActive = false;
            Cell.IsAnyCellActive = false;
            BtnSubmit.IsEnabled = true;

            // Střídání hráčů
            currentPlayer = (currentPlayer == player1) ? player2 : player1;
            UpdateTurnVisuals();
        }


        // Odstranění diakritiky z textu
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Rozloží znaky na základní písmeno + diakritické znaménko (FormD)
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                // Pokud znak není diakritické znaménko (NonSpacingMark), přidá se
                UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Vrátíme text zpět do kompaktní podoby a převedeme na malá písmena pro snadné porovnání
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }

        // Aktualní hrač na tahu - vizuální indikace
        private void UpdateTurnVisuals()
        {
            // Definice efektu záře
            var glowEffect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            if (currentPlayer == player1)
            {
                // HRÁČ 1 JE NA TAHU
                Player1Panel.Opacity = 1.0; // Plná viditelnost
                glowEffect.Color = Colors.Orange; // Oranžová záře
                Player1Panel.Effect = glowEffect;

                // HRÁČ 2 ČEKÁ
                Player2Panel.Opacity = 0.4; // Ztlumení
                Player2Panel.Effect = null; // Žádná záře

                // Změna barvy tlačítka pro odpověď na barvu hráče 1
                BtnSubmit.Background = Brushes.Orange;
            }
            else
            {
                // HRÁČ 2 JE NA TAHU
                Player2Panel.Opacity = 1.0;
                glowEffect.Color = Colors.DeepSkyBlue; // Modrá záře
                Player2Panel.Effect = glowEffect;

                // HRÁČ 1 ČEKÁ
                Player1Panel.Opacity = 0.4;
                Player1Panel.Effect = null;

                // Změna barvy tlačítka pro odpověď na barvu hráče 2
                BtnSubmit.Background = Brushes.DeepSkyBlue;
            }
        }
    }
}
