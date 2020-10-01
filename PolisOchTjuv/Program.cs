using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PolisOchTjuv
{
    class Program
    {
        static void Main(string[] args)
        {
            int size = 25;
            int TimeTilRelease = 30;

            Random rnd = new Random();
            var Persons = new List<Person>();
            var Jail = new List<Person>();
            CreatePersons(rnd, Persons);

            while (true)
            {

                var StealEvents = new List<string>();
                var ArrestEvents = new List<string>();

                for (int y = 1; y <= size; y++)
                {
                    for (int x = 1; x <= size * 4; x++)
                    {
                        int position = PositionCheck(x, y, Persons);

                        switch (position)
                        {
                            case 0: //Nothing
                                Console.Write(" ");
                                break;
                            case 1: //Medborgare
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("M");
                                break;
                            case 2: //Tjuv
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write("T");
                                break;
                            case 3: //Polis
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("P");
                                break;
                            case 4: //Tjuv + Medborgare
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("X");
                                StealEvents.Add(Steal(x, y, Persons, rnd));
                                break;
                            case 5: //Polis + Tjuv
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("X");
                                ArrestEvents.Add(Arrest(x, y, Persons));
                                break;
                            case 6: //Medborgare + Tjuv + Polis
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("X");
                                ArrestEvents.Add(Arrest(x, y, Persons));
                                break;
                        }
                    }
                    Console.WriteLine();
                }

                var jailed = Persons.Where(Persons => Persons.Jailed == 1).ToList();
                Persons = Persons.Except(jailed).ToList();
                Jail.AddRange(jailed);

                var timeInJail = Jail.Where(Jail => Jail.TimeInJail >= TimeTilRelease).ToList();
                Jail = Jail.Except(timeInJail).ToList();
                Persons.AddRange(timeInJail);

                Events(ArrestEvents, StealEvents, Jail);
                NewPosition(Persons, TimeTilRelease);
            }
        }

        public static void CreatePersons(Random rnd, List<Person> Persons)
        {
            int AntalMedborgare = 30;
            int AntalTjuvar = 20;
            int AntalPoliser = 10;

            for (int i = 0; i < AntalMedborgare; i++) Persons.Add(new Medborgare(rnd.Next(1, 26), rnd.Next(1, 101), rnd.Next(-1, 2), rnd.Next(-1, 2)));
            for (int i = 0; i < AntalTjuvar; i++) Persons.Add(new Tjuv(rnd.Next(1, 26), rnd.Next(1, 101), rnd.Next(-1, 2), rnd.Next(-1, 2)));
            for (int i = 0; i < AntalPoliser; i++) Persons.Add(new Polis(rnd.Next(1, 26), rnd.Next(1, 101), rnd.Next(-1, 2), rnd.Next(-1, 2)));
        }

        public static int PositionCheck(int x, int y, List<Person> Persons)
        {
            int place = 0;
            bool medborgare = false;
            bool tjuv = false;
            bool polis = false;

            foreach (Person a in Persons)
            {
                if (a is Medborgare)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        place = 1;
                        medborgare = true;
                    }
                }
                if (a is Tjuv)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        place = 2;
                        tjuv = true;
                    }
                }
                if (a is Polis)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        place = 3;
                        polis = true;
                    }
                }

                if(medborgare && tjuv && polis)
                {
                    place = 6;
                }
                else if (medborgare && tjuv)
                {
                    place = 4;
                }
                else if (polis && tjuv)
                {
                    place = 5;
                }
            }
            return place;
        }

        public static string Arrest(int x, int y, List<Person> Persons)
        {
            int givePlånbok = 0;
            int givePengar = 0;
            int giveKlocka = 0;
            int giveTelefon = 0;
            string ArrestEvents = "";
            
            foreach(Person a in Persons)
            {
                if (a is Tjuv)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        givePlånbok = a.Plånbok;
                        givePengar = a.Pengar;
                        giveKlocka = a.Klocka;
                        giveTelefon = a.Telefon;
                        a.Plånbok = 0;
                        a.Pengar = 0;
                        a.Klocka = 0;
                        a.Telefon = 0;

                        if (givePlånbok == 0 && givePengar == 0 && giveKlocka == 0 && giveTelefon == 0) { ArrestEvents = "Polis tog tjuven men han hade inget stulet gods på sig."; }
                        else
                        {
                            ArrestEvents = "Polis tog alla tjuvens stulna saker och satte tjuven i fängelse.";
                            a.Jailed = 1;
                        }

                    }
                }
                if (a is Polis)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        a.Plånbok = a.Plånbok + givePlånbok;
                        a.Pengar = a.Pengar + givePengar;
                        a.Klocka = a.Klocka + giveKlocka;
                        a.Telefon = a.Telefon + giveTelefon;
                    }
                }
            }

            return ArrestEvents;
        }

        public static string Steal(int x, int y, List<Person> Persons, Random rnd)
        {
            int givePlånbok = 0;
            int givePengar = 0;
            int giveKlocka = 0;
            int giveTelefon = 0;
            int take = rnd.Next(1, 5);
            string Event = "";
            bool takenItem = false;

            foreach (Person a in Persons)
            {
                if (a is Medborgare)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        while (!takenItem)
                        {
                            switch (take)
                            {
                                case 1:
                                    if (a.Plånbok > 0)
                                    {
                                        takenItem = true;
                                        givePlånbok++;
                                        a.Plånbok--;
                                        Event = "Tjuven stal en plånbok från medborgaren.";
                                    }
                                    else take++;
                                    break;
                                case 2:
                                    if (a.Pengar > 0)
                                    {
                                        takenItem = true;
                                        givePengar++;
                                        a.Pengar--;
                                        Event = "Tjuven stal en Peng från medborgaren.";
                                    }
                                    else take++;
                                    break;
                                case 3:
                                    if (a.Klocka > 0)
                                    {
                                        takenItem = true;
                                        giveKlocka++;
                                        a.Klocka--;
                                        Event = "Tjuven stal en klocka från medborgaren.";
                                    }
                                    else take++;
                                    break;
                                case 4:
                                    if (a.Telefon > 0)
                                    {
                                        takenItem = true;
                                        giveTelefon++;
                                        a.Telefon--;
                                        Event = "Tjuven stal en Telefon från medborgaren.";
                                    }
                                    else if(a.Plånbok > 0)
                                    {
                                        takenItem = true; 
                                        givePlånbok++; 
                                        a.Plånbok--;
                                        Event = "Tjuven stal en plånbok från medborgaren.";
                                    }
                                    else if (a.Pengar > 0) 
                                    { 
                                        takenItem = true; 
                                        givePengar++; 
                                        a.Pengar--; 
                                        Event = "Tjuven stal en Peng från medborgaren.";
                                    }
                                    else if (a.Klocka > 0)
                                    { 
                                        takenItem = true; 
                                        giveKlocka++;
                                        a.Klocka--;
                                        Event = "Tjuven stal en klocka från medborgaren.";
                                    }
                                    else 
                                    { 
                                        takenItem = true; 
                                        Event = "Medborgaren hade inget kvar för tjuven att sno.";
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (a is Tjuv)
                {
                    if (x == a.PositionX && y == a.PositionY)
                    {
                        a.Plånbok = a.Plånbok + givePlånbok;
                        a.Pengar = a.Pengar + givePengar;
                        a.Klocka = a.Klocka + giveKlocka;
                        a.Telefon = a.Telefon + giveTelefon;
                    }
                }
            }
            return Event;
        }

        public static void NewPosition(List<Person> Persons, int TimeTilRelease)
        {
            foreach (Person a in Persons)
            {
                a.PositionY = a.PositionY + a.MoveY;
                a.PositionX = a.PositionX + a.MoveX;
                if (a.PositionY > 25) a.PositionY = 1;
                if (a.PositionY < 1) a.PositionY = 25;
                if (a.PositionX > 100) a.PositionX = 1;
                if (a.PositionX < 0) a.PositionX = 99;

                if (a.TimeInJail >= TimeTilRelease && a is Tjuv)
                {
                    Console.WriteLine("En tjuv blev frisläppt.");
                    Thread.Sleep(3000);
                    a.TimeInJail = 0;
                    a.Jailed = 0;
                }

            }
            Thread.Sleep(300);
            Console.Clear();

        }

        public static void Events(List<string> ArrestEvents, List<string> StealEvent, List<Person> Jail)
        {
            bool playSleep = false;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"Fängelse: ");
            foreach (var j in Jail)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("T");
                j.TimeInJail++;
            }

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            foreach (var ord in StealEvent)
            {
                Console.WriteLine(ord);
                playSleep = true;
            }
            
            foreach (var ord2 in ArrestEvents)
            {
                Console.WriteLine(ord2);
                playSleep = true;
                
            }

            if(playSleep) Thread.Sleep(3000);

        }
    }

    class Person
    {
        public int PositionY;
        public int PositionX;
        public int MoveY;
        public int MoveX;

        public int Jailed = 0;
        public int TimeInJail = 0;

        public int Plånbok;
        public int Pengar;
        public int Klocka;
        public int Telefon;
    }

    class Medborgare : Person
    {
        public Medborgare(int positionY, int positionX, int moveY, int moveX)
        {
            PositionY = positionY;
            PositionX = positionX;
            MoveY = moveY;
            MoveX = moveX;
            Plånbok = 1;
            Pengar = 1;
            Klocka = 1;
            Telefon = 1;
    }
    }

    class Polis : Person
    {
        public Polis(int positionY, int positionX, int moveY, int moveX)
        {
            PositionY = positionY;
            PositionX = positionX;
            MoveY = moveY;
            MoveX = moveX;
            Plånbok = 0;
            Pengar = 0;
            Klocka = 0;
            Telefon = 0;
        }
    }

    class Tjuv : Person
    { 
        public Tjuv(int positionY, int positionX, int moveY, int moveX)
        {
            PositionY = positionY;
            PositionX = positionX;
            MoveY = moveY;
            MoveX = moveX;
            Plånbok = 0;
            Pengar = 0;
            Klocka = 0;
            Telefon = 0;
        }
    }
}
