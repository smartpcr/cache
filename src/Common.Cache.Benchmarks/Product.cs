// -----------------------------------------------------------------------
// <copyright file="Product.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Benchmarks
{
    using System;
    using System.Collections.Generic;
    using Common.Cache.Tests.Steps;

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public DateTime ManufactureDate { get; set; }
        public bool IsAvailable { get; set; }
        public Customer Manufacturer { get; set; }
        public List<string> Tags { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public static List<Product> CreateTestData(int size)
        {
            var products = new List<Product>(size);
            for (var i = 0; i < size; i++)
            {
                products.Add(new Product
                {
                    Id = i,
                    Name = $"Product {i}",
                    Price = (decimal)(i * 10.99),
                    ManufactureDate = DateTime.Now.AddDays(-i),
                    IsAvailable = i % 2 == 0,
                    Manufacturer = Customer.CreateTestData(),
                    Tags = new List<string> { "Tag1", "Tag2", "Tag3" },
                    Attributes = new Dictionary<string, string>
                    {
                        { "Color", "Red" },
                        { "Size", "Medium" }
                    }
                });
            }

            return products;
        }
    }
}