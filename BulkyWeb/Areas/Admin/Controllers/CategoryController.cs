using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _unitOfWork.CategoryRepository.GetAll().ToList();
            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj == null || obj.Name == null)
            {
                ModelState.AddModelError("", "Category Name cannot be empty");
            }
            if (_unitOfWork.CategoryRepository.Get(c => c.Name == obj.Name) != null)
            {
                ModelState.AddModelError("", "Category Name is already in use");
            }
            if (_unitOfWork.CategoryRepository.Get(c => c.DisplayOrder == obj.DisplayOrder) != null)
            {
                ModelState.AddModelError("", "Display Order is already in use");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index", "Category");
            }
            return View(obj);

        }

        public IActionResult Edit(int? id)
        {

            if (id == null || id == 0) return NotFound();
            //Category category = _db.Categories.FirstOrDefault(c => c.Id == id);
            Category category = _unitOfWork.CategoryRepository.Get(u => u.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category edited successfully";
                return RedirectToAction("Index", "Category");
            }
            return View(obj);

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0) return NotFound();

            Category category = _unitOfWork.CategoryRepository.Get(u => u.Id == id);

            if (category == null) return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Category category = _unitOfWork.CategoryRepository.Get(u => u.Id == id);

            if (category == null) return NotFound();

            _unitOfWork.CategoryRepository.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");

        }
    }
}
