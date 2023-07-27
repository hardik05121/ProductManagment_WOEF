using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class SupplierController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        public SupplierController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            SupplierIndexVM supplierIndexVM = new SupplierIndexVM();
            List<Supplier> objSupplierList = new List<Supplier>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select C.*,S.[StateName] As StateName,A.[CountryName] As CountryName from Cities C INNER JOIN States S ON C.StateId = S.Id INNER JOIN Countries A ON C.CountryId = A.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Supplier supplier = new Supplier();
                    supplier.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    supplier.SupplierName = Convert.ToString(dtbl.Rows[i]["SupplierName"]);
                    supplier.ContactPerson = Convert.ToString(dtbl.Rows[i]["ContactPerson"]);
                    supplier.WebSite = Convert.ToString(dtbl.Rows[i]["WebSite"]);
                    supplier.Email = Convert.ToString(dtbl.Rows[i]["Email"]);
                    supplier.Address = Convert.ToString(dtbl.Rows[i]["Address"]);
                    supplier.BillingAddress = Convert.ToString(dtbl.Rows[i]["BillingAddress"]);
                    supplier.ShippingAddress = Convert.ToString(dtbl.Rows[i]["ShippingAddress"]);
                    supplier.Description = Convert.ToString(dtbl.Rows[i]["Description"]);
                    supplier.SupplierImage = Convert.ToString(dtbl.Rows[i]["SupplierImage"]);
                    supplier.MobileNumber = Convert.ToInt64(dtbl.Rows[i]["MobileNumber"]);
                    supplier.PhoneNumber = Convert.ToInt64(dtbl.Rows[i]["PhoneNumber"]);
                    supplier.CountryId = Convert.ToInt32(dtbl.Rows[i]["CountryId"]);
                    supplier.BillingCountryId = Convert.ToInt32(dtbl.Rows[i]["BillingCountryId"]);
                    supplier.ShippingCountryId = Convert.ToInt32(dtbl.Rows[i]["ShippingCountryId"]);
                    supplier.StateId = Convert.ToInt32(dtbl.Rows[i]["StateId"]);
                    supplier.BillingStateId = Convert.ToInt32(dtbl.Rows[i]["BillingStateId"]);
                    supplier.ShippingStateId = Convert.ToInt32(dtbl.Rows[i]["ShippingStateId"]);
                    supplier.CityId = Convert.ToInt32(dtbl.Rows[i]["CityId"]);
                    supplier.BillingCityId = Convert.ToInt32(dtbl.Rows[i]["BillingCityId"]);
                    supplier.ShippingCityId = Convert.ToInt32(dtbl.Rows[i]["ShippingCityId"]);

                    supplier.Country = new Country
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(dtbl.Rows[i]["CountryName"])
                    };
                    supplier.State = new State
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["StateId"]),
                        StateName = Convert.ToString(dtbl.Rows[i]["StateName"])
                    };
                    supplier.City = new City
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CityId"]),
                        CityName = Convert.ToString(dtbl.Rows[i]["CityName"])
                    };
                    objSupplierList.Add(supplier);
                }
            }
            supplierIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "supplierName_desc" : "";
            var suppliers = (from data in objSupplierList
                             where term == "" ||
                                data.SupplierName.ToLower().
                                Contains(term) || data.Country.CountryName.ToLower().Contains(term) || data.State.StateName.ToLower().Contains(term) || data.City.CityName.ToLower().Contains(term)

                             select new Supplier
                             {
                                 Id = data.Id,
                                 SupplierName = data.SupplierName,
                                 Email = data.Email,
                                 MobileNumber = data.MobileNumber,
                                 Country = data.Country,
                                 State = data.State,
                                 City = data.City,
                                 WebSite = data.WebSite
                             });
            switch (orderBy)
            {
                case "supplierName_desc":
                    suppliers = suppliers.OrderByDescending(a => a.SupplierName);
                    break;

                default:
                    suppliers = suppliers.OrderBy(a => a.SupplierName);
                    break;
            }
            int totalRecords = suppliers.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            suppliers = suppliers.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            supplierIndexVM.Suppliers = suppliers;
            supplierIndexVM.CurrentPage = currentPage;
            supplierIndexVM.TotalPages = totalPages;
            supplierIndexVM.Term = term;
            supplierIndexVM.PageSize = pageSize;
            supplierIndexVM.OrderBy = orderBy;
            return View(supplierIndexVM);
        }

        [NonAction]
        public Supplier FetchSupplierById(int? id)
        {
            Supplier supplier = new Supplier();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("SupplierById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    supplier.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    supplier.SupplierName = dtbl.Rows[0]["SupplierName"].ToString();
                    supplier.ContactPerson = dtbl.Rows[0]["ContactPerson"].ToString();
                    supplier.Email = dtbl.Rows[0]["Email"].ToString();
                    supplier.WebSite = dtbl.Rows[0]["WebSite"].ToString();
                    supplier.Address = dtbl.Rows[0]["Address"].ToString();
                    supplier.BillingAddress = dtbl.Rows[0]["BillingAddress"].ToString();
                    supplier.ShippingAddress = dtbl.Rows[0]["ShippingAddress"].ToString();
                    supplier.Description = dtbl.Rows[0]["Description"].ToString();
                    supplier.SupplierImage = dtbl.Rows[0]["SupplierImage"].ToString();
                    supplier.MobileNumber = Convert.ToInt64(dtbl.Rows[0]["MobileNumber"]);
                    supplier.PhoneNumber = Convert.ToInt64(dtbl.Rows[0]["PhoneNumber"]);
                    supplier.CountryId = Convert.ToInt32(dtbl.Rows[0]["CountryId"]);
                    supplier.StateId = Convert.ToInt32(dtbl.Rows[0]["StateId"]);
                    supplier.CityId = Convert.ToInt32(dtbl.Rows[0]["CityId"]);
                    supplier.BillingCountryId = Convert.ToInt32(dtbl.Rows[0]["BillingCountryId"]);
                    supplier.BillingStateId = Convert.ToInt32(dtbl.Rows[0]["BillingStateId"]);
                    supplier.BillingCityId = Convert.ToInt32(dtbl.Rows[0]["BillingCityId"]);
                    supplier.ShippingCountryId = Convert.ToInt32(dtbl.Rows[0]["ShippingCountryId"]);
                    supplier.ShippingStateId = Convert.ToInt32(dtbl.Rows[0]["ShippingStateId"]);
                    supplier.ShippingCityId = Convert.ToInt32(dtbl.Rows[0]["ShippingCityId"]);
                }
                return supplier;
            }
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            List<Country> objCountryList = new List<Country>();
            DataTable Cdtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select * from Countries";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(Cdtbl);

                for (int i = 0; i < Cdtbl.Rows.Count; i++)
                {
                    Country country = new Country();
                    country.Id = Convert.ToInt32(Cdtbl.Rows[i]["Id"]);
                    country.CountryName = Convert.ToString(Cdtbl.Rows[i]["CountryName"]);
                    country.IsActive = Convert.ToBoolean(Cdtbl.Rows[i]["IsActive"]);
                    objCountryList.Add(country);
                }
            }

            List<State> objStateList = new List<State>();
            DataTable Sdtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select S.*,C.[CountryName] As CountryName from States S INNER JOIN Countries C ON S.CountryId = C.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(Sdtbl);

                for (int i = 0; i < Sdtbl.Rows.Count; i++)
                {
                    State state = new State();
                    state.Id = Convert.ToInt32(Sdtbl.Rows[i]["Id"]);
                    state.StateName = Convert.ToString(Sdtbl.Rows[i]["StateName"]);
                    state.CountryId = Convert.ToInt32(Sdtbl.Rows[i]["CountryId"]);
                    state.IsActive = Convert.ToBoolean(Sdtbl.Rows[i]["IsActive"]);
                    state.Country = new Country
                    {
                        Id = Convert.ToInt32(Sdtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(Sdtbl.Rows[i]["CountryName"])
                    };
                    objStateList.Add(state);
                }
            }

            List<City> objCityList = new List<City>();
            DataTable dtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select C.*,S.[StateName] As StateName,A.[CountryName] As CountryName from Cities C INNER JOIN States S ON C.StateId = S.Id INNER JOIN Countries A ON C.CountryId = A.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    City city = new City();
                    city.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    city.CityName = Convert.ToString(dtbl.Rows[i]["CityName"]);
                    city.CountryId = Convert.ToInt32(dtbl.Rows[i]["CountryId"]);
                    city.StateId = Convert.ToInt32(dtbl.Rows[i]["StateId"]);
                    city.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);

                    city.Country = new Country
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(dtbl.Rows[i]["CountryName"])
                    };
                    city.State = new State
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["StateId"]),
                        StateName = Convert.ToString(dtbl.Rows[i]["StateName"])
                    };

                    objCityList.Add(city);
                }
            }

            SupplierVM supplierVM = new()
            {
                CityList = objCityList.Select(u => new SelectListItem
                {
                    Text = u.CityName,
                    Value = u.Id.ToString()
                }),
                StateList = objStateList.Select(u => new SelectListItem
                {
                    Text = u.StateName,
                    Value = u.Id.ToString()
                }),
                CountryList = objCountryList.Select(u => new SelectListItem
                {
                    Text = u.CountryName,
                    Value = u.Id.ToString()
                }),
                Supplier = new Supplier()
            };

            if (id == null || id == 0)
            {
                //create
                return View(supplierVM);
            }
            else
            {
                //update
                supplierVM.Supplier = FetchSupplierById(id);
                return View(supplierVM);
            }
        }
        [HttpPost]
        public IActionResult Upsert(SupplierVM supplierVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string supplierPath = Path.Combine(wwwRootPath, @"images\supplier");

                    if (!string.IsNullOrEmpty(supplierVM.Supplier.SupplierImage))
                    {
                        //delete the old image
                        var oldImagePath =
                            Path.Combine(wwwRootPath, supplierVM.Supplier.SupplierImage.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(supplierPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    supplierVM.Supplier.SupplierImage = @"\images\supplier\" + fileName;
                }
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("SupplierUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", supplierVM.Supplier.Id);
                    sqlCmd.Parameters.AddWithValue("SupplierName", supplierVM.Supplier.SupplierName);
                    sqlCmd.Parameters.AddWithValue("ContactPerson", supplierVM.Supplier.ContactPerson);
                    sqlCmd.Parameters.AddWithValue("Email", supplierVM.Supplier.Email);
                    sqlCmd.Parameters.AddWithValue("WebSite", supplierVM.Supplier.WebSite);
                    sqlCmd.Parameters.AddWithValue("Address", supplierVM.Supplier.Address);
                    sqlCmd.Parameters.AddWithValue("MobileNumber", supplierVM.Supplier.MobileNumber);
                    sqlCmd.Parameters.AddWithValue("PhoneNumber", supplierVM.Supplier.PhoneNumber);
                    sqlCmd.Parameters.AddWithValue("CountryId", supplierVM.Supplier.CountryId);
                    sqlCmd.Parameters.AddWithValue("StateId", supplierVM.Supplier.StateId);
                    sqlCmd.Parameters.AddWithValue("CityId", supplierVM.Supplier.CityId);
                    sqlCmd.Parameters.AddWithValue("BillingAddress", supplierVM.Supplier.BillingAddress);
                    sqlCmd.Parameters.AddWithValue("BillingCountryId", supplierVM.Supplier.BillingCountryId);
                    sqlCmd.Parameters.AddWithValue("BillingStateId", supplierVM.Supplier.BillingStateId);
                    sqlCmd.Parameters.AddWithValue("BillingCityId", supplierVM.Supplier.BillingCityId);
                    sqlCmd.Parameters.AddWithValue("ShippingAddress", supplierVM.Supplier.ShippingAddress);
                    sqlCmd.Parameters.AddWithValue("ShippingCountryId", supplierVM.Supplier.ShippingCountryId);
                    sqlCmd.Parameters.AddWithValue("ShippingStateId", supplierVM.Supplier.ShippingStateId);
                    sqlCmd.Parameters.AddWithValue("ShippingCityId", supplierVM.Supplier.ShippingCityId);
                    sqlCmd.Parameters.AddWithValue("Description", supplierVM.Supplier.Description);
                    sqlCmd.Parameters.AddWithValue("SupplierImage", supplierVM.Supplier.SupplierImage);

                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "Supplier Name Already Exist!";
                            return View(supplierVM);
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
                return View(supplierVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var supplierToBeDeleted = FetchSupplierById(id);
            if (supplierToBeDeleted == null)
            {
                TempData["error"] = "Supplier can't be Delete.";
                return RedirectToAction("Index");
            }

            var oldImagePath =
                      Path.Combine(_webHostEnvironment.WebRootPath,
                       supplierToBeDeleted.SupplierImage.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("SupplierDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Supplier Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    
        //#region Csacadion Droup down State,country, City
        //[HttpGet]
        //public IActionResult GetStatesByCountry(int countryId)
        //{
        //    var states = _unitOfWork.State.GetAll(s => s.CountryId == countryId);
        //    return Json(states);
        //}

        //[HttpGet]
        //public IActionResult GetCitiesByState(int stateId)
        //{
        //    var cities = _unitOfWork.City.GetAll(c => c.StateId == stateId);
        //    return Json(cities);
        //}

    }
}
