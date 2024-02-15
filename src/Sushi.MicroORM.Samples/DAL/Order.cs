using Sushi.MicroORM.Mapping;
using System;

namespace Sushi.MicroORM.Samples.DAL
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
            }
        }

        public int ID;
        public int CustomerID;
        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public string Comments { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public TimeSpan DeliveryTime2 { get; set; }
    }
}