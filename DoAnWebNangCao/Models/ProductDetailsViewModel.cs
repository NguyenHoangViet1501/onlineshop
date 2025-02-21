using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoAnWebNangCao.Models
{
    public class ProductDetailsViewModel
    {

            public Product Product { get; set; }
            public ProductBrand ProductBrand { get; set; }
            public ProductCatalogue ProductCatalogue { get; set; }
        }
    }
