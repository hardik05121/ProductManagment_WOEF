using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System;
using System.Data;
using System.Drawing.Drawing2D;
using System.Net;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class CompanyController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        public CompanyController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            CompanyIndexVM companyIndexVM = new CompanyIndexVM();
            List<Company> objCompanyList = new List<Company>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select * from Companies";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Company company = new Company();
                    company.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    company.Title = Convert.ToString(dtbl.Rows[i]["Title"]);
                    company.Currency = Convert.ToString(dtbl.Rows[i]["Currency"]);
                    company.Address = Convert.ToString(dtbl.Rows[i]["Address"]);
                    company.PhoneNumber = Convert.ToInt64(dtbl.Rows[i]["PhoneNumber"]);
                    company.Email = Convert.ToString(dtbl.Rows[i]["Email"]);
                    company.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);
                    company.CompanyImage = Convert.ToString(dtbl.Rows[i]["CompanyImage"]);
                    objCompanyList.Add(company);
                }


            }
            companyIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "Name_desc" : "";
            var companies = (from data in objCompanyList
                             where term == "" ||
                                data.Title.ToLower().
                                Contains(term)


                             select new Company
                             {
                                 Id = data.Id,
                                 Title = data.Title,
                                 Currency = data.Currency,
                                 Address = data.Address,
                                 PhoneNumber = data.PhoneNumber,
                                 Email = data.Email,
                                 IsActive = data.IsActive,
                                 CompanyImage = data.CompanyImage
                             });
            switch (orderBy)
            {
                case "Name_desc":
                    companies = companies.OrderByDescending(a => a.Title);
                    break;

                default:
                    companies = companies.OrderBy(a => a.Title);
                    break;
            }
            int totalRecords = companies.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            companies = companies.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            companyIndexVM.Companies = companies;
            companyIndexVM.CurrentPage = currentPage;
            companyIndexVM.TotalPages = totalPages;
            companyIndexVM.Term = term;
            companyIndexVM.PageSize = pageSize;
            companyIndexVM.OrderBy = orderBy;
            return View(companyIndexVM);
        }
        #endregion


        [NonAction]
        public Company FetchCompanyById(int? id)
        {
            Company company = new Company();
            DataTable dtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("CompanyById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);
                if (dtbl.Rows.Count == 1)
                {
                    company.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    company.Title = dtbl.Rows[0]["Title"].ToString();
                    company.Currency = dtbl.Rows[0]["Currency"].ToString();
                    company.Address = dtbl.Rows[0]["Address"].ToString();
                    company.PhoneNumber = Convert.ToInt64(dtbl.Rows[0]["PhoneNumber"].ToString());
                    company.Email = dtbl.Rows[0]["Email"].ToString();
                    company.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"].ToString());
                    company.CompanyImage = dtbl.Rows[0]["CompanyImage"].ToString();
                }
                return company;
            }
        }

        #region Upsert
        [HttpGet] // to grt the data on display.
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Company());
            }
            else
            {
              Company company = FetchCompanyById(id);
               return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string companyPath = Path.Combine(wwwRootPath, @"images\company");

                    if (!string.IsNullOrEmpty(company.CompanyImage))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(wwwRootPath, company.CompanyImage.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(companyPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    company.CompanyImage = @"\images\company\" + fileName;
                }

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("CompanyUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", company.Id);
                    sqlCmd.Parameters.AddWithValue("Title", company.Title);
                    sqlCmd.Parameters.AddWithValue("Currency", company.Currency);
                    sqlCmd.Parameters.AddWithValue("Address", company.Address);
                    sqlCmd.Parameters.AddWithValue("PhoneNumber", company.PhoneNumber);
                    sqlCmd.Parameters.AddWithValue("Email", company.Email);
                    sqlCmd.Parameters.AddWithValue("IsActive", company.IsActive);
                    sqlCmd.Parameters.AddWithValue("companyImage", company.CompanyImage);

                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "Brand Name Already Exist!";
                            return View(company);
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
                return View(company);
            }
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var companyToBeDeleted = FetchCompanyById(id);
            if (companyToBeDeleted == null)
            {
                TempData["error"] = "Company can't be Delete.";
                return RedirectToAction("Index");
            }

            var oldImagePath =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           companyToBeDeleted.CompanyImage.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("CompanyDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Company Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
