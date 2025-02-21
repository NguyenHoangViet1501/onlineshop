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
    public class SanPhamController : Controller
    {
        DanhMucModelDataContext _sanPhamContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
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
                    var newobj = _sanPhamContext.Products.FirstOrDefault(x => x.ProductID == id);
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

        public JsonResult GetProductDetail(string id)
        {
            try
            {
                var product = (from p in _sanPhamContext.Products
                               join c in _sanPhamContext.ProductCatalogues
                               on p.CatalogueID equals c.CatalogueID
                               join b in _sanPhamContext.ProductBrands
                               on p.BrandID equals b.BrandID

                               where p.ProductID == id
                               select new
                               {
                                   BrandName = b.BrandName,
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

        [Route("SanPham/Edit/{id}")]


        public ActionResult Create()
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
                    return View();
                }
                else
                {
                    // Nếu không phải Quản lý (hoặc RoleId không phải giá trị bạn mong muốn), chuyển hướng đến trang không có quyền truy cập
                    return RedirectToAction("Index", "IndexKH");
                }
            }
        }
        [HttpPost]
        public JsonResult PostCreate()
        {
            try
            {
                // Lấy các giá trị từ form
                string tenSanPham = Request.Form["TenSanPham"];
                string idDanhMuc = Request.Form["IdDanhMuc"];
                string gia = Request.Form["Gia"];
                int giaTri = Convert.ToInt32(gia);
                string moTa = Request.Form["MoTa"];
                string hang = Request.Form["BrandId"];  // Đảm bảo tên trường giống như trong formData.append
                string soLuong = Request.Form["SoLuong"];
                int soLuongInt = Convert.ToInt32(soLuong);
                string mauSac = Request.Form["Color"];
                string size = Request.Form["Size"];

                // Lấy file hình ảnh từ request
                var imageFile = Request.Files["ImageFile"];

                // Kiểm tra nếu không có file
                if (imageFile == null || imageFile.ContentLength == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không có hình ảnh được chọn"
                    }, JsonRequestBehavior.AllowGet);
                }

                // Tạo đối tượng mới cho sản phẩm
                Product newObj = new Product
                {
                    ProductID = Guid.NewGuid().ToString(),
                    Name = tenSanPham,
                    CatalogueID = idDanhMuc,
                    Price = giaTri,
                    Description = moTa,
                    Quantity = soLuongInt,
                    color = mauSac,
                    size = size,
                    BrandID = hang,

                };

                // Kiểm tra thư mục lưu hình ảnh
                var imageFolderPath = Server.MapPath("~/Images/");
                if (!Directory.Exists(imageFolderPath))
                {
                    Directory.CreateDirectory(imageFolderPath);
                }

                // Lưu file vào thư mục "Images"
                var fileName = Path.GetFileName(imageFile.FileName);
                var filePath = Path.Combine(imageFolderPath, fileName);
                imageFile.SaveAs(filePath);

                // Lưu đường dẫn hình ảnh vào database
                newObj.Image_path = "/Images/" + fileName;

                // Lưu sản phẩm vào cơ sở dữ liệu
                _sanPhamContext.Products.InsertOnSubmit(newObj);
                _sanPhamContext.SubmitChanges();

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
        public JsonResult GetListSanPham(int pageNumber, int pageSize)
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
    .OrderBy(sp => sp.ProductID)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToList();


            if (listSanPham == null || listSanPham.Count == 0)
            {
                Debug.WriteLine("Không có sản phẩm.");
            }

            // Lấy tổng số sản phẩm
            var tongSoDanhMuc = _sanPhamContext.Products.Count();

            // Trả về dữ liệu dưới dạng JSON, bao gồm cả danh sách sản phẩm và tổng số sản phẩm
            return Json(new { listSanPham, tongSoDanhMuc }, JsonRequestBehavior.AllowGet);
        }





        public JsonResult TaoSanPham(string tenDanhMuc)
        {
            try
            {
                var newDanhMuc = new ProductCatalogue();
                newDanhMuc.CatalogueName = tenDanhMuc;
                _sanPhamContext.ProductCatalogues.InsertOnSubmit(newDanhMuc);
                _sanPhamContext.SubmitChanges();
                return Json(new { success = true, message = "Danh mục thêm thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult GetProductByName(string name)
        {
            Debug.WriteLine(name);
            var productList = _sanPhamContext.Products
                .Where(x => x.Name.Contains(name))
                .ToList(); // Lấy tất cả sản phẩm có tên chứa từ khóa

            if (productList == null || !productList.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm nào." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, listSanPham = productList, tongSoDanhMuc = productList.Count }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SuaDanhMuc(string id, string ten)
        {
            var catalogueObj = _sanPhamContext.ProductCatalogues.FirstOrDefault(x => x.CatalogueID == id);
            if (catalogueObj == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                catalogueObj.CatalogueName = ten;
                _sanPhamContext.SubmitChanges();
                return Json(new { success = true, message = "Danh mục sửa thành công!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult XoaSanPham(string id)
        {
            var productsObj = _sanPhamContext.Products.FirstOrDefault(x => x.ProductID == id);
            if (productsObj == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại." });
            }

            _sanPhamContext.Products.DeleteOnSubmit(productsObj);
            _sanPhamContext.SubmitChanges();

            return Json(new { success = true, message = "Sản phẩm đã được xóa thành công." });
        }
        public JsonResult Paging(int pageSize = 1, int crrPage = 1)
        {
            List<Product> products = _sanPhamContext.Products
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
                totalPage = Math.Ceiling((double)_sanPhamContext.Products.Count() / pageSize)//
            });

        }
        [HttpPost]
        public ActionResult PostEdit(FormCollection form)
        {
            try
            {
                string maSanPham = Request.Form["MaSanPham"];
                string tenSanPham = Request.Form["TenSanPham"];
                string idDanhMuc = Request.Form["IdDanhMuc"];
                string gia = Request.Form["Gia"];
                int giaTri = Convert.ToInt32(gia);
                string moTa = Request.Form["MoTa"];
                string hang = Request.Form["Branch"];
                string soLuong = Request.Form["SoLuong"];
                int soLuongInt = Convert.ToInt32(soLuong);
                string mauSac = Request.Form["Color"];
                string size = Request.Form["Size"];
                // Lấy các giá trị từ form

                var product = _sanPhamContext.Products.FirstOrDefault(x => x.ProductID == maSanPham);
                if (product != null)
                {
                    product.Name = tenSanPham;
                    product.CatalogueID = idDanhMuc;
                    product.Price = giaTri;
                    product.Description = moTa;
                    product.Quantity = soLuongInt;
                    product.color = mauSac;
                    product.size = size;

                    var imageFile = Request.Files["ImageFile"];

                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        // Kiểm tra thư mục lưu hình ảnh
                        var imageFolderPath = Server.MapPath("~/Images/");
                        if (!Directory.Exists(imageFolderPath))
                        {
                            Directory.CreateDirectory(imageFolderPath);
                        }

                        // Lưu file vào thư mục "Images"
                        var fileName = Path.GetFileName(imageFile.FileName);
                        var filePath = Path.Combine(imageFolderPath, fileName);
                        imageFile.SaveAs(filePath);

                        // Cập nhật đường dẫn hình ảnh vào cơ sở dữ liệu
                        product.Image_path = "/Images/" + fileName;
                    }

                    // Cập nhật sản phẩm vào cơ sở dữ liệu
                    _sanPhamContext.SubmitChanges();
                }



                // Lấy sản phẩm từ cơ sở dữ liệu




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
            var newobj = _sanPhamContext.Products.FirstOrDefault(x => x.ProductID == id);
            return View(newobj);
        }
        public JsonResult SearchProductsByName(string name, int pageNumber = 1, int pageSize = 8)
        {
            // Lọc sản phẩm theo tên và phân trang
            var productList = _sanPhamContext.Products
                .Where(sp => sp.Name.Contains(name)) // Tìm kiếm theo tên sản phẩm
                .Join(_sanPhamContext.ProductCatalogues,  // Thực hiện JOIN với bảng ProductCatalogues
                      sp => sp.CatalogueID,
                      pc => pc.CatalogueID,
                      (sp, pc) => new
                      {
                          sp.ProductID,
                          sp.Name,
                          sp.CatalogueID,
                          sp.Price,
                          sp.Description,                        
                          sp.Quantity,
                          CatalogueName = pc.CatalogueName,  // Lấy tên danh mục từ bảng ProductCatalogues
                          ImagePath = sp.Image_path // Đảm bảo đường dẫn ảnh hợp lệ
                      })
                .OrderBy(sp => sp.ProductID)  // Sắp xếp theo ProductID
                .Skip((pageNumber - 1) * pageSize)  // Skip các bản ghi đã được lấy ở các trang trước
                .Take(pageSize)  // Lấy số bản ghi theo pageSize
                .ToList();

            // Lấy tổng số sản phẩm tìm thấy
            var totalProducts = _sanPhamContext.Products
                .Where(sp => sp.Name.Contains(name))
                .Count();

            return Json(new { listSanPham = productList, tongSoDanhMuc = totalProducts }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetProductsByBrandIDandCatalogueID(string BrandID, string CatalogueID)
        {
            // Lấy danh sách sản phẩm từ cơ sở dữ liệu theo CatalogueID
            var productList = _sanPhamContext.Products
                                            .Where(p => p.BrandID == BrandID && p.CatalogueID == CatalogueID)
                                            .Select(p => new
                                            {
                                                p.ProductID,
                                                p.Name,
                                                p.Price,
                                                p.Image_path,
                                                p.Quantity,
                                                
                                            })
                                            .ToList();

            // Nếu không có sản phẩm, trả về danh sách rỗng
            if (productList == null || productList.Count == 0)
            {
                return Json(new { data = new List<string>() }, JsonRequestBehavior.AllowGet);
            }

            // Trả về dữ liệu sản phẩm dưới dạng JSON
            return Json(new { data = productList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllProducts()
        {
            List<Product> danhMucList = _sanPhamContext.Products.ToList();
            // Kiểm tra nếu danh sách rỗng hoặc null
            if (danhMucList == null || danhMucList.Count == 0)
            {
                return Json(new { data = new List<string>() }, JsonRequestBehavior.AllowGet); // trả về danh sách rỗng
            }

            // Trả về dữ liệu dưới dạng JSON
            var result = danhMucList.Select(d => new {
                id = d.ProductID,
                name = d.Name,
                price = d.Price,
                image_path = d.Image_path,
                
            }).ToList();

            return Json(new { data = result }, JsonRequestBehavior.AllowGet);
        }




    }
}
