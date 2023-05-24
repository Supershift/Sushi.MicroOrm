using Sushi.MicroORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sushi.MicroORM.ManualTests.DAL
{   
    public class Product
    {
        public class ProductMap : DataMap<Product>
        {
            public ProductMap()
            {
                Table("cat_Products");
                Id(x => x.ID, "Product_Key").Identity();
                Map(x => x.MetaData.Name, "Product_Name").SqlType(System.Data.SqlDbType.NVarChar);
                Map(x => x.MetaData.Description, "Product_Description").SqlType(System.Data.SqlDbType.VarChar);
                Map(x => x.Price, "Product_Price");                
                Map(x => x.MetaData.Identification.BarCode, "Product_BarCode");
                Map(x => x.MetaData.Identification.GUID, "Product_Guid");
                Map(x => x.MetaData.ProductTypeID, "Product_ProductTypeID");
                Map(x => x.ExternalIdentification.ExternalID, "Product_ExternalID");
            }
        }

        //these classes are here to test mapping of 'nested' members
        public class ProductMetaData
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public ProducType? ProductTypeID { get; set; }
            public Identification Identification { get; set; }
        }

        public class Identification
        {            
            public byte[] BarCode { get; set; }
            public Guid GUID { get; set; }
        }

        public class Identification2
        {
            public int? ExternalID { get; set; }
        }

        public int ID { get; set; }        
        public decimal Price { get; set; }
        public ProductMetaData MetaData { get; set; }
        public Identification2? ExternalIdentification { get; set; }

        public enum ProducType
        {
            Hifi = 1,
            Computer = 2,
            Living = 3,
            Food = 4
        }
    }
}
