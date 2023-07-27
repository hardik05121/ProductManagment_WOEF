using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{

    namespace ProductManagmentWeb.Areas.Admin.Controllers
    {
        [Area("Admin")]
        // [Authorize(Roles = "Admin")]
        public class ExpenseController : Controller
        {
            private readonly IWebHostEnvironment _webHostEnvironment;
            private readonly IConfiguration _configuration;


            public ExpenseController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
            {
                _webHostEnvironment = webHostEnvironment;
                _configuration = configuration;

            }



            #region Index
            public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
            {
                ViewData["CurrentFilter"] = term;
                term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

                ExpenseIndexVM expenseIndexVM = new ExpenseIndexVM();
                List<Expense> objExpenseList = new List<Expense>();

                DataTable dtbl = new DataTable();

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();

                    string str = "SELECT E.*,C.[ExpenseCategoryName] As ExpenseCategoryName from Expenses E INNER JOIN ExpenseCategories C ON E.ExpenseCategoryId = C.Id";
                    SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                    sqlDa.SelectCommand.CommandType = CommandType.Text;
                    sqlDa.Fill(dtbl);

                    for (int i = 0; i < dtbl.Rows.Count; i++)
                    {
                        Expense expense = new Expense();
                        expense.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                        expense.Reference = Convert.ToString(dtbl.Rows[i]["Reference"]);
                        expense.Amount = Convert.ToInt32(dtbl.Rows[i]["Amount"]);
                        expense.ExpenseCategoryId = Convert.ToInt32(dtbl.Rows[i]["ExpenseCategoryId"]);
                        expense.Note = Convert.ToString(dtbl.Rows[i]["Note"]);
                        expense.ExpenseFile = Convert.ToString(dtbl.Rows[i]["ExpenseFile"]);

                        expense.ExpenseCategory = new ExpenseCategory
                        {
                            Id = Convert.ToInt32(dtbl.Rows[i]["ExpenseCategoryId"]),
                            ExpenseCategoryName = Convert.ToString(dtbl.Rows[i]["ExpenseCategoryName"])
                        };
                        objExpenseList.Add(expense);
                    }
                }

                expenseIndexVM.ExpenseNameSortOrder = string.IsNullOrEmpty(orderBy) ? "expenseName_desc" : "";
                var expenses = (from data in objExpenseList
                                where term == "" || data.ExpenseCategory.ExpenseCategoryName.ToLower().Contains(term)

                                select new Expense
                                {
                                    Id = data.Id,
                                    CreatedDate = data.CreatedDate,
                                    ExpenseDate = data.ExpenseDate,
                                    Reference = data.Reference,
                                    Amount = data.Amount,
                                    ExpenseCategory = data.ExpenseCategory,
                                    Note = data.Note,
                                    ExpenseFile = data.ExpenseFile,
                                });

                switch (orderBy)
                {
                    case "expenseName_desc":
                        expenses = expenses.OrderByDescending(a => a.ExpenseCategory.ExpenseCategoryName);
                        break;

                    default:
                        expenses = expenses.OrderBy(a => a.ExpenseCategory.ExpenseCategoryName);
                        break;
                }
                int totalRecords = expenses.Count();
                int pageSize = 5;
                int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                expenses = expenses.Skip((currentPage - 1) * pageSize).Take(pageSize);
                // current=1, skip= (1-1=0), take=5 
                // currentPage=2, skip (2-1)*5 = 5, take=5 ,
                expenseIndexVM.Expenses = expenses;
                expenseIndexVM.CurrentPage = currentPage;
                expenseIndexVM.TotalPages = totalPages;
                expenseIndexVM.Term = term;
                expenseIndexVM.PageSize = pageSize;
                expenseIndexVM.OrderBy = orderBy;
                return View(expenseIndexVM);
            }


            #endregion

            [NonAction]
            public Expense FetchExpenseById(int? id)
            {
                Expense expense = new Expense();
                DataTable dtbl = new DataTable();

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    //string str = "select S.*,C.[CountryName] As CountryName from States S INNER JOIN Countries C ON S.Id = Id";
                    // SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                    SqlDataAdapter sqlDa = new SqlDataAdapter("ExpenseById", sqlConnection);
                    sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                    sqlDa.Fill(dtbl);

                    if (dtbl.Rows.Count == 1)
                    {
                        expense.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                        expense.ExpenseCategoryId = Convert.ToInt32(dtbl.Rows[0]["ExpenseCategoryId"].ToString());
                        expense.CreatedDate = Convert.ToDateTime(dtbl.Rows[0]["CreatedDate"]);
                        expense.ExpenseDate = Convert.ToDateTime(dtbl.Rows[0]["ExpenseDate"]);
                        expense.Reference = Convert.ToString(dtbl.Rows[0]["Reference"]).ToString();
                        expense.Amount = Convert.ToInt32(dtbl.Rows[0]["Amount"]);
                        expense.Note = Convert.ToString(dtbl.Rows[0]["Note"]).ToString();
                        expense.Reference = Convert.ToString(dtbl.Rows[0]["Reference"]).ToString();
                        expense.ExpenseFile = Convert.ToString(dtbl.Rows[0]["ExpenseFile"]).ToString();
                    }
                    return expense;
                }
            }

            #region Upsert
            [HttpGet]
            public IActionResult Upsert(int? id)
            {

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
                ExpenseVM expenseVM = new()
                {
                    ExpenseCategoryList = objExpenseCategoryList.Select(u => new SelectListItem
                    {
                        Text = u.ExpenseCategoryName,
                        Value = u.Id.ToString()
                    }),
                    Expense = new Expense()
                };
                if (id == null || id == 0)
                {

                    return View(expenseVM);
                }
                else
                {
                    expenseVM.Expense = FetchExpenseById(id);
                    return View(expenseVM);
                }
            }

            [HttpPost]
            public IActionResult Upsert(ExpenseVM expenseVM, IFormFile? file)
            {
                if (ModelState.IsValid)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string expensePath = Path.Combine(wwwRootPath, @"images\expense");

                        if (!string.IsNullOrEmpty((string?)expenseVM.Expense.ExpenseFile))
                        {
                            //delete the old image
                            var oldImagePath =
                                        Path.Combine(wwwRootPath, (string)expenseVM.Expense.ExpenseFile.TrimStart('\\'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var fileStream = new FileStream(Path.Combine(expensePath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        expenseVM.Expense.ExpenseFile = @"\images\expense\" + fileName;
                    }

                    using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                    {
                        sqlConnection.Open();
                        SqlCommand sqlCmd = new SqlCommand("ExpenseUpsert", sqlConnection);
                        sqlCmd.CommandType = CommandType.StoredProcedure;
                        sqlCmd.Parameters.AddWithValue("Id", expenseVM.Expense.Id);
                        if (expenseVM.Expense.CreatedDate != null)
                        {
                            sqlCmd.Parameters.AddWithValue("CreatedDate", expenseVM.Expense.CreatedDate);
                        }
                        
                        if (expenseVM.Expense.ExpenseDate != null)
                        {
                            sqlCmd.Parameters.AddWithValue("ExpenseDate", expenseVM.Expense.ExpenseDate);
                        }
                        
                        sqlCmd.Parameters.AddWithValue("Reference", expenseVM.Expense.Reference);
                        sqlCmd.Parameters.AddWithValue("Amount", expenseVM.Expense.Amount);
                        sqlCmd.Parameters.AddWithValue("ExpenseCategoryId", expenseVM.Expense.ExpenseCategoryId);
                        sqlCmd.Parameters.AddWithValue("UserId", expenseVM.Expense.UserId);
                        sqlCmd.Parameters.AddWithValue("Note", expenseVM.Expense.Note);
                        if(expenseVM.Expense.ExpenseFile != null)
                        {
                            sqlCmd.Parameters.AddWithValue("ExpenseFile", expenseVM.Expense.ExpenseFile);
                        }
                        
                        try
                        {
                            sqlCmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                            {
                                ModelState.AddModelError("", ex.Message);
                                TempData["error"] = "Exapance Name Already Exist!";
                                return View(expenseVM);
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
                    return View(expenseVM);
                }
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public IActionResult Delete(int? id)
            {
                var ExpenseToBeDeleted = FetchExpenseById(id);



                var oldImagePath =
                               Path.Combine(_webHostEnvironment.WebRootPath,
                               ExpenseToBeDeleted.ExpenseFile.TrimStart('\\'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                if (ExpenseToBeDeleted == null)
                {
                    TempData["error"] = "Expense can't be Delete.";
                    return RedirectToAction("Index");
                }
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("ExpenseDeleteById", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", id);
                    sqlCmd.ExecuteNonQuery();
                }
                TempData["success"] = "Expense Deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            #endregion
        }
    }
}
