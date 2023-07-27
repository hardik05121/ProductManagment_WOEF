using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    //[Authorize]
    public class CategoryController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        public CategoryController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            CategoryIndexVM categoryIndexVM = new CategoryIndexVM();
            List<Category> objCategoryList = new List<Category>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                // below both line are used for the add string in vs there add and add type text 
                // otherwise use storeprosudure.

                string str = "select * from Categories";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Category category = new Category();
                    category.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    category.Name = Convert.ToString(dtbl.Rows[i]["Name"]);
                    category.Description = Convert.ToString(dtbl.Rows[i]["Description"]);
                    category.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    objCategoryList.Add(category);
                }


            }
            categoryIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "Name_desc" : "";
            var categories = (from data in objCategoryList
                              where term == "" ||
                                 data.Name.ToLower().
                                 Contains(term)

                              select new Category
                              {
                                  Id = data.Id,
                                  Name = data.Name,
                                  Description = data.Description,
                                  IsActive = data.IsActive

                              });
            switch (orderBy)
            {
                case "Name_desc":
                    categories = categories.OrderByDescending(a => a.Name);
                    break;

                default:
                    categories = categories.OrderBy(a => a.Name);
                    break;
            }
            int totalRecords = categories.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            categories = categories.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            categoryIndexVM.Categories = categories;
            categoryIndexVM.CurrentPage = currentPage;
            categoryIndexVM.TotalPages = totalPages;
            categoryIndexVM.Term = term;
            categoryIndexVM.PageSize = pageSize;
            categoryIndexVM.OrderBy = orderBy;
            return View(categoryIndexVM);
        }


        [NonAction]
        public Category FetchCategoryById(int? id)
        {
            Category category = new Category();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {

                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("CategoryById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    category.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    category.Name = dtbl.Rows[0]["Name"].ToString();
                    category.Description = dtbl.Rows[0]["Description"].ToString();
                    category.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return category;
            }
        }
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new Category());
            }
            else
            {
                //update
                Category category = FetchCategoryById(id);
                return View(category);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Category category)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("CategoryUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", category.Id);
                    sqlCmd.Parameters.AddWithValue("Name", category.Name);
                    sqlCmd.Parameters.AddWithValue("Description", category.Description);
                    sqlCmd.Parameters.AddWithValue("IsActive", category.IsActive);

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
                            return View(category);
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
                return View(category);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var categoryToBeDeleted = FetchCategoryById(id);
            if (categoryToBeDeleted == null)
            {
                TempData["error"] = "Brand can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("CategoryDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction(nameof(Index));
        }

    }
}
