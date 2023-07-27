using ProductManagment_DataAccess.Data;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagment_DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public IUserRepository User { get; private set; }
        public IInventoryRepository Inventory { get; private set; }
        public IQuotationRepository Quotation { get; private set; }
        public IQuotationXproductRepository QuotationXproduct { get; private set; }

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            User = new UserRepository(_db);
            Inventory = new InventoryRepository(_db);
            Quotation = new QuotationRepository(_db);
            QuotationXproduct = new QuotationXproductRepository(_db);

        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
