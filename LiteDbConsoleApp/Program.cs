using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDbConsoleApp
{
    class Program
    {
        const string ConnectionString = @"C:\Work\LiteDbTest\CustomersDatabase.lite";
        static void Main(string[] args)
        {
            // CreateCUstomersCollection();

            EnumCustomers();
        }

        private static void EnumCustomers()
        {
            using (LiteDatabase database = new LiteDatabase(ConnectionString))
            {
                var customers = database.GetCollection("Customer");

                Console.WriteLine("FindAll unsorted");
                foreach (var cust in customers.FindAll().Take(20))
                {
                    Console.WriteLine($"\t {cust["IdProp"]}");
                }
                Console.WriteLine();

                Console.WriteLine($" #{Thread.CurrentThread.ManagedThreadId}: Use reader");
                var reader = new CollectionReader(customers.FindAll().Take(10));
                foreach (var cust in reader.Read())
                {
                    Console.WriteLine($"  {cust["IdProp"]}");
                }
/*
                                Console.WriteLine("Find QueryAll ordered by IdProp");
                                foreach (var cust in customers.Find(Query.All("IdProp")).Take(10))
                                {
                                    Console.WriteLine($"\t {cust["IdProp"]}");
                                }
                */

            }

            Thread.Sleep(30000);
        }

        private static void CreateCUstomersCollection()
        {
            Random rand = new Random();
            List<int> ids = Enumerable.Range(1, 100).Select(_ => rand.Next(1, 100)).Distinct().ToList();


            using (LiteDatabase database = new LiteDatabase(ConnectionString))
            {
                var customers = database.GetCollection<Customer>();

                customers.EnsureIndex("IdProp");

                foreach (var customer in ids.Select(id => new Customer
                {
                    _id = ObjectId.NewObjectId().ToString(),
                    IdProp = id,
                    CustomerId = $"CUST{id:000}",
                    FirstName = "Jonh_" + id,
                    LastName = "Doe_" + id,
                    Address = new string[] { $"{id} Street-{id}", "Toronto, ON" },
                    Birthdate = new DateTime(1990, 1, 1) + TimeSpan.FromDays(rand.Next(-10 * 365, 10 * 365))
                }))
                {
                    Console.WriteLine($"Customer {customer.IdProp} {customer.CustomerId}");

                    customers.Insert(customer);
                }
            }
        }


    }
}
