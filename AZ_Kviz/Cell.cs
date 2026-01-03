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

        public Cell(int id)
        {
            Id = id;
            Button = CreateButton();
            State = CellState.Free;
        }

        FrameworkElementFactory polygonF;

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


            //polygonF.SetValue(Polygon.FillProperty, Brushes.White); nahrazeno níže
            var fillBinding = new Binding("Background")
            {
                RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent)
            };
            polygonF.SetBinding(Polygon.FillProperty, fillBinding);

            // nastavit implicitní background
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



        private void OnClick()
        {
            Button.Background = Brushes.LightBlue;

            // Místo MessageBoxu vyvoláme událost
            // Předáme sami sebe (this), aby okno vědělo, o které políčko jde
            Clicked?.Invoke(this);

        }

        public void SetNumber(string number)
        {
            Button.Content = number;
        }

        public void SetState(CellState newState)
        {
            State = newState;

        }
    }
}
