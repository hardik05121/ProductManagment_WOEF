using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;

using ProductManagment_Models.ViewModels;

using System.Data;

//using ProductManagment_Models.ModelsMetadata;


namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BrandController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public BrandController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {

            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        //#region Index
        //public IActionResult Index()
        //{
        //    List<Brand> objBrandList = new List<Brand>();


        //    DataTable dtbl = new DataTable();
        //    using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        //    {
        //        sqlConnection.Open();
        //        string str = "select * from Brands";
        //        SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
        //        sqlDa.SelectCommand.CommandType = CommandType.Text;
        //        sqlDa.Fill(dtbl);

        //        for (int i = 0; i < dtbl.Rows.Count; i++)
        //        {
        //            Brand brand = new Brand();
        //            brand.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
        //            brand.BrandName = Convert.ToString(dtbl.Rows[i]["BrandName"]);
        //            brand.BrandImage = Convert.ToString(dtbl.Rows[i]["BrandImage"]);
        //            objBrandList.Add(brand);
        //        }
        //    }
        //    return View(objBrandList);
        //}
        // #endregion

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            BrandIndexVM brandIndexVM = new BrandIndexVM();
            List<Brand> objBrandList = new List<Brand>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                // below both line are used for the add string in vs there add and add type text 
                // otherwise use storeprosudure.

                string str = "select * from Brands";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Brand brand = new Brand();
                    brand.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    brand.BrandName = Convert.ToString(dtbl.Rows[i]["BrandName"]);
                    brand.BrandImage = Convert.ToString(dtbl.Rows[i]["BrandImage"]);
                    objBrandList.Add(brand);
                }


            }
            brandIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "brandName_desc" : "";
            var brands = (from data in objBrandList
                          where term == "" ||
                             data.BrandName.ToLower().
                             Contains(term)


                          select new Brand
                          {
                              Id = data.Id,
                              BrandName = data.BrandName,
                              BrandImage = data.BrandImage
                          });

            switch (orderBy)
            {
                case "brandName_desc":
                    brands = brands.OrderByDescending(a => a.BrandName);
                    break;

                default:
                    brands = brands.OrderBy(a => a.BrandName);
                    break;
            }

            int totalRecords = brands.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            brands = brands.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            brandIndexVM.Brands = brands;
            brandIndexVM.CurrentPage = currentPage;
            brandIndexVM.TotalPages = totalPages;
            brandIndexVM.Term = term;
            brandIndexVM.PageSize = pageSize;
            brandIndexVM.OrderBy = orderBy;

            return View(brandIndexVM);
        }
        #endregion


        #region Upsert
        [HttpGet] // to grt the data on display.
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                //create
                return View(new Brand());
            }
            else
            {
                //update
                Brand brand = FetchBrandByID(id);
                return View(brand);
            }
        }

        [NonAction]
        public Brand FetchBrandByID(int? id)
        {
            Brand brand = new Brand();
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
                    brand.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    brand.BrandName = dtbl.Rows[0]["BrandName"].ToString();
                    brand.BrandImage = dtbl.Rows[0]["BrandImage"].ToString();
                }
                return brand;
            }
        }


        [HttpPost]
        public IActionResult Upsert(Brand brand, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string brandPath = Path.Combine(wwwRootPath, @"images\brand");

                    if (!string.IsNullOrEmpty(brand.BrandImage))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(wwwRootPath, brand.BrandImage.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(brandPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    brand.BrandImage = @"\images\brand\" + fileName;
                }


                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("Upsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", brand.Id);
                    sqlCmd.Parameters.AddWithValue("BrandName", brand.BrandName);
                    sqlCmd.Parameters.AddWithValue("BrandImage", brand.BrandImage);

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
                            TempData["error"] = "Brand Name Already Exist!";
                            return View(brand);
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

                return View(brand);
            }
        }
        #endregion


        // POST: Book/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var brandToBeDeleted = FetchBrandByID(id);
            if (brandToBeDeleted == null)
            {
                TempData["error"] = "Brand can't be Delete.";
                return RedirectToAction("Index");
            }

            var oldImagePath =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           brandToBeDeleted.BrandImage.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("BrandDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Brand Deleted successfully";
            return RedirectToAction(nameof(Index));
        }

    }
}
