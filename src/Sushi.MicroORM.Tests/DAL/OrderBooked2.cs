using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
{    
    public class OrderBooked2
    {        
        public class OrderBooked2Map : DataMap<OrderBooked2>
        {
            public OrderBooked2Map()
            {
                Table("cat_Orders LEFT JOIN cat_Bookings ON Booking_CustomerID = Order_Customer_Key");
                Id(x => x.ID, "Order_Key");
                Map(x => x.CustomerID, "Order_Customer_Key");
                Map(x => x.Created, "Order_Created");
                Map(x => x.Comments, "Order_Comments");                
                Map(x => x.DeliveryTime, "Order_DeliveryTime");
                Map(x => x.DeliveryTime2, "Order_DeliveryTime2");
                Map(x => x.Created2, "Order_Created").ReadOnly();
                Map(x => x.BookingID, "Booking_Key").ReadOnly();
            }
        }

        public class OrderBooked2MapSave : OrderBooked2Map
        {
            public OrderBooked2MapSave() : base()
            {
                Table("cat_Orders");
            }
        }


        public int ID { get; set; }
        public int CustomerID { get; set; }
        public int OrderCount { get; set; }

        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public string Comments { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public TimeSpan DeliveryTime2 { get; set; }

        public int BookingID { get; set; }        

        public static List<OrderBooked2> FetchAll(int customerID)
        {
            var connector = new Connector<OrderBooked2>();

            var filter = connector.CreateDataFilter();
            
            filter.Add(x => x.CustomerID, customerID);
            
            var result = connector.FetchAll(filter);
            return result;
        }
        
        public void Save()
        {
            var connector = new Connector<OrderBooked2>(new OrderBooked2MapSave());
            connector.Save(this);
        }
    }
}
