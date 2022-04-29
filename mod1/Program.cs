using mod1.bd;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace WebM1
{

    class Program
    {
        static void AdoNetDemo()
        {
            var connection = new SqlConnection(@"Server=.\SQLEXPRESS;Database=Phone;Trusted_Connection=True;");
            connection.Open();
            /////////
            Console.WriteLine("3 самые дорогие модели: ");

            var command = connection.CreateCommand();
            command.CommandText = "SELECT TOP (3) m.Name, m.Price, mn.Name_M from Models m join Manufs mn ON m.ManufId = mn.Id Order by m.Price DESC";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Name= {reader.GetString(reader.GetOrdinal("Name"))}, Price = {reader.GetDouble(1)}, Name_M = {reader.GetString(reader.GetOrdinal("Name_M"))} ");
            }
            reader.Close();
            //////////////////////////////
            Console.WriteLine("производитель+ средняя цена моделей с  bluetooth: ");
            command = connection.CreateCommand();
            command.CommandText = "SELECT  mn.Name_M, AVG(m.Price) as avg_price from Models m join Manufs mn ON m.ManufId = mn.Id WHERE m.Bluetooth = 1 Group by mn.Name_M";

            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Name_M = {reader.GetString(reader.GetOrdinal("Name_M"))}, avgPrice = {reader.GetDouble(1)} ");
            }
            reader.Close();
            ////////////////////////////////
            Console.WriteLine("производитель + нижняя цена линейки +  верхняя цена: ");

            command = connection.CreateCommand();
            command.CommandText = "SELECT   mn.Name_M, Min(m.Price) as min_price, Max(m.Price) as max_price from Models m join Manufs mn ON m.ManufId = mn.Id Group by mn.Name_M";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Name_M = {reader.GetString(reader.GetOrdinal("Name_M"))}, minPrice = {reader.GetDouble(1)}, maxPrice = {reader.GetDouble(2)} ");
            }
            reader.Close();
            ///////////////////////////////
            Console.WriteLine("Год выпуска + средняя цена + кол-во моделей с bluetooth");

            command = connection.CreateCommand();
            command.CommandText = "SELECT   m.Year, AVG(m.Price) as avg_price,  (select count(m1.Bluetooth) from Models m1 where m1.Bluetooth = 1 AND m1.Year = m.Year) as count_b from Models m Group by m.Year ";
            reader = command.ExecuteReader();
            while (reader.Read())
            {
                Console.WriteLine($"Year =  {reader.GetInt32(0)}, avgPrice = {reader.GetDouble(1)}, countB = {reader.GetInt16(2)} ");
            }
            reader.Close();
            //////////////////////////////
            Console.WriteLine("Уценить самую дорогую модель на 10%");
            command = connection.CreateCommand();
            command.CommandText = "DECLARE @maxPr INT SELECT @maxPr = MAX (Price) from [dbo].[Models] UPDATE[dbo].[Models] SET Price = Price - 0.1 * Price where Price = @maxPr";
            reader = command.ExecuteReader();
           
            reader.Close();
            /////////////////////////////////
            Console.WriteLine("Добавить модель производителю с самым дешёвым телефоном");
            command = connection.CreateCommand();
            command.CommandText = "DECLARE @minPr INT SELECT @minPr = MAX (Price)  from [dbo].[Models] DECLARE @minM INT SELECT @minM = ManufId  from[dbo].[Models] where Price = @minPr Insert into[dbo].[Models] values ('newM', 2022, 10000, 1, @minM)";
            reader = command.ExecuteReader();
            

            reader.Close();
            /////////////////////////////////////
            Console.WriteLine("Удалить модели дешевле среднего");
            command = connection.CreateCommand();
            command.CommandText = $"DECLARE @avgPr INT SELECT @avgPr = AVG (Price)  from [dbo].[Models] DELETE[dbo].[Models] where Price<@avgPr";
            reader = command.ExecuteReader();
            reader.Close();

            connection.Close();
        }

        
        static void MaxPrice3(CContext db)
        {
            
            
            //db.CreateDbIfNotExist();
            foreach (var m in db.Models.
                OrderByDescending(m1 => m1.Price).Take(3))
            {
                Console.WriteLine(m.Name + " " + m.Price + " " + m.Manuf.Name_M);
            }    
            
        }
        static void MunMed(CContext db)
        {


            //db.CreateDbIfNotExist();
            foreach (var m in db.Models.
                Where(x=>x.Bluetooth == true).
                GroupBy(m1 => m1.Manuf.Name_M).
                Select(x=> new {NameT = x.Key, Avg = x.Average(y=>y.Price) }))
            {
                Console.WriteLine(m.NameT + " " + m.Avg);
            }

        }
        static void MunPr(CContext db)
        {

            var coms = db.Models.OrderBy(x => x.Manuf.Name_M);
            var co = coms.GroupBy(m1 => m1.Manuf.Name_M).Select(x => new { NameT = x.Key, MinP = x.Min(y => y.Price), MaxP = x.Max(y => y.Price) });
            //db.CreateDbIfNotExist();
            foreach (var m in co)
                
            {
                Console.WriteLine(m.NameT + " "+ m.MinP + " "+ m.MaxP);
                //Console.WriteLine(m.Key.Name_M + " " + m.Min(x=>x.Price)+" " + m.Max(x => x.Price));
            }

        }
        static void YePr(CContext db)
        {
            var coms = db.Models.OrderBy(x => x.Year).               
                GroupBy(m1 => m1.Year).
                Select(x => new { YT = x.Key, AvgP = x.Average(y => y.Price), CountY = x.Where(x => x.Bluetooth == true).Count() });
            
            foreach (var m in coms)
            {
                Console.WriteLine(m.YT + " " + m.AvgP + " " + m.CountY);
            }

        }

        static void MaxPr(CContext db)
        {
            var maxP = db.Models.Max(x => x.Price);
            var maxE = db.Models.Where(x => x.Price == maxP).First();
            maxE.Price = maxP - 0.1 * maxP;
            db.SaveChanges();
            Console.WriteLine($"{maxE.Name} {maxE.Price}");
        }

        static void MinPr(CContext db)
        {
            var minP = db.Models.Min(x => x.Price);
            var minE = db.Models.Where(x => x.Price == minP).First().Manuf;
            Model newM = new Model { Name = "NewM", Year = 2022, Price = 22225, Bluetooth = true, Manuf = minE };
            db.Models.AddRange(newM);
            db.SaveChanges();
            foreach (var m in db.Models.
                Where(x=>x.Manuf == minE))
            {
                Console.WriteLine(m.Name + " " + m.Year + " " + m.Price + " " + m.Bluetooth+ " " + m.Manuf.Name_M);
            }
        }

        static void DelMin(CContext db)
        {
            var avgP = db.Models.Average(x => x.Price);
            foreach (var m in db.Models.
                Where(x => x.Price< avgP))
            {
                db.Models.Remove(m);
            }
            db.SaveChanges();

            Console.WriteLine($"AvgPrise = {avgP}");
            foreach (var m in db.Models)
            {
                Console.WriteLine(m.Name + " " + m.Year + " " + m.Price + " " + m.Bluetooth + " " + m.Manuf.Name_M);
            }
        }
        static void Main(string[] args)
        {
            CContext db = new CContext();
            db.CreateDbIfNotExist();
            // пересоздаем базу данных
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            Manuf microsoft = new Manuf { Name_M = "Microsoft" };
            Manuf google = new Manuf { Name_M = "Google" };
            Manuf sumsung = new Manuf { Name_M = "Sumsung" };
            db.Manufs.AddRange(microsoft, google, sumsung);

            Model tom = new Model { Name = "Tom", Year = 2015, Price = 12345, Bluetooth = false, Manuf = microsoft };
            Model bob = new Model { Name = "Bob", Year = 2017, Price = 21000, Bluetooth = true, Manuf = google };
            Model alice = new Model { Name = "Alice", Year = 2021, Price = 15000, Bluetooth = true, Manuf = microsoft };
            Model kate = new Model { Name = "Kate", Year = 2019, Price = 7000, Bluetooth = true, Manuf = google };
            Model tomas = new Model { Name = "Tomas", Year = 2016, Price = 10000, Bluetooth = false, Manuf = microsoft };
            Model tomek = new Model { Name = "Tomek", Year = 2015, Price = 3000, Bluetooth = true, Manuf = google };
            Model rare = new Model { Name = "Rare", Year = 2019, Price = 10500, Bluetooth = true, Manuf = sumsung };
            Model tate = new Model { Name = "Tate", Year = 2016, Price = 11000, Bluetooth = false, Manuf = sumsung };
            Model babe = new Model { Name = "Babe", Year = 2017, Price = 16435, Bluetooth = true, Manuf = sumsung };

            db.Models.AddRange(tom, bob, alice, kate, tomas, tomek, rare, tate, babe);
            db.SaveChanges();

            //AdoNetDemo();
            //Console.WriteLine("3 самые дорогие модели: ");
            //MaxPrice3(db);
            //Console.WriteLine();
            //Console.WriteLine("производитель+ средняя цена иоделей с  bluetooth: ");
            //MunMed(db);
            //Console.WriteLine();
            //Console.WriteLine("производитель + нижняя цена линейки +  верхняя цена: ");
            //MunPr(db);
            //Console.WriteLine();
            //Console.WriteLine("Год выпуска + средняя цена + кол-во моделей с bluetooth");
            //YePr(db);
            //Console.WriteLine();
            //Console.WriteLine("Уценить самую дорогую модель на 10%");
            //MaxPr(db);
            //Console.WriteLine();
            //Console.WriteLine("Добавить модель производителю с самым дешёвым телефоном");
            //MinPr(db);
            //Console.WriteLine();
            //Console.WriteLine("Удалить модели дешевле среднего");
            //DelMin(db);
        }

        
    }
}