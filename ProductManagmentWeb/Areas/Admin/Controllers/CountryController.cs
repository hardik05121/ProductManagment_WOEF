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
    public class CountryController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public CountryController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {

            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            CountryIndexVM countryIndexVM = new CountryIndexVM();
            List<Country> objCountryList = new List<Country>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select * from Countries";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Country country = new Country();
                    country.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    country.CountryName = Convert.ToString(dtbl.Rows[i]["CountryName"]);
                    country.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    objCountryList.Add(country);
                }
            }
            countryIndexVM.CountryNameSortOrder = string.IsNullOrEmpty(orderBy) ? "countryName_desc" : "";
            var countries = (from data in objCountryList
                             where term == "" ||
                             data.CountryName.ToLower().
                             Contains(term)


                          select new Country
                          {
                              Id = data.Id,
                              CountryName = data.CountryName,
                              IsActive = data.IsActive,
                          });

            switch (orderBy)
            {
                case "stateName_desc":
                    countries = countries.OrderByDescending(a => a.CountryName);
                    break;

                default:
                    countries = countries.OrderBy(a => a.CountryName);
                    break;
            }
            int totalRecords = countries.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            countries = countries.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            countryIndexVM.Countries = countries;
            countryIndexVM.CurrentPage = currentPage;
            countryIndexVM.Term = term;
            countryIndexVM.PageSize = pageSize;
            countryIndexVM.OrderBy = orderBy;
            return View(countryIndexVM);
        }
        #endregion

        [NonAction]
        public Country FetchCountryById(int? id)
        {
            Country country = new Country();
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
                    country.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    country.CountryName = dtbl.Rows[0]["CountryName"].ToString();
                    country.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return country;
            }
        }
        #region Upsert
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new Country());
            }
            else
            {
                //update
                Country country = FetchCountryById(id);
                return View(country);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Country country)
        {
            if (ModelState.IsValid)
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("CountryUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", country.Id);
                    sqlCmd.Parameters.AddWithValue("CountryName", country.CountryName);
                    sqlCmd.Parameters.AddWithValue("IsActive", country.IsActive);

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
                            return View(country);
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
                return View(country);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var countryToBeDeleted = FetchCountryById(id);
            if (countryToBeDeleted == null)
            {
                TempData["error"] = "country can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("CountryDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Country Deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        #endregion
    }
}
