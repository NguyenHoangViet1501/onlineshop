using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnWebNangCao.Models;

namespace DoAnWebNangCao.Controllers
{
    public class CustomerController : Controller
    {
        DanhMucModelDataContext _context = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
        // GET: Customer
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        public JsonResult Details(string userId)
        {
            var user = _context.Users
                .Where(u => u.UserID == userId)
                .Select(u => new
                {
                    u.Account,
                    u.Name,
                    u.PhoneNumber,
                    u.Address,
                    u.Email
                })
                .FirstOrDefault();

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, data = user }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChangePassword()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        [HttpPost]
        public JsonResult PostChangePassword(string currentPassword, string newPassword)
        {
            var userId = Session["UserId"] as string;
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập." });
            }

            var user = _context.Users.FirstOrDefault(u => u.UserID == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy người dùng." });
            }

            var hashedCurrentPassword = HashPassword(currentPassword);
            if (user.Password != hashedCurrentPassword)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            var hashedNewPassword = HashPassword(newPassword);
            user.Password = hashedNewPassword;
            _context.SubmitChanges();

            return Json(new { success = true, message = "Đổi mật khẩu thành công." });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public ActionResult OrderHistory()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        public JsonResult GetOrderHistory()
        {
            var userId = Session["UserId"] as string;
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            var orders = _context.Orders
             .Where(o => o.UserID == userId)
             .Select(o => new
             {
                 o.OrderID,
                 o.OrderDate,
                 o.TotalPrice,
                 o.Status
             })
             .ToList()
             .Select(o => new
             {
                 o.OrderID,
                 OrderDate = o.OrderDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                 o.TotalPrice,
                 o.Status
             })
             .ToList();

            return Json(new { success = true, orders }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetOrderDetails(string orderId)
        {
            var userId = Session["UserId"] as string;
            if (userId == null)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập." }, JsonRequestBehavior.AllowGet);
            }

            var orderDetails = _context.OrderDetails
                .Where(od => od.OrderID == orderId)
                .Join(_context.Products,
                    od => od.ProductID,
                    p => p.ProductID,
                    (od, p) => new
                    {
                        p.Name,
                        od.Quantity,
                        od.Price
                    })
                .ToList();

            return Json(new { success = true, orderDetails }, JsonRequestBehavior.AllowGet);
        }
    }
}