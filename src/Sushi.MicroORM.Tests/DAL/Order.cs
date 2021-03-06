﻿using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
{
    [DataMap(typeof(OrderMap))]    
    public class Order
    {
        public class OrderMap : DataMap<Order>
        {
            public OrderMap()
            {
                Table("cat_Orders");
                Id(x => x.ID, "Order_Key");
                Map(x => x.CustomerID, "Order_CustomerID");
                Map(x => x.Created, "Order_Created");
                Map(x => x.Comments, "Order_Comments");
                Map(x => x.DeliveryTime, "Order_DeliveryTime");
                Map(x => x.DeliveryTime2, "Order_DeliveryTime2");
                Map(x => x.Created2, "Order_Created").ReadOnly();
                Map(x => x.Amount, "Order_Amount");
                Map(x => x.Measurement, "Order_Measurement");
            }
        }

        public class InvalidOrderMap : DataMap<Order>
        {
            public InvalidOrderMap()
            {
                //the table is missing on purpose, this allows testing of validation
                Id(x => x.ID, "Order_Key");
                Map(x => x.CustomerID, "Order_CustomerID");
                Map(x => x.Created, "Order_Created");
                Map(x => x.Comments, "Order_Comments");
                Map(x => x.DeliveryTime, "Order_DeliveryTime");
                Map(x => x.DeliveryTime2, "Order_DeliveryTime2");
                Map(x => x.Amount, "Order_Amount");
                Map(x => x.Measurement, "Order_Measurement");
            }
        }        

        public int ID;
        public int CustomerID;
        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public string Comments { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public TimeSpan DeliveryTime2 { get; set; }
        public decimal? Amount { get; set; }
        public double? Measurement { get; set; }
        
        public static List<Order> FetchAll(int customerID)
        {
            var connector = new Connector<Order>();
            var filter = connector.CreateDataFilter();
            
            filter.Add(x => x.CustomerID, customerID);
            
            var result = connector.FetchAll(filter);
            return result;
        }
    }

    
}
