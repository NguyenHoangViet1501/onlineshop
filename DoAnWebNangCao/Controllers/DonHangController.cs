using DoAnWebNangCao.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DoAnWebNangCao.Controllers
{
    public class DonHangController : Controller
    {
        DanhMucModelDataContext donHangContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
        // GET: DonHang
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("DonHang/DonHangNgay")]
        public ActionResult DonHangNgay(DateTime? date)  // Chỉnh sửa để nhận Nullable DateTime
        {
            try
            {
                if (date.HasValue)
                {
                    // Lọc các đơn hàng trong ngày đã chọn
                    var ordersInDay = donHangContext.Orders.ToList();

                    return View(ordersInDay); // Trả về view "DonHangNgay" với danh sách đơn hàng trong ngày
                }

                ViewBag.ErrorMessage = "Ngày không hợp lệ!";
                return View();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, trả về view với thông báo lỗi
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        [HttpGet]
        [Route("DonHang/DonHangNgay")]

        public JsonResult GetOrdersInDay(string date)
        {
            try
            {
                // Kiểm tra xem giá trị "date" có được truyền vào hay không
                DateTime orderDate = DateTime.Parse(date); // Chuyển đổi chuỗi ngày thành DateTime
                Console.WriteLine(orderDate); // In ra log để kiểm tra

                var ordersInDay = donHangContext.Orders
                    .Where(o => o.OrderDate.Date == orderDate.Date&& o.Status!="Đã huỷ") // Lọc đơn hàng theo ngày
                    .OrderBy(o => o.OrderDate) // Sắp xếp theo ngày
                    .Select(o => new
                    {
                        OrderID = o.OrderID,
                        OrderDate = o.OrderDate,
                        TotalPrice = o.TotalPrice,
                        Status = o.Status
                    })
                    .ToList();

                return Json(ordersInDay, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi khi lấy dữ liệu: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}