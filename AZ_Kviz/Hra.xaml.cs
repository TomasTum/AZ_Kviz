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

        // Propojené políčka
        private List<Cell> connectedCells = new List<Cell>();

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
                this.Close();
                return;
            }

            // Kontrola, zda již není aktivní otázka
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

        // Kontrola odpovědi
        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Pokud není aktivní otázka, nic se neděje
            if (!currentQuestion.HasValue) return;

            // Kliknutí jen jednou, zamezení opakovanému klikání
            BtnSubmit.IsEnabled = false;

            // Odstranění diakritiky
            string cleanUserAnswer = RemoveDiacritics(TxtAnswer.Text.Trim());
            string cleanCorrectAnswer = RemoveDiacritics(currentQuestion.Value.Odpoved.Trim());

            int delay;
            // Porovnání odpovědí
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

            // Kontrola vítěze
            if (CheckWinner(currentPlayer.State, out connectedCells))
            {
                WinningCells(connectedCells);
                await Task.Delay(2000); 

                string message = $"Vítězem se stává {currentPlayer.Name}!";
                Konec_hry konec_hry = new Konec_hry(message)
                {
                    Owner = this
                };
                konec_hry.ShowDialog();

                return;
            }

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
                // Hráč 1 je na tahu
                Player1Panel.Opacity = 1.0;
                glowEffect.Color = Colors.Orange;
                Player1Panel.Effect = glowEffect;

                // Hráč 2 čeká
                Player2Panel.Opacity = 0.4; // Ztlumení
                Player2Panel.Effect = null; // Žádná záře

                // Barva tlačítka pro odpověď na barvu hráče 1
                BtnSubmit.Background = Brushes.Orange;
            }
            else
            {
                // Hráč 2 je na tahu
                Player2Panel.Opacity = 1.0;
                glowEffect.Color = Colors.DeepSkyBlue;
                Player2Panel.Effect = glowEffect;

                // Hráč 1 čeká
                Player1Panel.Opacity = 0.4;
                Player1Panel.Effect = null;

                // Barva tlačítka pro odpověď na barvu hráče 2
                BtnSubmit.Background = Brushes.DeepSkyBlue;
            }
        }

        // Zvýraznění vítězných políček
        private void WinningCells(List<Cell> cells)
        {
            // Záře
            var glowEffect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 30,
                ShadowDepth = 0
            };
            foreach (Cell cell in cells)
            {
                if (cell.State == CellState.Player1)
                {
                    glowEffect.Color = Colors.Orange;
                }
                else if (cell.State == CellState.Player2)
                {
                    glowEffect.Color = Colors.DeepSkyBlue;
                }
                cell.Button.Effect = glowEffect;
                // Animace 
                var animation = new System.Windows.Media.Animation.DoubleAnimation
                {
                    From = 1.0,
                    To = 0.5,
                    Duration = TimeSpan.FromSeconds(1.5),
                    AutoReverse = true,
                    RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
                };
                cell.Button.BeginAnimation(Button.OpacityProperty, animation);
            }


        }

        // Zjištění vítěze - BFS pro hledání propojení tří stran
        private bool CheckWinner(CellState playerState, out List<Cell> connectedCells)
        {
            // Propojené pole
            connectedCells = null;

            List<Cell> playerCells = board.Cells.Where(c => c.State == playerState).ToList();
            // Minimálně musí mít 7 políček, aby mohl propojit všechny strany
            if (playerCells.Count < 7) return false;

            // Navštívené pole pro BFS
            List<Cell> visited = new List<Cell>();
            


            foreach (Cell startCell in playerCells)
            {
                // Pokud už tuto buňku navštívil, přeskočí ji
                if (visited.Contains(startCell)) continue;

                // Aktuální propojené pole
                List<Cell> currentCells = new List<Cell>();

                // Fronta pro BFS
                Queue<Cell> queue = new Queue<Cell>();
                queue.Enqueue(startCell);
                visited.Add(startCell);

                // Které strany jsou propojené
                bool touchesLeft = false;
                bool touchesRight = false;
                bool touchesBottom = false;

                while (queue.Count > 0)
                {
                    // Aktuální políčko
                    Cell current = queue.Dequeue();
                    currentCells.Add(current);

                    // Kontrola stran
                    if (current.Column == 0) touchesLeft = true;
                    if (current.Column == current.Row) touchesRight = true;
                    if (current.Row == 6) touchesBottom = true;

                    // Když jsou propojené všechny tři strany je vítěz
                    if (touchesLeft && touchesRight && touchesBottom)
                    {
                        connectedCells = currentCells;
                        return true;
                    }
                        

                    // Nalezení sousedů
                    foreach (Cell neighbor in GetNeighbors(current, playerCells))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
            return false;
        }


        // Nalezení sousedních políček
        private List<Cell> GetNeighbors(Cell c, List<Cell> playerCells)
        {
            int r = c.Row;
            int col = c.Column;

            // Relativní souřadnice 6 sousedů v trojúhelníkové síti
            (int dr, int dc)[] directions = new (int dr, int dc)[]
            {
                (0, -1), (0, 1),   // vlevo, vpravo
                (-1, -1), (-1, 0), // nad (vlevo, vpravo)
                (1, 0), (1, 1)     // pod (vlevo, vpravo)
            };

            List<Cell> neighbors = new List<Cell>();

            // Pro každý směr zkontroluje, zda existuje sousední buňka s danými souřadnicemi a patří hráči
            foreach ((int dr, int dc) d in directions)
            {
                Cell n = playerCells.FirstOrDefault(cell => cell.Row == r + d.dr && cell.Column == col + d.dc);
                if (n != null) neighbors.Add(n);
            }
            return neighbors;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MessageBoxResult result = MessageBox.Show("Opravdu chcete ukončit rozehranou hru?","Ukončení hry",MessageBoxButton.YesNo,MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Menu menu = new Menu();
                    menu.Show();
                    this.Close();
                }
                
            }
        }
    }
}
