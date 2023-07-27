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
    public class InquirySourceController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public InquirySourceController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            InquirySourceIndexVM inquirySourceIndexVM = new InquirySourceIndexVM();
            List<InquirySource> objInquirySourceList = new List<InquirySource>();

            DataTable dtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select * from InquirySources";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    InquirySource inquirySource = new InquirySource();
                    inquirySource.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    inquirySource.InquirySourceName = Convert.ToString(dtbl.Rows[i]["InquirySourceName"]);
                    inquirySource.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    objInquirySourceList.Add(inquirySource);
                }
            }
            inquirySourceIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "inquirySourceName_desc" : "";
            var inquirySources = (from data in objInquirySourceList
                                  where term == "" ||
                                     data.InquirySourceName.ToLower().
                                     Contains(term)


                                  select new InquirySource
                                  {
                                      Id = data.Id,
                                      InquirySourceName = data.InquirySourceName,
                                      IsActive = data.IsActive,

                                  });
            switch (orderBy)
            {
                case "inquirySourceName_desc":
                    inquirySources = inquirySources.OrderByDescending(a => a.InquirySourceName);
                    break;

                default:
                    inquirySources = inquirySources.OrderBy(a => a.InquirySourceName);
                    break;
            }
            int totalRecords = inquirySources.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            inquirySources = inquirySources.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            inquirySourceIndexVM.InquirySources = inquirySources;
            inquirySourceIndexVM.CurrentPage = currentPage;
            inquirySourceIndexVM.TotalPages = totalPages;
            inquirySourceIndexVM.Term = term;
            inquirySourceIndexVM.PageSize = pageSize;
            inquirySourceIndexVM.OrderBy = orderBy;

            return View(inquirySourceIndexVM);
        }



        #endregion

        [NonAction]
        public InquirySource FetchInquirySourceById(int? id)
        {
            InquirySource inquirySource = new InquirySource();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                DataTable dtbl = new DataTable();
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("InquirySourceById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);
                if (dtbl.Rows.Count == 1)
                {
                    inquirySource.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    inquirySource.InquirySourceName = dtbl.Rows[0]["InquirySourceName"].ToString();
                    inquirySource.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return inquirySource;
            }
        }

        #region Upsert
        [HttpGet] // to grt the data on display.
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View(new InquirySource());
            }
            else
            {
                //update
                InquirySource inquirySource = FetchInquirySourceById(id);
                return View(inquirySource);
            }
        }
        #endregion
   
        [HttpPost]
        public IActionResult Upsert(InquirySource inquirySource)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("InquirySourceUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", inquirySource.Id);
                    sqlCmd.Parameters.AddWithValue("InquirySourceName", inquirySource.InquirySourceName);
                    sqlCmd.Parameters.AddWithValue("IsActive", inquirySource.IsActive);
                   
                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "inquirySource Name Already Exist!";
                            return View(inquirySource);
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
                return View(inquirySource);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var inquirySourceToBeDeleted = FetchInquirySourceById(id);
            if (inquirySourceToBeDeleted == null)
            {
                TempData["error"] = "Brand can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("InquirySourceDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "inquirySource Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
