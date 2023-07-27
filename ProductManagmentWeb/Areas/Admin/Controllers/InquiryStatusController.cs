using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;

using System.Diagnostics.Metrics;


namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class InquiryStatusController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public InquiryStatusController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            InquiryStatusIndexVM inquiryStatusIndexVM = new InquiryStatusIndexVM();
            List<InquiryStatus> objInquiryStatusList = new List<InquiryStatus>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select * from InquiryStatuses";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    InquiryStatus inquiryStatus = new InquiryStatus();
                    inquiryStatus.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    inquiryStatus.InquiryStatusName = Convert.ToString(dtbl.Rows[i]["InquiryStatusName"]);
                    inquiryStatus.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    objInquiryStatusList.Add(inquiryStatus);
                }
            }

            inquiryStatusIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "inquiryStatusName_desc" : "";
            var inquiryStatuses = (from data in objInquiryStatusList
                                   where term == "" ||
                                      data.InquiryStatusName.ToLower().
                                      Contains(term)


                                   select new InquiryStatus
                                   {
                                       Id = data.Id,
                                       InquiryStatusName = data.InquiryStatusName,
                                       IsActive = data.IsActive,

                                   });
            switch (orderBy)
            {
                case "inquiryStatusName_desc":
                    inquiryStatuses = inquiryStatuses.OrderByDescending(a => a.InquiryStatusName);
                    break;

                default:
                    inquiryStatuses = inquiryStatuses.OrderBy(a => a.InquiryStatusName);
                    break;
            }
            int totalRecords = inquiryStatuses.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            inquiryStatuses = inquiryStatuses.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            inquiryStatusIndexVM.InquiryStatuses = inquiryStatuses;
            inquiryStatusIndexVM.CurrentPage = currentPage;
            inquiryStatusIndexVM.TotalPages = totalPages;
            inquiryStatusIndexVM.Term = term;
            inquiryStatusIndexVM.PageSize = pageSize;
            inquiryStatusIndexVM.OrderBy = orderBy;

            return View(inquiryStatusIndexVM);
        }
        #endregion

        [NonAction]
        public InquiryStatus FetchInquiryStatusById(int? id)
        {
            InquiryStatus inquiryStatus = new InquiryStatus();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("InquiryStatusById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    inquiryStatus.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    inquiryStatus.InquiryStatusName = dtbl.Rows[0]["InquiryStatusName"].ToString();
                    inquiryStatus.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return inquiryStatus;
            }
        }

        #region Upsert
        [HttpGet] // to grt the data on display.
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View(new InquiryStatus());
            }
            else
            {
                //update
                InquiryStatus inquiryStatus = FetchInquiryStatusById(id);
                return View(inquiryStatus);
            }
        }
        #endregion

        [HttpPost]
        public IActionResult Upsert(InquiryStatus inquiryStatus)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("InquiryStatusUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", inquiryStatus.Id);
                    sqlCmd.Parameters.AddWithValue("InquiryStatusName", inquiryStatus.InquiryStatusName);
                    sqlCmd.Parameters.AddWithValue("IsActive", inquiryStatus.IsActive);
                   
                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "inquiryStatus Name Already Exist!";
                            return View(inquiryStatus);
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
                return View(inquiryStatus);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var inquiryStatusToBeDeleted = FetchInquiryStatusById(id);
            if (inquiryStatusToBeDeleted == null)
            {
                TempData["error"] = "inquiryStatus can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("InquiryStatusDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "inquiryStatus Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
