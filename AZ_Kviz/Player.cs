using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AZ_Kviz
{
    public class Player
    {
        public string Name { get; set; }
        public Brush PlayerColor { get; set; }
        public CellState State { get; set; }

        public Player(string name, Brush color, CellState state)
        {
            Name = name;
            PlayerColor = color;
            State = state;
        }
    }
}