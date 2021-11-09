using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.Tests.DAL
{
    
    public class OrderObjectAsPk
    {
        public class OrderMap : DataMap<OrderObjectAsPk>
        {
            public OrderMap()
            {
                Table("cat_Orders");
                Id(x => x.ID, "Order_Key").Identity().SqlType(System.Data.SqlDbType.Int);
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

        public object ID { get; set; }
        public int CustomerID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Created2 { get; set; }
        public string Comments { get; set; }
        public TimeSpan? DeliveryTime { get; set; }
        public TimeSpan DeliveryTime2 { get; set; }
        public decimal? Amount { get; set; }
        public double? Measurement { get; set; }
    }

    
}
