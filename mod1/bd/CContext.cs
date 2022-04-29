using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mod1.bd
{
    internal class CContext : DbContext
    {
        public DbSet <Manuf> Manufs{ get; set; }
        public DbSet <Model> Models { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=.\SQLEXPRESS;Database=PhoneEF;Trusted_Connection=True;");
            base.OnConfiguring(optionsBuilder);
        }

        public void CreateDbIfNotExist()
        {
            this.Database.EnsureCreated();
        }


    }

    public class Manuf
    {

        public int Id { get; set; }

        [MaxLength(10)]
        public string Name_M { get; set; }
    }

    public class Model
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(10)]
        public string Name { get; set; }
        public int Year { get; set; }
        public double Price { get; set; }
        public bool Bluetooth { get; set; }
        public int ManufId { get; set; }
        public Manuf Manuf { get; set; }
    }
}
