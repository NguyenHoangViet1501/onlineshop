using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.IO;
using System.Drawing.Printing;
using DoAnWebNangCao.Models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Principal;
using System.Web.Helpers;

namespace QuanLyTaiKhoan.Controllers
{
    public class TaiKhoanController : Controller
    {
        DanhMucModelDataContext _dataShopDataContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");

        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                // Nếu session chưa được thiết lập (chưa đăng nhập), chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Login");
            }
            else
            {
                // Kiểm tra RoleId (ví dụ bạn muốn RoleId là true cho Quản lý, 2 cho Nhân viên, hoặc bất kỳ giá trị nào)
                var roleId = Session["RoleId"];

                if (roleId != null && roleId.Equals(true)) // Giả sử RoleId = true là Quản lý
                {
                    // Nếu RoleId là Quản lý (RoleId = true), cho phép truy cập vào trang Index
                    return View();
                }
                else
                {
                    // Nếu không phải Quản lý (hoặc RoleId không phải giá trị bạn mong muốn), chuyển hướng đến trang không có quyền truy cập
                    return RedirectToAction("Index", "IndexKH");
                }
            }
        }
        public ActionResult Create()
        {
            if (Session["UserId"] == null)
            {
                // Nếu session chưa được thiết lập (chưa đăng nhập), chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Login");
            }
            else
            {
                // Kiểm tra RoleId (ví dụ bạn muốn RoleId là true cho Quản lý, 2 cho Nhân viên, hoặc bất kỳ giá trị nào)
                var roleId = Session["RoleId"];

                if (roleId != null && roleId.Equals(true)) // Giả sử RoleId = true là Quản lý
                {
                    // Nếu RoleId là Quản lý (RoleId = true), cho phép truy cập vào trang Index
                    return View();
                }
                else
                {
                    // Nếu không phải Quản lý (hoặc RoleId không phải giá trị bạn mong muốn), chuyển hướng đến trang không có quyền truy cập
                    return RedirectToAction("Index", "IndexKH");
                }
            }
        }
        public ActionResult Edit()
        {
            if (Session["UserId"] == null)
            {
                // Nếu session chưa được thiết lập (chưa đăng nhập), chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Login");
            }
            else
            {
                // Kiểm tra RoleId (ví dụ bạn muốn RoleId là true cho Quản lý, 2 cho Nhân viên, hoặc bất kỳ giá trị nào)
                var roleId = Session["RoleId"];

                if (roleId != null && roleId.Equals(true)) // Giả sử RoleId = true là Quản lý
                {
                    // Nếu RoleId là Quản lý (RoleId = true), cho phép truy cập vào trang Index
                    return View();
                }
                else
                {
                    // Nếu không phải Quản lý (hoặc RoleId không phải giá trị bạn mong muốn), chuyển hướng đến trang không có quyền truy cập
                    return RedirectToAction("Index", "IndexKH");
                }
            }
        }
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetListUsers(int pageNumber, int pageSize)
        {
            Debug.WriteLine("Đã đến đây");

            // Truy vấn kết hợp giữa bảng Products và ProductCatalogues
            var listUsers = _dataShopDataContext.Users
                .Join(_dataShopDataContext.Roles,  // Thực hiện JOIN với bảng ProductCatalogues
                      us => us.RoleID,  // Trường CatalogueID trong bảng Products
                      rl => rl.RoleID,  // Trường CatalogueID trong bảng ProductCatalogues
                      (us, rl) => new
                      {
                          us.UserID,
                          us.Account,
                          us.Password,
                          RoleName = rl.RoleName,
                          us.Name,
                          us.PhoneNumber,
                          us.Address,
                          us.Email,
                      })
                .OrderBy(us => us.UserID)
                .Skip((pageNumber - 1) * pageSize)  // Thực hiện phân trang
                .Take(pageSize)  // Giới hạn số lượng sản phẩm theo pageSize
                .ToList();

            if (listUsers == null || listUsers.Count == 0)
            {
                Debug.WriteLine("Không có sản phẩm.");
            }

            // Lấy tổng số sản phẩm
            var tongSoDanhMuc = _dataShopDataContext.Users.Count();

            // Trả về dữ liệu dưới dạng JSON, bao gồm cả danh sách sản phẩm và tổng số sản phẩm
            return Json(new { listUsers, tongSoDanhMuc }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetUserById(string id)
        {
            var user = _dataShopDataContext.Users.FirstOrDefault(x => x.UserID == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, user = new { user.UserID, user.Account, user.Password, user.RoleID, user.Name ,user.PhoneNumber, user.Address, user.Email } }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SuaTaiKhoan(string ID, string Account, string Password, string Role, string Name, string Phone, string Address, string Email)
        {
            var userObj = _dataShopDataContext.Users.FirstOrDefault(x => x.UserID == ID);
            if (userObj == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            // Update the user properties
            userObj.Account = Account;
            userObj.Name = Name;
            userObj.PhoneNumber = Phone;
            userObj.Address = Address;
            userObj.Email = Email;
            _dataShopDataContext.SubmitChanges();
            return Json(new { success = true, message = "Sửa tài khoản thành công!" });
        }

        [HttpPost]
        public JsonResult XoaTaiKhoan(string id)
        {
            var UserObj = _dataShopDataContext.Users.FirstOrDefault(x => x.UserID == id);
            if (UserObj == null)
            {
                return Json(new { success = false, message = "Không có tài khoản." });
            }

            _dataShopDataContext.Users.DeleteOnSubmit(UserObj);
            _dataShopDataContext.SubmitChanges();

            return Json(new { success = true, message = "Tài khoản đã được xóa thành công." });
        }
        private string HashPassword(string password)
        {
            //Thực hiện mã hóa mật khẩu bằng SHA256
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
        [HttpPost]
        public JsonResult ThemTaiKhoan(string Account, string password, string Role, string Name, string Phone, string Address, string Email)
        {
            try
            {
                var existingUser = _dataShopDataContext.Users.FirstOrDefault(u => u.Account == Account);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Tài khoản đã tồn tại." });
                }
                var newUser = new User
                {
                    UserID = Guid.NewGuid().ToString(),
                    Account = Account,
                    Password = HashPassword(password),
                    RoleID = (Role == "Admin") ? true : false, // Chuyển đổi "Admin" => true, "Customer" => false
                    Name = Name,
                    PhoneNumber = Phone,
                    Address = Address,
                    Email = Email
                };

                _dataShopDataContext.Users.InsertOnSubmit(newUser);
                _dataShopDataContext.SubmitChanges();

                return Json(new { success = true, message = "Thêm tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }

        }
        //TÌM KIẾM
        public JsonResult SearchUsersByName(string name, int pageNumber, int pageSize)
        {
            var userList = _dataShopDataContext.Users
                .Where(us => us.Name.Contains(name))
                .Join(_dataShopDataContext.Roles,  // Thực hiện JOIN với bảng ProductCatalogues
                      us => us.RoleID,  // Trường CatalogueID trong bảng Products
                      rl => rl.RoleID,  // Trường CatalogueID trong bảng ProductCatalogues
                      (us, rl) => new
                      {
                          us.UserID,
                          us.Account,
                          us.Password,
                          RoleName = rl.RoleName,
                          us.Name,
                          us.PhoneNumber,
                          us.Address,
                          us.Email,
                      })
                .OrderBy(us => us.UserID)
                .Skip((pageNumber - 1) * pageSize)  // Thực hiện phân trang
                .Take(pageSize)  // Giới hạn số lượng sản phẩm theo pageSize
                .ToList();
            var totalUsers = _dataShopDataContext.Users
                .Where(us => us.Name.Contains(name))
                .Count();
            return Json(new { listUsers = userList, tongSoDanhMuc = totalUsers }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ResetPassword(string id, string newPassword)
        {
            try
            {
                var user = _dataShopDataContext.Users.SingleOrDefault(u => u.UserID == id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản." });
                }
                user.Password = HashPassword(newPassword);
                _dataShopDataContext.SubmitChanges();

                return Json(new { success = true, message = "Đặt lại mật khẩu thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        public ActionResult TKAdmin()
        {
            if (Session["Account"] == null)
            {
                return RedirectToAction("Login", "Login"); // Chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
            }

            var account = Session["Account"].ToString(); // Lấy tài khoản từ Session
            var user = _dataShopDataContext.Users.FirstOrDefault(u => u.Account == account);

            if (user == null)
            {
                return RedirectToAction("Index", "IndexKH"); // Nếu không tìm thấy tài khoản, chuyển đến trang khác
            }

            return View(user); // Trả về View TKAdmin kèm theo thông tin admin
        }
        [HttpPost]
        public JsonResult DoiMatKhau(string currentPassword, string newPassword)
        {
            if (Session["Account"] == null)
            {
                return Json(new { success = false, message = "Bạn chưa đăng nhập!" });
            }

            var account = Session["Account"].ToString();
            var user = _dataShopDataContext.Users.FirstOrDefault(u => u.Account == account);

            if (user == null)
            {
                return Json(new { success = false, message = "Tài khoản không tồn tại." });
            }

            // Kiểm tra mật khẩu cũ
            var hashedCurrentPassword = HashPassword(currentPassword);
            if (user.Password != hashedCurrentPassword)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            // Cập nhật mật khẩu mới
            user.Password = HashPassword(newPassword);
            _dataShopDataContext.SubmitChanges();

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

    }
}