using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ProductRepository: Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
            Product productDB = _db.Products.FirstOrDefault(u=>u.Id == product.Id);
            if (productDB != null)
            {
                productDB.Title = product.Title;
                productDB.Description = product.Description;
                productDB.CategoryId = product.CategoryId;
                productDB.Price = product.Price;
                productDB.Price50 = product.Price50;
                productDB.Price100 = product.Price100;
                productDB.ListPrice = product.ListPrice;
                productDB.Author = product.Author;
                productDB.ISBN = product.ISBN;
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    productDB.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
