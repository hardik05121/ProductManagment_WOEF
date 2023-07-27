using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public class CityController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;


        public CityController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            CityIndexVM cityIndexVM = new CityIndexVM();
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

            cityIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "cityName_desc" : "";
            var cities = (from data in objCityList
                          where term == "" || data.CityName.ToLower().
                             Contains(term) || data.Country.CountryName.ToLower().
                             Contains(term) || data.State.StateName.ToLower().Contains(term)

                          select new City
                          {
                              Id = data.Id,
                              CityName = data.CityName,
                              State = data.State,
                              IsActive = data.IsActive,
                              Country = data.Country
                          });
            switch (orderBy)
            {
                case "cityName_desc":
                    cities = cities.OrderByDescending(a => a.CityName);
                    break;

                default:
                    cities = cities.OrderBy(a => a.CityName);
                    break;
            }
            int totalRecords = cities.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            cities = cities.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            cityIndexVM.Cities = cities;
            cityIndexVM.CurrentPage = currentPage;
            cityIndexVM.TotalPages = totalPages;
            cityIndexVM.Term = term;
            cityIndexVM.PageSize = pageSize;
            cityIndexVM.OrderBy = orderBy;
            return View(cityIndexVM);
        }

        [NonAction]
        public City FetchCityById(int? id)
        {
            City city = new City();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("CityById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    city.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    city.CityName = dtbl.Rows[0]["CityName"].ToString();
                    city.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                    city.CountryId = Convert.ToInt32(dtbl.Rows[0]["CountryId"]);
                    city.StateId = Convert.ToInt32(dtbl.Rows[0]["StateId"]);
                }
                return city;
            }
        }

        #region Upsert
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

            CityVM cityVM = new()
            {
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

                // this for add for the dropdown list
                //StateList = Enumerable.Empty<SelectListItem>(),
                City = new City()
            };

            if (id == null || id == 0)
            {
                //create
                return View(cityVM);
            }
            else
            {
                cityVM.City = FetchCityById(id);
                return View(cityVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(CityVM cityVM)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("CityUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", cityVM.City.Id);
                    sqlCmd.Parameters.AddWithValue("CityName", cityVM.City.CityName);
                    sqlCmd.Parameters.AddWithValue("IsActive", cityVM.City.IsActive);
                    sqlCmd.Parameters.AddWithValue("CountryId", cityVM.City.CountryId);
                    sqlCmd.Parameters.AddWithValue("StateId", cityVM.City.StateId);

                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "State Name Already Exist!";
                            return View(cityVM);
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
                return View(cityVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var cityToBeDeleted = FetchCityById(id);
            if (cityToBeDeleted == null)
            {
                TempData["error"] = "City can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("CityDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "City Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        #endregion

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

