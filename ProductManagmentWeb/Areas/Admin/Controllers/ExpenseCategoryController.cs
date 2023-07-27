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
   // [Authorize]
    public class ExpenseCategoryController : Controller
    {


        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public ExpenseCategoryController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            ExpenseCategoryIndexVM expenseCategoryIndexVM = new ExpenseCategoryIndexVM();
            List<ExpenseCategory> objExpenseCategoryList = new List<ExpenseCategory>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select * from ExpenseCategories";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    ExpenseCategory expenseCategory = new ExpenseCategory();
                    expenseCategory.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    expenseCategory.ExpenseCategoryName = Convert.ToString(dtbl.Rows[i]["ExpenseCategoryName"]);
                    expenseCategory.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    objExpenseCategoryList.Add(expenseCategory);
                }


            }
            expenseCategoryIndexVM.ExpenseCategoryNameSortOrder = string.IsNullOrEmpty(orderBy) ? "expenseCategoryName_desc" : "";
            var expenseCategories = (from data in objExpenseCategoryList
                                     where term == "" ||
                             data.ExpenseCategoryName.ToLower().
                             Contains(term)

                          select new ExpenseCategory
                          {
                              Id = data.Id,
                              ExpenseCategoryName = data.ExpenseCategoryName,
                              IsActive = data.IsActive,
                          });

            switch (orderBy)
            {
                case "stateName_desc":
                    expenseCategories = expenseCategories.OrderByDescending(a => a.ExpenseCategoryName);
                    break;

                default:
                    expenseCategories = expenseCategories.OrderBy(a => a.ExpenseCategoryName);
                    break;
            }
            int totalRecords = expenseCategories.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            expenseCategories = expenseCategories.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            expenseCategoryIndexVM.ExpenseCategories = expenseCategories;
            expenseCategoryIndexVM.CurrentPage = currentPage;
            expenseCategoryIndexVM.TotalPages = totalPages;
            expenseCategoryIndexVM.Term = term;
            expenseCategoryIndexVM.PageSize = pageSize;
            expenseCategoryIndexVM.OrderBy = orderBy;
            return View(expenseCategoryIndexVM);
        }
        #endregion


        [NonAction]
        public ExpenseCategory FetchExpenseCategoryById(int? id)
        {
            ExpenseCategory expenseCategory = new ExpenseCategory();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {

                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("BrandById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    expenseCategory.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    expenseCategory.ExpenseCategoryName = dtbl.Rows[0]["ExpenseCategoryName"].ToString();
                    expenseCategory.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return expenseCategory;
            }
        }
        #region Upsert
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new ExpenseCategory());
            }
            else
            {
                //update
                ExpenseCategory expenseCategory = FetchExpenseCategoryById(id);
                return View(expenseCategory);
            }

        }

        [HttpPost]
        public IActionResult Upsert(ExpenseCategory expenseCategory)
        {
            if (ModelState.IsValid)
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("ExpenseCategoryUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", expenseCategory.Id);
                    sqlCmd.Parameters.AddWithValue("ExpenseCategoryName", expenseCategory.ExpenseCategoryName);
                    sqlCmd.Parameters.AddWithValue("IsActive", expenseCategory.IsActive);

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
                            TempData["error"] = "Country Name Already Exist!";
                            return View(expenseCategory);
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
                return View(expenseCategory);
            }
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var countryToBeDeleted = FetchExpenseCategoryById(id);
            if (countryToBeDeleted == null)
            {
                TempData["error"] = "country can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("ExpenseCategoryDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Country Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
