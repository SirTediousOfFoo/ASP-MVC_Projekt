using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Vjezba.Model
{
    public class Osoba
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }

        private string _oib;
        public string OIB
        {
            get => this._oib;
            set
            {
                if (!Regex.IsMatch($"{value}", "[0-9]{11}"))
                    throw new InvalidOperationException();

                this._oib = value;
            }
        }

        private string _jmbg;
        public string JMBG 
        { 
            get => _jmbg; 
            set
            {
                if (!Regex.IsMatch($"{value}", "[0-9]{13}"))
                    throw new InvalidOperationException();

                _jmbg = value;
            }
        }

        public DateTime DatumRodjenja
        {
            get
            {
                var year = 1000 + int.Parse(JMBG.Substring(4, 3));
                if (year < 1100)
                    year += 1000;

                return new DateTime(year, int.Parse(JMBG.Substring(2, 2)), int.Parse(JMBG.Substring(0, 2)));
            }
        }
    }
}
