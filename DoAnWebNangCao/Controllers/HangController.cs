using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnWebNangCao.Models;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;

namespace DoAnWebNangCao.Controllers
{
    public class HangController : Controller
    {
        DanhMucModelDataContext _HangContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
        // GET: DanhMucList
        public ActionResult Index()
        {
            // Kiểm tra xem UserId có tồn tại trong session hay không (nghĩa là người dùng đã đăng nhập)
            if (Session["UserId"] == null)
            {
                // Nếu session chưa được thiết lập (chưa đăng nhập), chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Login");
            }
            else
            {
                // Kiểm tra RoleId (ví dụ bạn muốn RoleId là 1 cho Quản lý, 2 cho Nhân viên, hoặc bất kỳ giá trị nào)
                var roleId = Session["RoleId"];

                if (roleId != null && roleId.Equals(true)) // Giả sử RoleId = 1 là Quản lý
                {
                    // Nếu RoleId là Quản lý (RoleId = 1), cho phép truy cập vào trang Index
                    return View();
                }
                else
                {
                    // Nếu không phải Quản lý (hoặc RoleId không phải giá trị bạn mong muốn), chuyển hướng đến trang không có quyền truy cập
                    return RedirectToAction("Index", "IndexKH");
                }
            }
        }

        public ActionResult Details(string id)
        {
            if (Session["UserId"] == null)
            {
                // Nếu session chưa được thiết lập (chưa đăng nhập), chuyển hướng đến trang đăng nhập
                return RedirectToAction("Login", "Login");
            }
            else
            {
                // Kiểm tra RoleId (ví dụ bạn muốn RoleId là 1 cho Quản lý, 2 cho Nhân viên, hoặc bất kỳ giá trị nào)
                var roleId = Session["RoleId"];

                if (roleId != null && roleId.Equals(true)) // Giả sử RoleId = 1 là Quản lý
                {
                    // Nếu RoleId là Quản lý (RoleId = 1), cho phép truy cập vào trang Index
                    var newobj = _HangContext.ProductBrands.FirstOrDefault(x => x.BrandID == id);
                    return View(newobj);
                }
                else
                {
                    // Nếu không phải Quản lý (hoặc RoleId không phải giá trị bạn mong muốn), chuyển hướng đến trang không có quyền truy cập
                    return RedirectToAction("Index", "IndexKH");
                }
            }

        }
        [Route("SanPham/Edit/{id}")]


        [Route("SanPham/Edit/{id}")]


        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public JsonResult PostCreate()
        {
            try
            {
                // Lấy các giá trị từ form
                string idDanhMuc = Request.Form["IdDanhMuc"];
                string Name = Request.Form["BrandName"];
                // Tạo đối tượng mới cho sản phẩm
                ProductBrand newObj = new ProductBrand
                {
                    BrandID = Guid.NewGuid().ToString(),
                    CatalogueID = idDanhMuc,
                    BrandName = Name,

                };
                // Lưu sản phẩm vào cơ sở dữ liệu
                _HangContext.ProductBrands.InsertOnSubmit(newObj);
                _HangContext.SubmitChanges();

                return Json(new
                {
                    success = true,
                    message = "Thêm mới thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetListHang(int pageNumber, int pageSize)
        {
            Debug.WriteLine("Đã đến đây");

            // Truy vấn kết hợp giữa bảng Products và ProductCatalogues
            var listBrand = _HangContext.ProductBrands
                .Join(_HangContext.ProductCatalogues,  // Thực hiện JOIN với bảng ProductCatalogues
                      pb => pb.CatalogueID,  // Trường CatalogueID trong bảng Products
                      pc => pc.CatalogueID,  // Trường CatalogueID trong bảng ProductCatalogues
                      (pb, pc) => new
                      {
                          pb.BrandID,
                          pb.BrandName,
                          CatalogueName = pc.CatalogueName,  // Lấy tên danh mục từ bảng ProductCatalogues
                      })
                .OrderBy(sp => sp.BrandID)
                .Skip((pageNumber - 1) * pageSize)  // Thực hiện phân trang
                .Take(pageSize)  // Giới hạn số lượng sản phẩm theo pageSize
                .ToList();

            if (listBrand == null || listBrand.Count == 0)
            {
                Debug.WriteLine("Không có hãng.");
            }

            // Lấy tổng số sản phẩm
            var tongSoDanhMuc = _HangContext.Products.Count();

            // Trả về dữ liệu dưới dạng JSON, bao gồm cả danh sách sản phẩm và tổng số sản phẩm
            return Json(new { listBrand, tongSoDanhMuc }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBrandByName(string name)
        {
            Debug.WriteLine(name);
            var productList = _HangContext.ProductBrands
                .Where(x => x.BrandName.Contains(name))
                .ToList(); // Lấy tất cả sản phẩm có tên chứa từ khóa

            if (productList == null || !productList.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy hãng nào." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, listSanPham = productList, tongSoDanhMuc = productList.Count }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult XoaHang(string id)
        {
            var productsObj = _HangContext.ProductBrands.FirstOrDefault(x => x.BrandID == id);
            if (productsObj == null)
            {
                return Json(new { success = false, message = "Hãng không tồn tại." });
            }

            _HangContext.ProductBrands.DeleteOnSubmit(productsObj);
            _HangContext.SubmitChanges();

            return Json(new { success = true, message = "Hãng đã được xóa thành công." });
        }
        public JsonResult Paging(int pageSize = 1, int crrPage = 1)
        {
            List<Product> products = _HangContext.Products
                .Skip((crrPage - 1) * pageSize)//skip so luong ban ghi
                .Take(pageSize)//lay bao nhieu ban ghi
                .ToList();
            return Json(new
            {
                data = products,
                success = true,
                message = "Lay du lieu thanh cong",
                crrPage = crrPage,
                pageSize = pageSize,
                totalPage = Math.Ceiling((double)_HangContext.Products.Count() / pageSize)//
            });

        }
        [HttpPost]
        public ActionResult PostEdit(FormCollection form)
        {
            try
            {
                string maHang = Request.Form["MaNhanHang"];
                string tenHang = Request.Form["TenNhanHang"];
                string idDanhMuc = Request.Form["IdDanhMuc"];
                // Lấy các giá trị từ form

                var Hang = _HangContext.ProductBrands.FirstOrDefault(x => x.BrandID == maHang);
                if (Hang != null)
                {
                    Hang.CatalogueID = idDanhMuc;
                    Hang.BrandName = tenHang;
                    // Cập nhật sản phẩm vào cơ sở dữ liệu
                    _HangContext.SubmitChanges();
                }
                return Json(new
                {
                    success = true,
                    message = "Cập nhật sản phẩm thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult Edit(string id)
        {
            var newobj = _HangContext.ProductBrands.FirstOrDefault(x => x.BrandID == id);
            return View(newobj);
        }        
        public JsonResult SearchBrandsByName(string name, int pageNumber = 1, int pageSize = 8)
        {
            // Lọc sản phẩm theo tên và phân trang
            var productList = _HangContext.ProductBrands
                .Where(sp => sp.BrandName.Contains(name)) // Tìm kiếm theo tên sản phẩm
                .Join(_HangContext.ProductCatalogues,  // Thực hiện JOIN với bảng ProductCatalogues
                      sp => sp.CatalogueID,
                      pc => pc.CatalogueID,
                      (sp, pc) => new
                      {
                          sp.BrandID,
                          sp.BrandName,
                          sp.CatalogueID,
                          CatalogueName = pc.CatalogueName,  // Lấy tên danh mục từ bảng ProductCatalogues
                      })
                .OrderBy(sp => sp.BrandID)  // Sắp xếp theo ProductID
                .Skip((pageNumber - 1) * pageSize)  // Skip các bản ghi đã được lấy ở các trang trước
                .Take(pageSize)  // Lấy số bản ghi theo pageSize
                .ToList();

            // Lấy tổng số sản phẩm tìm thấy
            var totalProducts = _HangContext.ProductBrands
                .Where(sp => sp.BrandName.Contains(name))
                .Count();

            return Json(new { listSanPham = productList, tongSoDanhMuc = totalProducts }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetBrandByCatalogueId(string catalogueId)
        {
            // Kiểm tra xem catalogueId có đúng không
            System.Diagnostics.Debug.WriteLine("CatalogueID: " + catalogueId);

            // Lấy danh sách sản phẩm từ cơ sở dữ liệu theo CatalogueID
            var brandList = _HangContext.ProductBrands
                                            .Where(p => p.CatalogueID == catalogueId)
                                            .Select(p => new
                                            {
                                                p.BrandID,
                                                p.BrandName,
                                                p.CatalogueID,
                                            })
                                            .ToList();

            // Nếu không có sản phẩm, trả về danh sách rỗng
            if (brandList == null || brandList.Count == 0)
            {
                return Json(new { data = new List<string>() }, JsonRequestBehavior.AllowGet);
            }

            // Trả về dữ liệu sản phẩm dưới dạng JSON
            return Json(new { data = brandList }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAllProducts()
        {
            List<ProductBrand> danhMucList = _HangContext.ProductBrands.ToList();
            // Kiểm tra nếu danh sách rỗng hoặc null
            if (danhMucList == null || danhMucList.Count == 0)
            {
                return Json(new { data = new List<string>() }, JsonRequestBehavior.AllowGet); // trả về danh sách rỗng
            }

            // Trả về dữ liệu dưới dạng JSON
            var result = danhMucList.Select(d => new
            {
                id = d.BrandID,
                name = d.BrandName,
            }).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetListBrandForSP()
        {
            List<ProductBrand> danhMucList = _HangContext.ProductBrands.ToList();
            // Kiểm tra nếu danh sách rỗng hoặc null
            if (danhMucList == null || danhMucList.Count == 0)
            {
                return Json(new { data = new List<string>() }, JsonRequestBehavior.AllowGet); // trả về danh sách rỗng
            }

            // Trả về dữ liệu dưới dạng JSON
            var result = danhMucList.Select(d => new {
                CatalogueID = d.CatalogueID,
                BrandID = d.BrandID,
                BrandName = d.BrandName
            }).ToList();

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }





    }
}
