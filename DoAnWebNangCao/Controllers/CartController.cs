using DoAnWebNangCao.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace DoAnWebNangCao.Controllers
{
    public class CartController : Controller
    {
        DanhMucModelDataContext _context = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
        // GET: Cart
        public ActionResult Index()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            return View();
        }

        private List<CartItem> getCart()
        {
            var cart = Session["Cart"] as List<CartItem>;
            if (cart == null)
            {
                cart = new List<CartItem>();
                Session["Cart"] = cart;
            }
            return cart;
        }


        [HttpPost]
        public JsonResult AddToCart(string id)
        {
            if (Session["UserId"] == null)
            {
                return Json(new { success = false});
            }
            var productObj = GetProductByID(id);
            if (productObj == null)
            {
                return Json(new { success = false });
            }
            var cart = getCart();
            //check san pham da co trong gio hang chua
            var cartItem = cart.FirstOrDefault(p => p.ID == id);
            if (cartItem != null)
            {
                cartItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem { ID = id, Name = productObj.Name, Price = (int)productObj.Price, Quantity = 1 });

            }
            return Json(new { success = true });
        }

        public Product GetProductByID(string id)
        {
            var productObj = _context.Products.FirstOrDefault(sp => sp.ProductID == id);
            return productObj;
        }
        [HttpPost]
        public JsonResult GetCartItems()
        {
            var cart = getCart();
            return Json(getCart(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateCart(string id, int quantity)
        {
            var cart = getCart();
            var cartItem = cart.FirstOrDefault(c => c.ID == id);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                int totalPrice = cartItem.Price * cartItem.Quantity; // Tổng giá mới cho mục này
                return Json(new { success = true, totalPrice });
            }
            return Json(new { success = false });
        }

        [HttpPost]
        public JsonResult RemoveFromCart(string id)
        {
            var cart = getCart();
            var cartItem = cart.FirstOrDefault(c => c.ID == id);
            if (cartItem != null)
            {
                Debug.WriteLine(cartItem.ID + "/" + cartItem.Name + "/" + cartItem.Quantity);
                //PrintSession();
                cart.Remove(cartItem);
                //PrintSession();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        public void PrintSession()
        {
            var cart = getCart();
            foreach (var cartItem in cart)
            {
                Debug.WriteLine(cartItem.ID + "/" + cartItem.Name + "/" + cartItem.Quantity);
            }
        }
        [HttpPost]
        public JsonResult Checkout(string userId)
        {
            try
            {
                var cart = getCart();
                if (cart == null || !cart.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống." });
                }

                decimal totalPrice = cart.Sum(item => item.Price * item.Quantity);

                var order = new Order
                {
                    OrderID = Guid.NewGuid().ToString(),
                    UserID = userId,
                    OrderDate = DateTime.Now,
                    TotalPrice = totalPrice,
                    Status = "Đang xử lý"
                };
                _context.Orders.InsertOnSubmit(order);

                foreach (var item in cart)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderDetailID = Guid.NewGuid().ToString(),
                        OrderID = order.OrderID,
                        ProductID = item.ID,
                        Quantity = item.Quantity,
                        Price = item.Price * item.Quantity,
                    };
                    _context.OrderDetails.InsertOnSubmit(orderDetail);
                    var product = _context.Products.FirstOrDefault(p => p.ProductID == item.ID);
                    if (product != null)
                    {
                        // Giảm số lượng tồn kho
                        product.Quantity -= item.Quantity;
                        // Kiểm tra xem tồn kho có âm không
                        if (product.Quantity < 0)
                        {
                            return Json(new { success = false, message = $"Tồn kho sản phẩm {product.Name} không đủ." });
                        }
                    }
                }

                _context.SubmitChanges();
                Session["Cart"] = null;

                return Json(new { success = true, message = "Đơn hàng đã được lưu thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu đơn hàng: " + ex.Message });
            }
        }


    }
}