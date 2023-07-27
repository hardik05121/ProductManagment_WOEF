

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;
using System.Drawing.Drawing2D;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Admin")]
    public class StateController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;


        public StateController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            StateIndexVM stateIndexVM = new StateIndexVM();
            List<State> objStateList = new List<State>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select S.*,C.[CountryName] As CountryName from States S INNER JOIN Countries C ON S.CountryId = C.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    State state = new State();
                    state.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    state.StateName = Convert.ToString(dtbl.Rows[i]["StateName"]);
                    state.CountryId = Convert.ToInt32(dtbl.Rows[i]["CountryId"]);
                    state.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);

                    state.Country = new Country
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(dtbl.Rows[i]["CountryName"])
                    };

                    objStateList.Add(state);
                }
            }

            stateIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "stateName_desc" : "";
            var states = (from data in objStateList /*_unitOfWork.State.GetAll(includeProperties: "Country").ToList()*/
                          where term == "" ||
                             data.StateName.ToLower().
                             Contains(term) || data.Country.CountryName.ToLower().Contains(term)


                          select new State
                          {
                              Id = data.Id,
                              StateName = data.StateName,
                              IsActive = data.IsActive,
                              CountryId = data.CountryId,
                              Country = data.Country
                          });

            switch (orderBy)
            {
                case "stateName_desc":
                    states = states.OrderByDescending(a => a.StateName);
                    break;

                default:
                    states = states.OrderBy(a => a.StateName);
                    break;
            }
            int totalRecords = states.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            states = states.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            stateIndexVM.States = states;
            stateIndexVM.CurrentPage = currentPage;
            stateIndexVM.TotalPages = totalPages;
            stateIndexVM.Term = term;
            stateIndexVM.PageSize = pageSize;
            stateIndexVM.OrderBy = orderBy;
            return View(stateIndexVM);
        }
        #endregion

        [NonAction]
        public State FetchStateById(int? id)
        {
            State state = new State();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
               //string str = "select S.*,C.[CountryName] As CountryName from States S INNER JOIN Countries C ON S.Id = Id";
               // SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
               SqlDataAdapter sqlDa = new SqlDataAdapter("StateById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    state.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    state.StateName = dtbl.Rows[0]["StateName"].ToString();
                    state.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                    state.CountryId = Convert.ToInt32(dtbl.Rows[0]["CountryId"]);
                 //   state.Country.CountryName = dtbl.Rows[0]["Country.CountryName"].ToString();
                    //{
                    //    Id = Convert.ToInt32(dtbl.Rows[0]["CountryId"]),
                    //    CountryName = Convert.ToString(dtbl.Rows[0]["CountryName"])
                    //};
                }
                return state;
            }
        }


        #region Upsert
        [HttpGet]
        public IActionResult Upsert(int? id)
        {
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
            StateVM stateVM = new()
            {
                CountryList = objCountryList.Select(u => new SelectListItem
                {
                    Text = u.CountryName,
                    Value = u.Id.ToString()
                }),
                State = new State()
            };
            if (id == null || id == 0)
            {
                //create
                //return View(stateVM);
                return View(stateVM);
            }
            else
            {
                //update
                //stateVM.State = _unitOfWork.State.Get(u => u.Id == id);
                //        return View(stateVM);
                stateVM.State = FetchStateById(id);
                return View(stateVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(StateVM stateVM)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("StateUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", stateVM.State.Id);
                    sqlCmd.Parameters.AddWithValue("StateName", stateVM.State.StateName);
                    sqlCmd.Parameters.AddWithValue("IsActive", stateVM.State.IsActive);
                    sqlCmd.Parameters.AddWithValue("CountryId", stateVM.State.CountryId);

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
                            TempData["error"] = "State Name Already Exist!";
                            return View(stateVM);
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
                return View(stateVM);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {
            var stateToBeDeleted = FetchStateById(id);
            if (stateToBeDeleted == null)
            {
                TempData["error"] = "State can't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("StateDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "State Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
