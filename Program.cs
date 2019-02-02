using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace conversion
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var dbConnection = new SqliteConnection("DataSource=:memory:");
            dbConnection.Open();

            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(dbConnection)
                .Options;
            var context = new TestDbContext(options);

            context.Database.EnsureCreated();

            context.Add(new Derived1() { PropertyWithSharedBackingColumn = 1 });
            context.Add(new Derived2() { PropertyWithSharedBackingColumn = 2 });
            context.SaveChanges();

            List<Base> results = new TestDbContext(options).Entities.ToList();

            Console.WriteLine(string.Join(Environment.NewLine, results));
        }
    }

    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Base>()
                .HasDiscriminator(e => e.Type)
                .HasValue<Derived1>(1)
                .HasValue<Derived2>(2);


            builder.Entity<Derived1>()
                .HasBaseType<Base>()
                .Property(e => e.PropertyWithSharedBackingColumn)
                .HasColumnName("SharedColumn")
                .HasConversion(
                    i => "1",
                    stored => 1);

            builder.Entity<Derived2>()
                .HasBaseType<Base>()
                .Property(e => e.PropertyWithSharedBackingColumn)
                .HasColumnName("SharedColumn")
                .HasConversion(
                    i => "2",
                    stored => 2);

        }

        public DbSet<Base> Entities { get; set; }
    }

    public abstract class Base
    {
        public int Id { get; set; }
        public abstract int Type { get; set; }
    }

    public class Derived1 : Base
    {
        public override int Type
        {
            get => 1;
            set { }
        }

        public int PropertyWithSharedBackingColumn { get; set; }

        public override string ToString()
        {
            return $"Id={Id} Type={Type} SharedColumnValue={PropertyWithSharedBackingColumn}";
        }
    }

    public class Derived2 : Base
    {

        public override int Type
        {
            get => 2;
            set { }
        }

        public int PropertyWithSharedBackingColumn { get; set; }

        public override string ToString()
        {
            return $"Id={Id} Type={Type} SharedColumnValue={PropertyWithSharedBackingColumn}";
        }
    }
}
