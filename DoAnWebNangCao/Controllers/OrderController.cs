using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnWebNangCao.Models;

namespace DoAnWebNangCao.Controllers
{
    public class OrderController : Controller
    {
        DanhMucModelDataContext _context = new DanhMucModelDataContext("Data Source=AHNGUYEN\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");

        // GET: Order
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        public ActionResult GetOrders(string customerName, DateTime? startDate, DateTime? endDate, string status)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var orders = _context.Orders
                .Join(_context.Users, o => o.UserID, u => u.UserID, (o, u) => new
                {
                    o.OrderID,
                    UserName = u.Name,
                    o.OrderDate,
                    o.TotalPrice,
                    o.Status
                });

            if (!string.IsNullOrEmpty(customerName))
            {
                orders = orders.Where(o => o.UserName.Contains(customerName));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
            }

            if (!string.IsNullOrEmpty(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            var result = orders.ToList().Select(o => new
            {
                o.OrderID,
                o.UserName,
                OrderDate = o.OrderDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                o.TotalPrice,
                o.Status
            });

            return Json(result);
        }

        [HttpPost]
        public ActionResult GetOrderDetails(string orderId)
        {
            var details = _context.OrderDetails
                .Join(_context.Products, d => d.ProductID, p => p.ProductID, (d, p) => new
                {
                    d.OrderID,
                    ProductName = p.Name,
                    d.Quantity,
                    d.Price
                })
                .Where(d => d.OrderID == orderId)
                .ToList();

            return Json(details);
        }

        [HttpPost]
        public ActionResult UpdateOrderStatus(string orderId, string status)
        {
            var order = _context.Orders.SingleOrDefault(o => o.OrderID == orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // Cập nhật trạng thái đơn hàng
            if (status == "Đã huỷ" && order.Status != "Đã huỷ")
            {
                var orderDetails = _context.OrderDetails.Where(d => d.OrderID == orderId).ToList();
                foreach (var detail in orderDetails)
                {
                    var product = _context.Products.SingleOrDefault(p => p.ProductID == detail.ProductID);
                    if (product != null)
                    {
                        product.Quantity += detail.Quantity; // Tăng tồn kho
                    }
                }
            }

            order.Status = status;
            _context.SubmitChanges();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
        }

        public ActionResult Details(string id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var roleId = Session["RoleId"];

            if (roleId != null && roleId.Equals(true))
            {
                var details = _context.OrderDetails
                    .Join(_context.Products, d => d.ProductID, p => p.ProductID, (d, p) => new
                    {
                        d.OrderID,
                        ProductName = p.Name,
                        d.Quantity,
                        d.Price
                    })
                    .Where(d => d.OrderID == id)
                    .ToList();

                return View(details);
            }
            else
            {
                return RedirectToAction("Index", "Order");
            }
        }
    }
}