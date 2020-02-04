using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL.Multilevel
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
                Map(x => x.CustomerID, "Order_Customer_Key");
                Map(x => x.Created, "Order_Created");
                Map(x => x.Delivery.Comments, "Order_Comments");
                Map(x => x.Delivery.DeliveryTime, "Order_DeliveryTime");
                Map(x => x.Delivery.DeliveryTime2, "Order_DeliveryTime2");
                Map(x => x.Created2, "Order_Created").ReadOnly();
            }
        }

        public class DeliveryInfo
        {
            public string Comments { get; set; }
            public TimeSpan? DeliveryTime { get; set; }
            public TimeSpan DeliveryTime2 { get; set; }
        }

        public struct GeoLocation
        {
            public int Longitutude { get; set; }
            public int Latitude { get; set; }
        }

        public int ID { get; set; }
        public int CustomerID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public DeliveryInfo Delivery;
        public GeoLocation Location { get; set; }
        public string Comments { get; set; }

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
