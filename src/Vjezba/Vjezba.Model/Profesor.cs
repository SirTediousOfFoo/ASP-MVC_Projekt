using System;
using System.Collections.Generic;
using System.Text;

namespace Vjezba.Model
{
    public class Profesor : Osoba
    {
        public string Odjel { get; set; }
        public Zvanje Zvanje { get; set; }
        public DateTime DatumIzbora { get; set; }

        public int KolikoDoReizbora()
        {
            var next = DatumIzbora.AddYears(Zvanje == Zvanje.Asistent ? 4 : 5);
            return (int)((next - DateTime.Now).TotalDays / 365.0);
        }
    }

    public enum Zvanje
    {
        Asistent, Predavac, VisiPredavac, ProfVisokeSkole
    }
}
