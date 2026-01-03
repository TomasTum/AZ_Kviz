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

        public List<Cell> Cells { get; set; } = new List<Cell>();

        public event Action<Cell> OnCellClicked;

        public Board(Canvas canvas)
        {
            this.canvas = canvas;
        }

        public void GenerateBoard()
        {
            int number = 1;

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double dynamicStartX = screenWidth / 2.0;

            for (int row = 0; row < TotalRows; row++)
            {
                int cols = row + 1;
                for (int col = 0; col < cols; col++)
                {
                    Cell cell = new Cell(number);
                    cell.SetNumber(number.ToString());

                    // Přihlášení k události Clicked buňky
                    cell.Clicked += (clickCell) => OnCellClicked?.Invoke(clickCell);

                    double offsetX = dynamicStartX - (row * HexWidth / 2.0) + (col * HexWidth) - (HexWidth / 2.0);
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
