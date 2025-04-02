// -----------------------------------------------------------------------
// <copyright file="Customer.cs" company="Microsoft Corp.">
//     Copyright (c) Microsoft Corp. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Common.Cache.Tests.Steps
{
    using System;

#if NET462
    public class Customer
#else
    using MemoryPack;

    [MemoryPackable]
    public sealed partial class Customer
#endif
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDay { get; set; }
        public byte[] Profile { get; set; }

        public static Customer CreateTestData()
        {
            return new Customer()
            {
                Id = 100,
                FirstName = "John",
                LastName = "Doe",
                BirthDay = new DateTime(1980, 1, 1),
                Profile = new byte[] { 0x01, 0x02, 0x03 }
            };
        }

        public static Customer CreateTestData(int payloadSize)
        {
            var payload = new byte[payloadSize];
            new Random().NextBytes(payload);
            return new Customer()
            {
                Id = payloadSize % 100,
                FirstName = "John",
                LastName = "Doe",
                BirthDay = new DateTime(1980, 1, 1),
                Profile = payload
            };
        }
    }
}