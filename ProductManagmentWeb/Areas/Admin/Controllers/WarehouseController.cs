using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;

namespace ProductManagment.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class WarehouseController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public WarehouseController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {

            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            WarehouseIndexVM warehouseIndexVM = new WarehouseIndexVM();
            List<Warehouse> objWarehouseList = new List<Warehouse>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select * from Warehouses";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Warehouse warehouse = new Warehouse();
                    warehouse.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    warehouse.WarehouseName = Convert.ToString(dtbl.Rows[i]["WarehouseName"]);
                    warehouse.MobileNumber = Convert.ToInt64(dtbl.Rows[i]["MobileNumber"]);
                    warehouse.ContactPerson = Convert.ToInt64(dtbl.Rows[i]["ContactPerson"]);
                    warehouse.Email = Convert.ToString(dtbl.Rows[i]["Email"]);
                    warehouse.Address = Convert.ToString(dtbl.Rows[i]["Address"]);
                    warehouse.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    objWarehouseList.Add(warehouse);
                }
            }

            warehouseIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "warehouseName_desc" : "";
            var warehouses = (from data in objWarehouseList
                          where term == "" ||
                             data.WarehouseName.ToLower().
                             Contains(term)

                          select new Warehouse
                          {
                              Id = data.Id,
                              WarehouseName = data.WarehouseName,
                              ContactPerson = data.ContactPerson,
                              MobileNumber =  data.MobileNumber,
                              Email = data.Email,
                              Address = data.Address,
                              IsActive = data.IsActive
                          });

            switch (orderBy)
            {
                case "warehouseName_desc":
                    warehouses = warehouses.OrderByDescending(a => a.WarehouseName);
                    break;

                default:
                    warehouses = warehouses.OrderBy(a => a.WarehouseName);
                    break;
            }
            int totalRecords = warehouses.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            warehouses = warehouses.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            warehouseIndexVM.Warehouses = warehouses;
            warehouseIndexVM.CurrentPage = currentPage;
            warehouseIndexVM.TotalPages = totalPages;
            warehouseIndexVM.Term = term;
            warehouseIndexVM.PageSize = pageSize;
            warehouseIndexVM.OrderBy = orderBy;
            return View(warehouseIndexVM);
        }


        [NonAction]
        public Warehouse FetchWarehouseById(int? id)
        {
            Warehouse warehouse = new Warehouse();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("WarehouseById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    warehouse.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    warehouse.WarehouseName= dtbl.Rows[0]["WarehouseName"].ToString();
                    warehouse.ContactPerson = Convert.ToInt64(dtbl.Rows[0]["ContactPerson"]);
                    warehouse.MobileNumber = Convert.ToInt64(dtbl.Rows[0]["MobileNumber"]);
                    warehouse.Email = dtbl.Rows[0]["Email"].ToString();
                    warehouse.Address = dtbl.Rows[0]["Address"].ToString();
                    warehouse.IsActive= Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return warehouse;
            }
        }

        #region Upsert
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new Warehouse());
            }
            else
            {
                //update
                Warehouse warehouse = FetchWarehouseById(id);
                return View(warehouse);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Warehouse warehouse)
        {

            if (ModelState.IsValid)
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("WarehouseUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", warehouse.Id);
                    sqlCmd.Parameters.AddWithValue("WarehouseName", warehouse.WarehouseName);
                    sqlCmd.Parameters.AddWithValue("ContactPerson", warehouse.ContactPerson);
                    sqlCmd.Parameters.AddWithValue("MobileNumber", warehouse.MobileNumber);
                    sqlCmd.Parameters.AddWithValue("Email", warehouse.Email);
                    sqlCmd.Parameters.AddWithValue("Address", warehouse.Address);
                    sqlCmd.Parameters.AddWithValue("IsActive", warehouse.IsActive);

                    //  sqlCmd.ExecuteNonQuery();

                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "Warehouse Name Already Exist!";
                            return View(warehouse);
                        }
                        else
                        {
                            throw; // Rethrow other unexpected SQL exceptions
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(warehouse);
            }
        }
        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var warehouseToBeDeleted = FetchWarehouseById(id);
            if (warehouseToBeDeleted == null)
            {
                TempData["error"] = "Warehouse can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("WarehouseDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Warehouse Deleted successfully";
            return RedirectToAction(nameof(Index));
        }

    }
}
