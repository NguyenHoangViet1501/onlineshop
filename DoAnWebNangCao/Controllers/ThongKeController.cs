using DoAnWebNangCao.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace DoAnWebNangCao.Controllers
{
    public class ThongKeController : Controller
    {
        DanhMucModelDataContext thongkeContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");

        // GET: ThongKe
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        public ActionResult DoanhThu()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult DoanhThuNgay()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult SPDaBan()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public ActionResult SPHetHang()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }
        public JsonResult GetDoanhThu(int month, int year)
        {
            try
            {
                var orders = thongkeContext.Orders
                    .Where(o => o.OrderDate.Month == month && o.OrderDate.Year == year && o.Status != "Đã huỷ")
                    .ToList();

                // Tính tổng doanh thu
                var totalRevenue = orders.Sum(o => o.TotalPrice); // Tính tổng doanh thu từ các đơn hàng

                return Json(new { totalRevenue = totalRevenue }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Log lỗi vào console hoặc hệ thống log của bạn
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Hàm lấy thông tin thống kê doanh thu theo ngày
        public JsonResult GetDoanhThuTheoNam(int year)
        {
            try
            {
                var data = thongkeContext.Orders
                    .Where(o => o.OrderDate.Year == year && o.Status != "Đã huỷ")
                    .GroupBy(o => o.OrderDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        TotalRevenue = g.Sum(o => o.TotalPrice)
                    })
                    .OrderBy(x => x.Month)
                    .ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetRevenueStats(DateTime startDate, DateTime endDate)
        {
            try
            {
                endDate = endDate.AddDays(1).AddTicks(-1);

                // Nếu startDate và endDate giống nhau, lấy tất cả đơn hàng trong ngày đó
                if (startDate.Date == endDate.Date)
                {
                    // Đảm bảo startDate là 00:00:00
                    endDate = endDate.AddDays(1).AddTicks(-1); // Đảm bảo endDate là 23:59:59 của ngày đó
                }

                // Lọc đơn hàng theo ngày trong khoảng thời gian và phân trang
                var orders = thongkeContext.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != "Đã huỷ")
                    .OrderBy(o => o.OrderDate)

                    .GroupBy(o => o.OrderDate.Date) // Nhóm theo ngày (chỉ lấy phần ngày, bỏ qua giờ, phút, giây)
                    .Select(g => new
                    {
                        OrderDate = g.Key, // Ngày của nhóm
                        TotalOrders = g.Count(), // Tổng số đơn hàng trong ngày
                        TotalRevenue = g.Sum(o => o.TotalPrice) // Tổng doanh thu trong ngày
                    })
                    .ToList();


                var totalRevenue = orders.Sum(o => o.TotalRevenue); // Tổng doanh thu trong khoảng thời gian




                var result = new
                {
                    Data = orders,

                    TotalRevenue = totalRevenue // Tổng doanh thu
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetDoanhThuThangHT()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // Lọc các đơn hàng trong tháng hiện tại
                var orders = thongkeContext.Orders
                    .Where(o => o.OrderDate.Month == currentMonth && o.OrderDate.Year == currentYear && o.Status != "Đã huỷ")
                    .ToList();

                // Tính tổng doanh thu
                var totalRevenue = orders.Sum(o => o.TotalPrice);

                return Json(new { totalRevenue = totalRevenue }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Action để lấy doanh thu của ngày hiện tại
        public JsonResult GetDoanhThuNgayHT()
        {
            try
            {
                var currentDate = DateTime.Now.Date;

                // Lọc các đơn hàng trong ngày hiện tại
                var orders = thongkeContext.Orders
                    .Where(o => o.OrderDate.Date == currentDate && o.Status != "Đã huỷ")
                    .ToList();

                // Tính tổng doanh thu
                var totalRevenue = orders.Sum(o => o.TotalPrice);

                return Json(new { totalRevenue = totalRevenue }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetProductSalesInMonth(int month, int year, int pageNumber, int pageSize)
        {
            try
            {

                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var productSales = thongkeContext.OrderDetails
                    .Where(od => od.Order.OrderDate >= startDate && od.Order.OrderDate <= endDate && od.Order.Status != "Đã huỷ")
                    .GroupBy(od => new { od.ProductID, od.Product.Name })
                    .Select(g => new
                    {
                        ProductID = g.Key.ProductID,
                        ProductName = g.Key.Name,
                        TotalSold = g.Sum(od => od.Quantity)
                    })
                   .OrderByDescending(p => p.TotalSold)
                  .Skip((pageNumber - 1) * pageSize)
                   .Take(pageSize)
                   .ToList();

                if (productSales == null || productSales.Count == 0)
                {
                    Debug.WriteLine("Không có sản phẩm.");
                }

                // Lấy tổng số sản phẩm
                var tongSoSanPham = thongkeContext.Products.Count();

                return Json(new { productSales, tongSoSanPham }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetProductSalesInMonthHT()
        {
            try
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                var productSales = thongkeContext.OrderDetails
                    .Where(o => o.Order.OrderDate.Month == currentMonth && o.Order.OrderDate.Year == currentYear && o.Order.Status != "Đã huỷ")
                    .GroupBy(od => new { od.ProductID, od.Product.Name })
                    .Select(g => new
                    {

                        TotalSold = g.Sum(od => od.Quantity)
                    })
                    .ToList();
                var totalRevenue = productSales.Sum(o => o.TotalSold);

                if (productSales == null || productSales.Count == 0)
                {
                    Debug.WriteLine("Không có sản phẩm.");
                }
                return Json(new { totalRevenue = totalRevenue }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetListSanPham(int pageNumber, int pageSize)
        {
            Debug.WriteLine("Đã đến đây");

            // Truy vấn kết hợp giữa bảng Products, ProductCatalogues và ProductBrands
            var listSanPham = thongkeContext.Products
                .Where(sp => sp.Quantity == 0) // Lọc sản phẩm hết hàng
                .Join(thongkeContext.ProductCatalogues,
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
                .Join(thongkeContext.ProductBrands,
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
                .OrderBy(sp => sp.ProductID) // Sắp xếp theo ProductID
                .Skip((pageNumber - 1) * pageSize) // Phân trang
                .Take(pageSize) // Lấy số lượng sản phẩm theo pageSize
                .ToList();

            if (listSanPham == null || listSanPham.Count == 0)
            {
                Debug.WriteLine("Không có sản phẩm.");
            }

            // Lấy tổng số sản phẩm có Quantity == 0
            var tongSoSanPham = thongkeContext.Products
                .Where(sp => sp.Quantity == 0) // Lọc sản phẩm hết hàng
                .Count();
            var totalSP = listSanPham.Count();
            // Trả về dữ liệu dưới dạng JSON, bao gồm cả danh sách sản phẩm và tổng số sản phẩm
            return Json(new { listSanPham, tongSoSanPham, totalSP = totalSP }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetListSanPhamHT()
        {
            var listSanPham = thongkeContext.Products
                .Where(sp => sp.Quantity == 0) // Lọc sản phẩm hết hàng
                .ToList();
            var totalSP = listSanPham.Count();
            // Trả về dữ liệu dưới dạng JSON, bao gồm cả danh sách sản phẩm và tổng số sản phẩm
            return Json(new { totalSP = totalSP }, JsonRequestBehavior.AllowGet);
        }

    }

}




