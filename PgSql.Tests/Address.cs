using System;
using System.Collections.Generic;
using System.Text;

namespace PgSql.Tests
{
    public class Address
    {
        public string City { get; set; } 
        public string Zip { get; set; }

        public int Position { get; set; }

        public Address(string zip, string city)
        {
            City = city;
            Zip = zip;
        }
    }
}
