using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DoAnWebNangCao.Models;

namespace DoAnWebNangCao.Controllers
{
    public class LoginController : Controller
    {
        DanhMucModelDataContext _loginContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");

        // GET: Login
        public ActionResult Index()
        {
            return View();
        }

        // GET: Login (Trang đăng nhập)
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login (Xử lý đăng nhập)
        [HttpPost]

        public JsonResult Login(string account, string password)
        {
            string _password = HashPassword(password);
            // Kiểm tra tài khoản và mật khẩu
            var user = _loginContext.Users.FirstOrDefault(u => u.Account == account && u.Password == _password);

            if (user != null)
            {
                // Đăng nhập thành công, lưu thông tin vào session
                Session["UserId"] = user.UserID;
                Session["Account"] = user.Account;
                Session["RoleId"] = user.RoleID;
                Session["Name"] = user.Name;  // Lưu tên người dùng vào session nếu cần

                // Kiểm tra roleId để chuyển hướng người dùng
                if (user.RoleID == false)  // Quản lý
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "IndexKH"), name = user.Name });
                }
                else if (user.RoleID == true)  // Khach hang
                {
                    return Json(new { success = true, redirectUrl = Url.Action("Index", "SanPham"), name = user.Name });
                }
            }

            // Đăng nhập thất bại
            return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không đúng." });
        }


        // POST: Account/Logout
        public ActionResult Logout()
        {
            // Xóa session khi người dùng đăng xuất
            Session.Clear();
            return RedirectToAction("Login");
        }
        [HttpPost]
        public JsonResult Register(string account, string password, string name, string phonenumber, string address, string email)
        {
            try
            {
                var existingUser = _loginContext.Users.FirstOrDefault(u => u.Account == account&&u.Email == email&&u.PhoneNumber==phonenumber);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Tài khoản đã tồn tại." });
                }

                var newUser = new User
                {
                    UserID = Guid.NewGuid().ToString(),  // Tạo ID ngẫu nhiên cho UserID
                    Account = account,
                    Password = HashPassword(password),  // Mã hóa mật khẩu
                                                      // Password = password,
                    Name = name,
                    Email = email,
                    PhoneNumber = phonenumber,
                    Address = address,
                    RoleID = false
                };

                _loginContext.Users.InsertOnSubmit(newUser);
                _loginContext.SubmitChanges();

                return Json(new { success = true, message = "Đăng ký thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
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
    }
}
