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
        public int Id { get; set; }
        public CellState State { get; set; }
        public Button Button { get; set; }

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
                Margin = new Thickness(5),
                
            };

            // Vytvoření šestihranu
            Polygon polygon = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(30,0),
                    new Point(60,15),
                    new Point(60,45),
                    new Point(30,60),
                    new Point(0,45),
                    new Point(0,15)
                },
                Fill = Brushes.LightGray,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            // Číslo uprostřed
            TextBlock text = new TextBlock
            {
                Text = Id.ToString(),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };


            ControlTemplate controlTemplate = new ControlTemplate(typeof(Button));
            Grid grid = new Grid();

            grid.Children.Add(polygon);
            grid.Children.Add(text);

            //controlTemplate.VisualTree = grid;




            button.Content = grid;
            button.Click += (s, e) => OnClick();

           
            return button;
        }



        private void OnClick()
        {
            Random rnd = new Random();
            var question = Database.GetQuestionById(rnd.Next(1, 6));
            MessageBox.Show(question.Value.Otazka.ToString(),"Políčko: " + Id);
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
