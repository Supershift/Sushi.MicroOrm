﻿using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.ManualTests.DAL.Customers
{
    [DataMap(typeof(CustomerMap))]
    public class Customer
    {
        public class CustomerMap : DataMap<Customer>
        {
            public CustomerMap()
            {
                Table("cat_Customers");
                Id(x => x.ID, "Customer_Key");
                Map(x => x.Name, "Customer_Name");                
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
    }
}
