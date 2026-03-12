using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AZ_Kviz
{
    public class Board
    {
        private Canvas canvas;
        private int TotalRows = 7;
        private double HexWidth = 60;
        private double HexHeight = 52;
        private double StartX = 350;
        private double StartY = 40;
        private double Gap = 2.0;

        // Seznam všech buněk na desce
        public List<Cell> Cells { get; set; } = new List<Cell>();
        // Událost pro kliknutí na buňku
        public event Action<Cell> OnCellClicked;

        public Board(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void GenerateBoard()
        {
            int number = 1;

            // Nastavení pozice pro první buňku (horní řada)
            // Pokud není v XAML nastavena, použije se jako základ 500
            double canvasWidth = double.IsNaN(canvas.Width) ? 500 : canvas.Width;

            // 2. Startovní X bude přesná polovina šířky Canvasu
            StartX = canvasWidth / 2.0;

            // Vytvoření šestiúhelníkové mřížky
            for (int row = 0; row < TotalRows; row++)
            {
                int cols = row + 1;
                for (int col = 0; col < cols; col++)
                {
                    Cell cell = new Cell(number);
                    cell.SetNumber(number.ToString());
                    cell.Row = row;    
                    cell.Column = col;  
                    cell.Clicked += (clickCell) => OnCellClicked?.Invoke(clickCell);

                    // Pozice
                    // CenterStartX nás hodí doprostřed Canvasu
                    // (row * HexWidth / 2.0) posouvá celou řadu doleva podle toho, jak je dlouhá
                    // (col * HexWidth) posouvá konkrétní políčko v řadě doprava
                    // (HexWidth / 2.0) je drobná korekce na střed samotného šestiúhelníku
                    double offsetX = StartX - (row * (HexWidth + Gap) / 2.0) + (col * (HexWidth + Gap)) - (HexWidth / 2.0);

                    // StartY - jak vysoko pod horním okrajem pyramida začne
                    double offsetY = StartY + row * (HexHeight - 5);

                    Canvas.SetLeft(cell.Button, offsetX);
                    Canvas.SetTop(cell.Button, offsetY);


                    canvas.Children.Add(cell.Button);
                    Cells.Add(cell);
                    number++;
                }
            }
        }
    }
}
