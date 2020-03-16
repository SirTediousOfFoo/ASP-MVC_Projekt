using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Vjezba.Model
{
    public class Student : Osoba
    {
        private string _jmbag;
        public string JMBAG
        {
            get => _jmbag;
            set
            {
                if (!Regex.IsMatch($"{value}", "[0-9]{10}"))
                    throw new InvalidOperationException();

                _jmbag = value;
            }
        }

        public decimal Prosjek { get; set; }
        public int BrPolozeno { get; set; }
        public int ECTS { get; set; }
    }
}
