using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnWebNangCao.Models
{
    public class CartItem
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
    }
}