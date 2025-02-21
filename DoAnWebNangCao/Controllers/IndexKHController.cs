using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnWebNangCao.Models;

namespace DoAnWebNangCao.Controllers
{
    public class IndexKHController : Controller
    {
        // GET: IndexKH
        DanhMucModelDataContext _sanPhamContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ProductDetails(string id)
        {
            var product = (from p in _sanPhamContext.Products
                           join b in _sanPhamContext.ProductBrands on p.BrandID equals b.BrandID
                           join c in _sanPhamContext.ProductCatalogues on p.CatalogueID equals c.CatalogueID
                           where p.ProductID == id
                           select new ProductDetailsViewModel
                           {
                               Product = p,
                               ProductBrand = b,
                               ProductCatalogue = c,
                           }).FirstOrDefault();

            if (product == null)
            {
                return HttpNotFound();  
            }

            return View(product);
        }



        public JsonResult GetProductDetail1(string id)
        {
            try
            {
                var product = (from p in _sanPhamContext.Products
                               join c in _sanPhamContext.ProductCatalogues
                               on p.CatalogueID equals c.CatalogueID
                               where p.ProductID == id
                               select new
                               {
                                   ProductID = p.ProductID,
                                   ProductName = p.Name,
                                   Price = p.Price,
                                   Quantity = p.Quantity,
                                   CategoryName = c.CatalogueName,
                                   Description = p.Description,
                                   color = p.color,
                                   size = p.size,
                                   ImagePath = p.Image_path
                               }).FirstOrDefault();

                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" },
                        JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = true,
                    data = product
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi khi lấy chi tiết sản phẩm: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult SearchProducts(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Json(new { data = new List<Product>() }, JsonRequestBehavior.AllowGet);
            }

            var searchResults = _sanPhamContext.Products
                .Where(p => p.Name.Contains(query))  
                .Select(p => new
                {
                    p.ProductID,
                    p.Name,
                    p.Price,
                    p.Image_path
                })
                .ToList();  

            return Json(new { data = searchResults }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult FilterProducts(string sortOrder, decimal? minPrice, decimal? maxPrice)
        {
            // Khởi tạo truy vấn cơ bản
            var productsQuery = _sanPhamContext.Products.AsQueryable();

            // Lọc theo khoảng giá nếu có
            if (minPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);

            // Lọc theo thứ tự giá (từ thấp đến cao hoặc ngược lại)
            if (sortOrder == "asc")
            {
                productsQuery = productsQuery.OrderBy(p => p.Price);  // Giá từ thấp đến cao
            }
            else if (sortOrder == "desc")
            {
                productsQuery = productsQuery.OrderByDescending(p => p.Price);  // Giá từ cao đến thấp
            }

            // Lấy danh sách sản phẩm sau khi lọc và sắp xếp
            var filteredProducts = productsQuery
                .Select(p => new
                {
                    p.ProductID,
                    p.Name,
                    p.Price,
                    p.Image_path
                })
                .ToList();

            // Trả kết quả cho frontend dưới dạng JSON
            return Json(new { data = filteredProducts }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetProductDetail(string productID)
        {
            Debug.WriteLine("Đã đến đây");

            // Truy vấn kết hợp giữa bảng Products, ProductCatalogues và ProductBrands
            var listSanPham = _sanPhamContext.Products
    .Join(_sanPhamContext.ProductCatalogues,
          sp => sp.CatalogueID,
          pc => pc.CatalogueID,
          (sp, pc) => new
          {
              sp.ProductID,
              sp.Name,
              sp.Price,
              sp.Description,
              sp.Quantity,
              CatalogueName = pc.CatalogueName,
              ImagePath = sp.Image_path,
              BrandID = sp.BrandID
          })
    .Join(_sanPhamContext.ProductBrands,
          sp => sp.BrandID,
          pb => pb.BrandID,
          (sp, pb) => new
          {
              sp.ProductID,
              sp.Name,
              sp.Price,
              sp.Description,
              sp.Quantity,
              CatalogueName = sp.CatalogueName,
              ImagePath = sp.ImagePath,
              BrandName = pb.BrandName
          })
    .Where(o => o.ProductID == productID).FirstOrDefault();


            if (listSanPham == null)
            {
                Debug.WriteLine("Không có sản phẩm.");
            }

            // Lấy tổng số sản phẩm
            var tongSoDanhMuc = _sanPhamContext.Products.Count();

            // Trả về dữ liệu dưới dạng JSON, bao gồm cả danh sách sản phẩm và tổng số sản phẩm
            return Json(new { listSanPham, tongSoDanhMuc }, JsonRequestBehavior.AllowGet);
        }
        // Hủy context khi không sử dụng nữa
    }

}
