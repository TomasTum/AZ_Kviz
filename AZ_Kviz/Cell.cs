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


            //// Vytvoření šestihranu
            //Polygon polygon = new Polygon
            //{
            //    Points = new PointCollection
            //    {
            //        new Point(30,0),
            //        new Point(60,15),
            //        new Point(60,45),
            //        new Point(30,60),
            //        new Point(0,45),
            //        new Point(0,15)
            //    },
            //    Fill = Brushes.LightGray,
            //    Stroke = Brushes.Black,
            //    StrokeThickness = 1
            //};

            //// Číslo uprostřed
            //TextBlock text = new TextBlock
            //{
            //    Text = Id.ToString(),
            //    FontSize = 16,
            //    FontWeight = FontWeights.Bold,
            //    HorizontalAlignment = HorizontalAlignment.Center,
            //    VerticalAlignment = VerticalAlignment.Center
            //};


            //Grid grid = new Grid();
            //grid.Children.Add(polygon);
            //grid.Children.Add(text);
            //button.Content = grid;


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
