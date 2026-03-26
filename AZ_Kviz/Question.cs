using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace AZ_Kviz
{
    public class Question
    {
        public int Id { get; set; }
        public string Otazka { get; set; }
        public string SpravnaOdpoved { get; set; }
        public string Zkratka { get; set; }
        public string Kategorie { get; set; }
    }
}