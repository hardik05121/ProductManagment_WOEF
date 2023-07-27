using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;
using System.Drawing.Drawing2D;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class TaxController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public TaxController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            TaxIndexVM taxIndexVM = new TaxIndexVM();
            List<Tax> objTaxList = new List<Tax>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select * from Taxs";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Tax tax = new Tax();
                    tax.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    tax.Name = Convert.ToString(dtbl.Rows[i]["Name"]);
                    tax.Percentage = Convert.ToDouble(dtbl.Rows[i]["Percentage"]);
                    objTaxList.Add(tax);
                }
            }
            taxIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "name_desc" : "";
            var taxes = (from data in objTaxList
                         where term == "" || data.Name.ToLower().Contains(term)

                          select new Tax
                          {
                              Id = data.Id,
                              Name = data.Name,
                              Percentage = data.Percentage
                          });

            switch (orderBy)
            {
                case "name_desc":
                    taxes = taxes.OrderByDescending(a => a.Name);
                    break;

                default:
                    taxes = taxes.OrderBy(a => a.Name);
                    break;
            }

            int totalRecords = taxes.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            taxes = taxes.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            taxIndexVM.Taxes = taxes;
            taxIndexVM.CurrentPage = currentPage;
            taxIndexVM.TotalPages = totalPages;
            taxIndexVM.Term = term;
            taxIndexVM.PageSize = pageSize;
            taxIndexVM.OrderBy = orderBy;
            return View(taxIndexVM);
        }

        [NonAction]
        public Tax FetchTaxById(int? id)
        {
            Tax tax = new Tax();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("TaxById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    tax.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    tax.Name = dtbl.Rows[0]["Name"].ToString();
                    tax.Percentage = Convert.ToDouble(dtbl.Rows[0]["Percentage"]);
                }
                return tax;
            }
        }

        #region Upsert
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new Tax());
            }
            else
            {
                //update
                Tax tax = FetchTaxById(id);
                return View(tax);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Tax tax)
        {

            if (ModelState.IsValid)
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("TaxUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", tax.Id);
                    sqlCmd.Parameters.AddWithValue("Percentage", tax.Percentage);
                    sqlCmd.Parameters.AddWithValue("Name", tax.Name);

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
                            TempData["error"] = "Tax Name Already Exist!";
                            return View(tax);
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
                return View(tax);
            }
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var taxToBeDeleted = FetchTaxById(id);
            if (taxToBeDeleted == null)
            {
                TempData["error"] = "Tax vcan't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("TaxDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Tax Deleted successfully";
            return RedirectToAction(nameof(Index));
        }

    }
}
