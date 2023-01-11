using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests.DAL.Customers
{
    [DataMap(typeof(AddressMap))]
    public class Address
    {
        public class AddressMap : DataMap<Address>
        {
            public AddressMap()
            {
                Table("cat_Addresses");
                Id(x => x.ID, "Address_Key");
                Map(x => x.Street, "Address_Street");
                Map(x => x.Number, "Address_Number");
                Map(x => x.City, "Address_City");
            }
        }

        public int ID { get; set; }
        public string Street { get; set; }
        public string Number { get; set; }
        public string City { get; set; }
    }
}
