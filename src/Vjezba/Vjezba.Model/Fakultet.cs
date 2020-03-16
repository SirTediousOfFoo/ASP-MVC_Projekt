using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vjezba.Model
{
    public class Fakultet
    {
        public List<Osoba> Osobe { get; set; } = new List<Osoba>();

        public int KolikoProfesora() => Osobe.OfType<Profesor>().Count();
        public int KolikoStudenata() => Osobe.OfType<Student>().Count();

        public Student DohvatiStudenta(string jmbag)
            => Osobe.OfType<Student>().FirstOrDefault(s => s.JMBAG == jmbag);

        public IEnumerable<Profesor> DohvatiProfesore()
            => Osobe.OfType<Profesor>().OrderBy(p => p.DatumIzbora);
    }
}
