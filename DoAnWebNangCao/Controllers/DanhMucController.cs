using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DoAnWebNangCao.Models;
using System.Diagnostics;

namespace DoAnWebNangCao.Controllers
{
    public class DanhMucController : Controller
    {
        DanhMucModelDataContext _danhMucContext = new DanhMucModelDataContext("Data Source=LAPTOP-RVUKI8N0\\SQLEXPRESS;Initial Catalog=MobileShopDB_2;Integrated Security=True;TrustServerCertificate=True");
        // GET: DanhMucList
        public ActionResult Index()
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
        public JsonResult GetListDanhMuc(int pageNumber, int pageSize)
        {
            List<ProductCatalogue> danhMucList = _danhMucContext.ProductCatalogues.ToList();
            //var listDanhMuc = _danhMucContext.ProductCatalogues.ToList();
            //                 //.OrderBy(sp => sp.CatalogueName)
            //                 //.Skip((pageNumber - 1) * pageSize)
            //                 //.Take(pageSize)
            //                 //.ToList();
            var listDanhMuc = danhMucList.Select(d => new {
                CatalogueID = d.CatalogueID,
                CatalogueName = d.CatalogueName
            }).OrderBy(sp => sp.CatalogueName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
            if (listDanhMuc == null)
            {   
                Debug.WriteLine("loi");
            }


            var tongSoDanhMuc = _danhMucContext.ProductCatalogues.Count();
            return Json(new { listDanhMuc, tongSoDanhMuc }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult TaoDanhMuc(string tenDanhMuc)
        {
            try
            {
                var newDanhMuc = new ProductCatalogue();
                newDanhMuc.CatalogueID = Guid.NewGuid().ToString();
                newDanhMuc.CatalogueName = tenDanhMuc;
                _danhMucContext.ProductCatalogues.InsertOnSubmit(newDanhMuc);
                _danhMucContext.SubmitChanges();
                return Json(new { success = true, message = "Danh mục thêm thành công!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = e.Message }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult GetListDanhMucForSP()
        {
            List<ProductCatalogue> danhMucList = _danhMucContext.ProductCatalogues.ToList();
            // Kiểm tra nếu danh sách rỗng hoặc null
            if (danhMucList == null || danhMucList.Count == 0)
            {
                return Json(new { data = new List<string>() }, JsonRequestBehavior.AllowGet); // trả về danh sách rỗng
            }

            // Trả về dữ liệu dưới dạng JSON
            var result = danhMucList.Select(d => new {
                CatalogueID = d.CatalogueID,
                CatalogueName = d.CatalogueName
            }).ToList();

            return Json(new { success= true ,data = result }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetObjectByID(string id)
        {
            Debug.WriteLine(id);
            var catalogueObj = _danhMucContext.ProductCatalogues.FirstOrDefault(x => x.CatalogueID == id);
            if (catalogueObj == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại." }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true, danhMuc = catalogueObj }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public JsonResult SuaDanhMuc(string id,string ten)
        {
            var catalogueObj = _danhMucContext.ProductCatalogues.FirstOrDefault(x => x.CatalogueID == id);
            if(catalogueObj == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                catalogueObj.CatalogueName = ten;
                _danhMucContext.SubmitChanges();
                return Json(new { success = true, message = "Danh mục sửa thành công!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult XoaDanhMuc(string id)
        {
            var catalogueObj = _danhMucContext.ProductCatalogues.FirstOrDefault(x => x.CatalogueID == id);
            if (catalogueObj == null)
            {
                return Json(new { success = false, message = "Danh mục không tồn tại." });
            }

            _danhMucContext.ProductCatalogues.DeleteOnSubmit(catalogueObj);
            _danhMucContext.SubmitChanges();

            return Json(new { success = true, message = "Danh mục đã được xóa thành công." });
        }

        public JsonResult TimKiemDanhMuc(string name)
        {
            {
                var productList = _danhMucContext.ProductCatalogues
               .Where(x => x.CatalogueName.Contains(name)) // Tìm kiếm theo tên sản phẩm
               .Select(x => new
               {
                   x.CatalogueID,
                   x.CatalogueName
               })
               .ToList();
                //Debug.WriteLine(productList);
                return Json(productList, JsonRequestBehavior.AllowGet);
            }
        }


    }
}