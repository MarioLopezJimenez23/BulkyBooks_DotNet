using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objproductList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category").ToList();
            return View(objproductList);
        }

        public IActionResult Upsert(int? id) //Update+Insert
        {
            IEnumerable<SelectListItem> categoriesLst = _unitOfWork.CategoryRepository.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            //ViewBag.Categories = categoriesLst;
            //ViewBag["Categories"] = categoriesLst;
            ProductViewModel viewModel = new ProductViewModel()
            {
                Product = id == null || id == 0 ? new Product(): _unitOfWork.ProductRepository.Get(u => u.Id == id),
                Categories = categoriesLst
            };
            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel obj, IFormFile? file)
        {
            if (obj.Product.Id==0 && _unitOfWork.ProductRepository.Get(c => c.Title == obj.Product.Title) != null)
            {
                ModelState.AddModelError("", "There is already a book with the same name");
            }

            if (ModelState.IsValid)
            {
                string wwwroot = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwroot, @"images\product");
                    string filePath = Path.Combine(productPath, fileName);

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        string oldImagePath = Path.Combine(wwwroot, obj.Product.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath)) 
                            System.IO.File.Delete(oldImagePath);
                    }

                    using(FileStream fileStream = new FileStream(filePath,FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }
                else
                {
                    if(obj.Product.Id == 0 || (obj.Product.Id!=0 && obj.Product.ImageUrl == null))
                    {
                        obj.Product.ImageUrl = string.Empty;
                    }
                }
                
                if(obj.Product.Id == 0)
                {
                    _unitOfWork.ProductRepository.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.ProductRepository.Update(obj.Product);
                }
                
                _unitOfWork.Save();
                TempData["success"] = "product created successfully";
                return RedirectToAction("Index", "product");
            }
            else
            {
                obj.Categories = _unitOfWork.CategoryRepository.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(obj);
            }
        }

        //public IActionResult Edit(int? id)
        //{

        //    if (id == null || id == 0) return NotFound();

        //    Product product = _unitOfWork.ProductRepository.Get(u => u.Id == id);

        //    if (product == null) return NotFound();

        //    return View(product);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.ProductRepository.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "product edited successfully";
        //        return RedirectToAction("Index", "product");
        //    }
        //    return View(obj);

        //}

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0) return NotFound();

        //    Product product = _unitOfWork.ProductRepository.Get(u => u.Id == id);

        //    if (product == null) return NotFound();

        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    Product product = _unitOfWork.ProductRepository.Get(u => u.Id == id);

        //    if (product == null) return NotFound();

        //    _unitOfWork.ProductRepository.Remove(product);
        //    _unitOfWork.Save();
        //    TempData["success"] = "product deleted successfully";
        //    return RedirectToAction("Index");

        //}

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objproductList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new {data = objproductList });
        }

        public IActionResult Delete(int? id)
        {
            Product productToBeDeleted = _unitOfWork.ProductRepository.Get(p=>p.Id == id);

            if (productToBeDeleted == null) return Json(new { success = false, message = "Error while deleting" });

            string wwwroot = _webHostEnvironment.WebRootPath;
            if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
            {
                string oldImagePath = Path.Combine(wwwroot, productToBeDeleted.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.ProductRepository.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = false, message = "Error while deleting" });
            //return RedirectToAction("Index");

        }
        #endregion
    }
}
