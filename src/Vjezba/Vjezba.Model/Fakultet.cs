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

        public IEnumerable<Student> DohvatiStudente91()
            => Osobe.OfType<Student>().Where(p => p.DatumRodjenja.Year > 1991);

        public IEnumerable<Student> DohvatiStudente91NoLinq()
        {
            var result = new List<Student>();
            foreach(var o in Osobe)
            {
                var s = o as Student;
                if (s != null && s.DatumRodjenja.Year > 1991)
                    result.Add(s);
            }
            return result;
        }

        public IEnumerable<Student> StudentiNeTvzD()
            => Osobe.OfType<Student>().Where(p => !($"{p.JMBAG}".StartsWith("0246")) && $"{p.Prezime}".StartsWith("D"));

        public List<Student> DohvatiStudente91List()
            => Osobe.OfType<Student>().Where(p => p.DatumRodjenja.Year > 1991).ToList();

        public Student NajboljiProsjek(int god) => Osobe.OfType<Student>()
            .Where(p => p.DatumRodjenja.Year == god)
            .OrderByDescending(p => p.Prosjek)
            .FirstOrDefault();

        public IEnumerable<Student> StudentiGodinaOrdered(int god) => Osobe.OfType<Student>()
            .Where(p => p.DatumRodjenja.Year == god)
            .OrderByDescending(p => p.Prosjek);

        public IEnumerable<Profesor> SviProfesori(bool asc)
        {
            var query = this.Osobe.OfType<Profesor>()
                .OrderBy(p => p.Prezime)
                .ThenBy(p => p.Ime);

            return asc ? query : query.Reverse();
        }

        public int KolikoProfesoraUZvanju(Zvanje zvanje) => Osobe.OfType<Profesor>()
            .Count(p => p.Zvanje == zvanje);

        public IEnumerable<Profesor> NeaktivniProfesori(int x) => Osobe.OfType<Profesor>()
            .Where(p => p.Zvanje == Zvanje.Predavac || p.Zvanje == Zvanje.VisiPredavac)
            .Where(p => p.Predmeti.Count < x);

        public IEnumerable<Profesor> AktivniAsistenti(int x, int minEcts) => Osobe.OfType<Profesor>()
            .Where(p => p.Zvanje == Zvanje.Asistent)
            .Where(p => p.Predmeti.Count(pp => pp.ECTS >= minEcts) > x);

        public void IzmjeniProfesore(Action<Profesor> action)
        {
            foreach (var prof in Osobe.OfType<Profesor>())
                action(prof);
        }
    }
}
