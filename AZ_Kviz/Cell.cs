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
    public enum CellState
    {
        Free,
        Player1,
        Player2
    }

    public class Cell
    {
        public int Id { get; private set; }
        public CellState State { get; private set; }
        public Button Button { get; private set; }

        public Cell(int id)
        {
            Id = id;
            Button = CreateButton();
            State = CellState.Free;
        }

        private Button CreateButton()
        {
            Button button = new Button
            {
                Width = 60,
                Height = 60,
            };
            button.Click += (s, e) => OnClick();

            // Šestihran
            ControlTemplate template = new ControlTemplate(typeof(Button));
            FrameworkElementFactory grid = new FrameworkElementFactory(typeof(Grid));

            FrameworkElementFactory polygon = new FrameworkElementFactory(typeof(Polygon));
            polygon.SetValue(Polygon.NameProperty, "HexPolygon");
            polygon.SetValue(Polygon.PointsProperty, PointCollection.Parse("30,0 60,15 60,45 30,60 0,45 0,15"));
            polygon.SetValue(Polygon.FillProperty, Brushes.LightGray);
            polygon.SetValue(Polygon.StrokeProperty, Brushes.Black);
            polygon.SetValue(Polygon.StrokeThicknessProperty, 1.0);
            grid.AppendChild(polygon);

            FrameworkElementFactory content = new FrameworkElementFactory(typeof(ContentPresenter));
            content.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            content.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            content.SetValue(TextBlock.FontSizeProperty, 16.0);
            content.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            grid.AppendChild(content);

            template.VisualTree = grid;
            button.Template = template;

            return button;
        }

        private void OnClick()
        {
            Random rnd = new Random();
            var question = Database.GetQuestionById(rnd.Next(1, 6));
            MessageBox.Show(question.Value.Otazka.ToString());
        }

        public void SetNumber(string number)
        {
            Button.Content = number;
        }

        public void SetState(CellState newState)
        {
            State = newState;

            if (Button.Template.FindName("HexPolygon", Button) is Polygon poly)
            {
                switch (State)
                {
                    case CellState.Player1:
                        poly.Fill = Brushes.LightBlue;
                        break;
                    case CellState.Player2:
                        poly.Fill = Brushes.LightCoral;
                        break;
                    default:
                        poly.Fill = Brushes.White;
                        break;
                }
            }
        }
    }
}
