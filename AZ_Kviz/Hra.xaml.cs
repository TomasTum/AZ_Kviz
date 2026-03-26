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
    public partial class Hra : UserControl
    {
        private Board board;
        private Cell activeCell;
        private (string Otazka, string Odpoved, string Zkratka, string Kategorie)? currentQuestion;
        private (string Otazka, string Odpoved, string Kategorie)? currentSubQuestion;
        private bool isQuestionActive = false;
        private string selectedCategory;
        private Random rnd = new Random();

        // Hráči
        private Player player1;
        private Player player2;
        private Player currentPlayer;

        // Seznam všech dostupných otázek (ID)
        private List<int> allAvailableQuestions = new List<int>();
        private List<int> allAvailableSubQuestions = new List<int>();

        // Propojené políčka
        private List<Cell> connectedCells = new List<Cell>();

        public Hra(Player player1, Player player2, string kategorie)
        {
            InitializeComponent();

            this.player1 = player1;
            this.player2 = player2;
            // Začíná hráč 1
            currentPlayer = player1;
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

        // Načtení všech ID otázek
        private void LoadAllQuestionIds()
        {
            allAvailableQuestions = Database.GetAllQuestions(selectedCategory).Select(q => q.Id).ToList();
            allAvailableSubQuestions = Database.GetAllSubQuestions(selectedCategory).Select(q => q.Id).ToList();
        }

        // Kliknutí na políčko
        private void Board_OnCellClicked(Cell clickedCell)
        {

            // Kontrola, zda již není aktivní otázka
            if (isQuestionActive) return;

            isQuestionActive = true;
            activeCell = clickedCell;
            int randomId;

            if (activeCell.State == CellState.Black)
            {
                // Generování náhodného ID otázky, která je v seznamu dostupných a nebyla použita
                int index = rnd.Next(0, allAvailableSubQuestions.Count);
                randomId = allAvailableSubQuestions[index];

                // Načtení konkrétní otázky z DB
                currentSubQuestion = Database.GetSubQuestionById(randomId);

                // Smazání použitého ID z dostupných otázek
                allAvailableSubQuestions.Remove(randomId);

                // Zobrazení UI
                if (currentSubQuestion.HasValue)
                {
                    isQuestionActive = true;
                    TxtSubQuestion.Text = currentSubQuestion.Value.Otazka;
                    SubQuestionArea.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Generování náhodného ID otázky, která je v seznamu dostupných a nebyla použita
                int index = rnd.Next(0, allAvailableQuestions.Count);
                randomId = allAvailableQuestions[index];

                // Načtení konkrétní otázky z DB
                currentQuestion = Database.GetQuestionById(randomId);

                // Smazání použitého ID z dostupných otázek
                allAvailableQuestions.Remove(randomId);

                // Zobrazení UI
                if (currentQuestion.HasValue)
                {
                    isQuestionActive = true;
                    TxtQuestion.Text = currentQuestion.Value.Otazka;
                    TxtHint.Text = currentQuestion.Value.Zkratka;
                    TxtAnswer.Text = "";
                    QuestionArea.Visibility = Visibility.Visible;
                    TxtAnswer.Focus();
                }
            }
        }

        // Kontrola odpovědi
        private async void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Kontrola aktivní otázky
            if (!currentQuestion.HasValue) return;

            // Kliknutí jen jednou
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
                TxtCorrectAnswer.Text = $"Správná odpověď: {currentQuestion.Value.Odpoved}";
                TxtCorrectAnswer.Visibility = Visibility.Visible;
                delay = 3000;
            }

            await Task.Delay(delay);

            // Srytí UI
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

                // Reset pro novou hru
                isQuestionActive = false;
                Cell.IsAnyCellActive = false;

                await Task.Delay(2000);

                string message = $"Vítězem se stává {currentPlayer.Name}!";
                Window currentWindow = Window.GetWindow(this);
                Konec_hry konec_hry = new Konec_hry(message)
                {
                    Owner = currentWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                konec_hry.ShowDialog();

                return;
            }

            // Střídání hráčů
            currentPlayer = (currentPlayer == player1) ? player2 : player1;
            UpdateTurnVisuals();
        }

        // Odpověď ANO
        private async void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            // Kontrola aktivní otázky
            if (!currentSubQuestion.HasValue) return;

            // Kliknutí jen jednou
            BtnYes.IsEnabled = false;
            BtnNo.IsEnabled = false;

            int delay;

            if (currentSubQuestion.Value.Odpoved == "ANO")
            {
                // SPRÁVNĚ
                activeCell.SetState(currentPlayer.State, currentPlayer.PlayerColor);

                delay = 1000;

                // Kontrola vítěze
                if (CheckWinner(currentPlayer.State, out connectedCells))
                {
                    SubQuestionArea.Visibility = Visibility.Collapsed;
                    WinningCells(connectedCells);

                    // Reset pro novou hru
                    isQuestionActive = false;
                    Cell.IsAnyCellActive = false;

                    await Task.Delay(2000);

                    string message = $"Vítězem se stává {currentPlayer.Name}!";
                    Window currentWindow = Window.GetWindow(this);
                    Konec_hry konec_hry = new Konec_hry(message)
                    {
                        Owner = currentWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    konec_hry.ShowDialog();

                    return;
                }

                // Střídání hráčů
                currentPlayer = (currentPlayer == player1) ? player2 : player1;
                UpdateTurnVisuals();
            }
            else
            {
                // ŠPATNĚ
                Player opponent = (currentPlayer == player1) ? player2 : player1;
                activeCell.SetState(opponent.State, opponent.PlayerColor);

                delay = 3000;

                // Kontrola vítěze
                if (CheckWinner(opponent.State, out connectedCells))
                {
                    SubQuestionArea.Visibility = Visibility.Collapsed;
                    WinningCells(connectedCells);

                    // Reset pro novou hru
                    isQuestionActive = false;
                    Cell.IsAnyCellActive = false;

                    await Task.Delay(2000);

                    string message = $"Vítězem se stává {opponent.Name}!";
                    Window currentWindow = Window.GetWindow(this);
                    Konec_hry konec_hry = new Konec_hry(message)
                    {
                        Owner = currentWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    konec_hry.ShowDialog();

                    return;
                }

                TxtSubCorrectAnswer.Text = $"Správná odpověď: {currentSubQuestion.Value.Odpoved}";
                TxtSubCorrectAnswer.Visibility = Visibility.Visible;
            }

            await Task.Delay(delay);

            // Srytí UI
            SubQuestionArea.Visibility = Visibility.Collapsed;
            TxtSubCorrectAnswer.Text = "";
            TxtSubCorrectAnswer.Visibility = Visibility.Collapsed;
            currentSubQuestion = null;
            isQuestionActive = false;
            Cell.IsAnyCellActive = false;
            BtnYes.IsEnabled = true;
            BtnNo.IsEnabled = true;
        }

        // Odpověď NE
        private async void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            // Kontrola aktivní otázky
            if (!currentSubQuestion.HasValue) return;

            // Kliknutí jen jednou
            BtnYes.IsEnabled = false;
            BtnNo.IsEnabled = false;

            int delay;

            if (currentSubQuestion.Value.Odpoved == "NE")
            {
                // SPRÁVNĚ
                activeCell.SetState(currentPlayer.State, currentPlayer.PlayerColor);

                delay = 1000;

                // Kontrola vítěze
                if (CheckWinner(currentPlayer.State, out connectedCells))
                {
                    SubQuestionArea.Visibility = Visibility.Collapsed;
                    WinningCells(connectedCells);

                    // Reset pro novou hru
                    isQuestionActive = false;
                    Cell.IsAnyCellActive = false;

                    await Task.Delay(2000);

                    string message = $"Vítězem se stává {currentPlayer.Name}!";
                    Window currentWindow = Window.GetWindow(this);
                    Konec_hry konec_hry = new Konec_hry(message)
                    {
                        Owner = currentWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    konec_hry.ShowDialog();

                    return;
                }

                // Střídání hráčů
                currentPlayer = (currentPlayer == player1) ? player2 : player1;
                UpdateTurnVisuals();
            }
            else
            {
                // ŠPATNĚ
                Player opponent = (currentPlayer == player1) ? player2 : player1;
                activeCell.SetState(opponent.State, opponent.PlayerColor);

                delay = 3000;

                // Kontrola vítěze
                if (CheckWinner(opponent.State, out connectedCells))
                {
                    SubQuestionArea.Visibility = Visibility.Collapsed;
                    WinningCells(connectedCells);

                    // Reset pro novou hru
                    isQuestionActive = false;
                    Cell.IsAnyCellActive = false;

                    await Task.Delay(2000);

                    string message = $"Vítězem se stává {opponent.Name}!";
                    Window currentWindow = Window.GetWindow(this);
                    Konec_hry konec_hry = new Konec_hry(message)
                    {
                        Owner = currentWindow,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    konec_hry.ShowDialog();

                    return;
                }

                TxtSubCorrectAnswer.Text = $"Správná odpověď: {currentSubQuestion.Value.Odpoved}";
                TxtSubCorrectAnswer.Visibility = Visibility.Visible;
            }

            await Task.Delay(delay);

            // Srytí UI
            SubQuestionArea.Visibility = Visibility.Collapsed;
            TxtSubCorrectAnswer.Text = "";
            TxtSubCorrectAnswer.Visibility = Visibility.Collapsed;
            currentSubQuestion = null;
            isQuestionActive = false;
            Cell.IsAnyCellActive = false;
            BtnYes.IsEnabled = true;
            BtnNo.IsEnabled = true;
        }


        // Odstranění diakritiky
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            // Rozložení znaků na základní písmeno + diakritické znaménko (FormD)
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

            // Normalizace zpět do FormC + malá písmena
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
        }

        // Aktualní hrač na tahu - vizuální indikace
        private void UpdateTurnVisuals()
        {
            // Záře
            System.Windows.Media.Effects.DropShadowEffect glowEffect = new System.Windows.Media.Effects.DropShadowEffect
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
                Player2Panel.Opacity = 0.4;
                Player2Panel.Effect = null;

                // Barva tlačítka pro odpověď
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

                // Barva tlačítka pro odpověď
                BtnSubmit.Background = Brushes.DeepSkyBlue;
            }
        }

        // Zvýraznění vítězných políček
        private void WinningCells(List<Cell> cells)
        {
            // Záře
            System.Windows.Media.Effects.DropShadowEffect glowEffect = new System.Windows.Media.Effects.DropShadowEffect
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
                System.Windows.Media.Animation.DoubleAnimation animation = new System.Windows.Media.Animation.DoubleAnimation
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

        // Zjištění vítěze - BFS
        private bool CheckWinner(CellState playerState, out List<Cell> connectedCells)
        {
            // Propojené pole
            connectedCells = null;

            List<Cell> playerCells = board.Cells.Where(c => c.State == playerState).ToList();

            // Min 7 polí
            if (playerCells.Count < 7) return false;

            // Navštívené pole
            List<Cell> visited = new List<Cell>();

            foreach (Cell startCell in playerCells)
            {
                // Přeskočení navštívených
                if (visited.Contains(startCell)) continue;

                // Aktuální propojené pole
                List<Cell> currentCells = new List<Cell>();

                // Fronta
                Queue<Cell> queue = new Queue<Cell>();
                queue.Enqueue(startCell);
                visited.Add(startCell);

                // Propojené strany
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

                    // 3 strany = vítěz
                    if (touchesLeft && touchesRight && touchesBottom)
                    {
                        connectedCells = currentCells;
                        return true;
                    }

                    // Nalezení sousedních polí
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


        // Nalezení sousedních polí
        private List<Cell> GetNeighbors(Cell c, List<Cell> playerCells)
        {
            int r = c.Row;
            int col = c.Column;

            // Relativní souřadnice sousedů
            (int dr, int dc)[] directions = new (int dr, int dc)[]
            {
                (0, -1), (0, 1),   // vlevo, vpravo
                (-1, -1), (-1, 0), // nad (vlevo, vpravo)
                (1, 0), (1, 1)     // pod (vlevo, vpravo)
            };

            List<Cell> neighbors = new List<Cell>();

            // Kontrola polí v okolí
            foreach ((int dr, int dc) d in directions)
            {
                Cell n = playerCells.FirstOrDefault(cell => cell.Row == r + d.dr && cell.Column == col + d.dc);
                if (n != null) neighbors.Add(n);
            }
            return neighbors;
        }

        // Kliknití ESC
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MessageBoxResult result = MessageBox.Show("Opravdu chcete ukončit rozehranou hru?", "Ukončení hry", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    Hlavni_okno mainWin = (Hlavni_okno)Window.GetWindow(this);
                    mainWin.SwitchView(new Menu());
                }
            }
        }
    }
}