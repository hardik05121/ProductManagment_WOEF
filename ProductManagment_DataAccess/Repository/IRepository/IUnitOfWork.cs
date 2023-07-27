using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagment_DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        IUserRepository User { get; }
        IInventoryRepository Inventory { get; }
        IQuotationRepository Quotation { get; }
        IQuotationXproductRepository QuotationXproduct { get; }
        void Save();
    }
}
