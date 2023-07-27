using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.ViewModels;
using ProductManagment_Models.Models;

using System.Data;
using Microsoft.AspNetCore.Authorization;

using System.Drawing.Drawing2D;
using Microsoft.Data.SqlClient;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]

    public class CustomerController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        public CustomerController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            CustomerIndexVM customerIndexVM = new CustomerIndexVM();
            List<Customer> objCustomerList = new List<Customer>();

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
                    Customer customer = new Customer();
                    customer.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    customer.CustomerName = Convert.ToString(dtbl.Rows[i]["CustomerName"]);
                    customer.ContactPerson = Convert.ToString(dtbl.Rows[i]["ContactPerson"]);
                    customer.WebSite = Convert.ToString(dtbl.Rows[i]["WebSite"]);
                    customer.Email = Convert.ToString(dtbl.Rows[i]["Email"]);
                    customer.Address = Convert.ToString(dtbl.Rows[i]["Address"]);
                    customer.Description = Convert.ToString(dtbl.Rows[i]["Description"]);
                    customer.CustomerImage = Convert.ToString(dtbl.Rows[i]["CustomerImage"]);
                    customer.MobileNumber = Convert.ToInt64(dtbl.Rows[i]["MobileNumber"]);
                    customer.PhoneNumber = Convert.ToInt64(dtbl.Rows[i]["PhoneNumber"]);
                    customer.CountryId = Convert.ToInt32(dtbl.Rows[i]["CountryId"]);
                    customer.StateId = Convert.ToInt32(dtbl.Rows[i]["StateId"]);
                    customer.CityId = Convert.ToInt32(dtbl.Rows[i]["CityId"]);
                    customer.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);

                    customer.Country = new Country
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(dtbl.Rows[i]["CountryName"])
                    };
                    customer.State = new State
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["StateId"]),
                        StateName = Convert.ToString(dtbl.Rows[i]["StateName"])
                    };
                    customer.City = new City
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CityId"]),
                        CityName = Convert.ToString(dtbl.Rows[i]["CityName"])
                    };
                    objCustomerList.Add(customer);
                }
            }
            customerIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "customerName_desc" : "";
            var customers = (from data in objCustomerList
                             where term == "" ||
                                data.CustomerName.ToLower().
                                Contains(term) || data.Country.CountryName.ToLower().Contains(term) || data.State.StateName.ToLower().Contains(term) || data.City.CityName.ToLower().Contains(term)

                             select new Customer
                             {
                                 Id = data.Id,
                                 CustomerName = data.CustomerName,
                                 ContactPerson = data.ContactPerson,
                                 Email = data.Email,
                                 MobileNumber = data.MobileNumber,
                                 WebSite = data.WebSite,
                                 CustomerImage = data.CustomerImage
                             });

            switch (orderBy)
            {
                case "customerName_desc":
                    customers = customers.OrderByDescending(a => a.CustomerName);
                    break;

                default:
                    customers = customers.OrderBy(a => a.CustomerName);
                    break;
            }
            int totalRecords = customers.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            customers = customers.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            customerIndexVM.Customers = customers;
            customerIndexVM.CurrentPage = currentPage;
            customerIndexVM.TotalPages = totalPages;
            customerIndexVM.Term = term;
            customerIndexVM.PageSize = pageSize;
            customerIndexVM.OrderBy = orderBy;
            return View(customerIndexVM);

        }

        [NonAction]
        public Customer FetchCustomerById(int? id)
        {
            Customer customer = new Customer();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("CustomerById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    customer.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    customer.CustomerName = dtbl.Rows[0]["CustomerName"].ToString();
                    customer.ContactPerson = dtbl.Rows[0]["ContactPerson"].ToString();
                    customer.Email = dtbl.Rows[0]["Email"].ToString();
                    customer.WebSite = dtbl.Rows[0]["WebSite"].ToString();
                    customer.Address = dtbl.Rows[0]["Address"].ToString();
                    customer.Description = dtbl.Rows[0]["Description"].ToString();
                    customer.CustomerImage = dtbl.Rows[0]["CustomerImage"].ToString();
                    customer.MobileNumber = Convert.ToInt64(dtbl.Rows[0]["MobileNumber"]);
                    customer.PhoneNumber = Convert.ToInt64(dtbl.Rows[0]["PhoneNumber"]);
                    customer.CountryId = Convert.ToInt32(dtbl.Rows[0]["CountryId"]);
                    customer.StateId = Convert.ToInt32(dtbl.Rows[0]["StateId"]);
                    customer.CityId = Convert.ToInt32(dtbl.Rows[0]["CityId"]);
                    customer.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                }
                return customer;
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

            CustomerVM customerVM = new()
            {
                CityList = objCityList.Select(u => new SelectListItem
                {
                    Text = u.CityName,
                    Value = u.Id.ToString()
                }),

                CountryList = objCountryList.Select(u => new SelectListItem
                {
                    Text = u.CountryName,
                    Value = u.Id.ToString()
                }),
                StateList = objStateList.Select(u => new SelectListItem
                {
                    Text = u.StateName,
                    Value = u.Id.ToString()
                }),

                Customer = new Customer()
            };
            if (id == null || id == 0)
            {
                //create
                return View(customerVM);
            }
            else
            {
                //update
                customerVM.Customer = FetchCustomerById(id);
                return View(customerVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(CustomerVM customerVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string customerPath = Path.Combine(wwwRootPath, @"images\customer");

                    if (!string.IsNullOrEmpty((string?)customerVM.Customer.CustomerImage))
                    {
                        //delete the old image
                        var oldImagePath =
                                    Path.Combine(wwwRootPath, (string)customerVM.Customer.CustomerImage.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(customerPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    customerVM.Customer.CustomerImage = @"\images\customer\" + fileName;
                }
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("Upsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", customerVM.Customer.Id);
                    sqlCmd.Parameters.AddWithValue("CustomerName", customerVM.Customer.CustomerName);
                    sqlCmd.Parameters.AddWithValue("ContactPerson", customerVM.Customer.ContactPerson);
                    sqlCmd.Parameters.AddWithValue("Email", customerVM.Customer.Email);
                    sqlCmd.Parameters.AddWithValue("MobileNumber", customerVM.Customer.MobileNumber);
                    sqlCmd.Parameters.AddWithValue("PhoneNumber", customerVM.Customer.PhoneNumber);
                    sqlCmd.Parameters.AddWithValue("WebSite", customerVM.Customer.WebSite);
                    sqlCmd.Parameters.AddWithValue("Address", customerVM.Customer.Address);
                    sqlCmd.Parameters.AddWithValue("CountryId", customerVM.Customer.CountryId);
                    sqlCmd.Parameters.AddWithValue("StateId", customerVM.Customer.StateId);
                    sqlCmd.Parameters.AddWithValue("CityId", customerVM.Customer.CityId);
                    sqlCmd.Parameters.AddWithValue("Description", customerVM.Customer.Description);
                    sqlCmd.Parameters.AddWithValue("CustomerImage", customerVM.Customer.CustomerImage);
                    sqlCmd.Parameters.AddWithValue("IsActive", customerVM.Customer.IsActive);
                   
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
                            return View(customerVM);
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

                return View(customerVM);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var customerToBeDeleted = FetchCustomerById(id);
            if (customerToBeDeleted == null)
            {
                TempData["error"] = "Supplier can't be Delete.";
                return RedirectToAction("Index");
            }

            var oldImagePath =
                      Path.Combine(_webHostEnvironment.WebRootPath,
                       customerToBeDeleted.CustomerImage.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("CustomerDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "customer Deleted successfully";
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
        //#endregion

    }
}
