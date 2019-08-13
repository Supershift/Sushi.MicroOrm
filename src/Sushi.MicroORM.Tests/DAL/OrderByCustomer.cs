using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sushi.MicroORM.Tests.DAL
{
    [DataMap(typeof(OrderByCustomerMap))]
    public class OrderByCustomer
    {
        public class OrderByCustomerMap : DataMap<OrderByCustomer>
        {
            public OrderByCustomerMap()
            {
                Id(x => x.CustomerID, "Order_Customer_Key");
                Map(x => x.Count, "Order_Count");
            }

        }


        public int CustomerID { get; set; }
        public int Count { get; set; }
    }
}