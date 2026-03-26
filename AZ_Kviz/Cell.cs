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
using System.Windows.Data;
using System.Reflection;

namespace AZ_Kviz
{
    public enum CellState
    {
        Free,
        Player1,
        Player2,
        Black
    }

    public class Cell
    {
        public int Id { get; set; }
        public CellState State { get; set; }
        public Button Button { get; set; }
        public event Action<Cell> Clicked;
        public static bool IsAnyCellActive { get; set; } = false;
        public int Row { get; set; }
        public int Column { get; set; }

        public Cell(int id)
        {
            Id = id;
            Button = CreateButton();
            State = CellState.Free;
        }

        FrameworkElementFactory polygonF;

        // Vytvoření tlačítka
        private Button CreateButton()
        {
            Button button = new Button
            {
                Width = 60,
                Height = 60,
                Margin = new Thickness(5),
                Cursor = System.Windows.Input.Cursors.Hand,
            };

            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

            polygonF = new FrameworkElementFactory(typeof(Polygon));
            polygonF.SetValue(Polygon.NameProperty, "HexPolygon");
            polygonF.SetValue(Polygon.PointsProperty, PointCollection.Parse("30,0 60,15 60,45 30,60 0,45 0,15"));


            Binding fillBinding = new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
            };
            polygonF.SetBinding(Polygon.FillProperty, fillBinding);

            button.Background = Brushes.White;

            polygonF.SetValue(Polygon.StrokeProperty, Brushes.Black);
            polygonF.SetValue(Polygon.StrokeThicknessProperty, 1.0);
            grid.AppendChild(polygonF);

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextBlock.FontSizeProperty, 16.0);
            content.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            grid.AppendChild(content);

            template.VisualTree = grid;
            button.Template = template;
            button.Click += (s, e) => OnClick();

            return button;
        }

        // Kliknutí na tlačítko
        private void OnClick()
        {
            if (IsAnyCellActive || (State != CellState.Free && State != CellState.Black)) return;

            IsAnyCellActive = true;
            Button.Background = Brushes.LightBlue;

            // Vyvolání událost kliknutí
            Clicked?.Invoke(this);
        }

        // Nastavení čísla na tlačítko
        public void SetNumber(string number)
        {
            Button.Content = number;
        }

        // Nastavení políčka (satv, barva)
        public void SetState(CellState newState, Brush color)
        {
            this.State = newState;
            this.Button.Background = color;

            // Vypnutí ruky
            if (newState == CellState.Player1 || newState == CellState.Player2)
            {
                Button.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }
    }
}