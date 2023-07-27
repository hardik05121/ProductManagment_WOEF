using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
   // [Authorize(Roles = "Admin")]
    public class UnitController : Controller
    {

        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public UnitController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            this._configuration = configuration;
        }

        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            UnitIndexVM unitIndexVM = new UnitIndexVM();
            List<Unit> objUnitList = new List<Unit>();

            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();

                string str = "select * from Units";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Unit unit = new Unit();
                    unit.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    unit.UnitName = Convert.ToString(dtbl.Rows[i]["UnitName"]);
                    unit.BaseUnit= Convert.ToString(dtbl.Rows[i]["BaseUnit"]);
                    unit.UnitCode = Convert.ToInt32(dtbl.Rows[i]["UnitCode"]);
                    objUnitList.Add(unit);
                }


            }

            unitIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "unitName_desc" : "";
            var units = (from data in objUnitList
                         where term == "" ||
                             data.UnitName.ToLower().
                             Contains(term)


                          select new Unit
                          {
                              Id = data.Id,
                              UnitName = data.UnitName,
                              UnitCode = data.UnitCode,
                              BaseUnit = data.BaseUnit
                          });
            switch (orderBy)
            {
                case "unitName_desc":
                    units = units.OrderByDescending(a => a.UnitName);
                    break;

                default:
                    units = units.OrderBy(a => a.UnitName);
                    break;
            }
            int totalRecords = units.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            units = units.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            unitIndexVM.Units = units;
            unitIndexVM.CurrentPage = currentPage;
            unitIndexVM.TotalPages = totalPages;
            unitIndexVM.Term = term;
            unitIndexVM.PageSize = pageSize;
            unitIndexVM.OrderBy = orderBy;
            return View(unitIndexVM);


        }

        [NonAction]
        public Unit FetchUnitById(int? id)
        {
            Unit unit = new Unit();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlDataAdapter sqlDa = new SqlDataAdapter("UnitById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    unit.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    unit.UnitName = dtbl.Rows[0]["UnitName"].ToString();
                    unit.BaseUnit = dtbl.Rows[0]["BaseUnit"].ToString();
                    unit.UnitCode = Convert.ToInt32(dtbl.Rows[0]["UnitCode"]);
                }
                return unit;
            }
        }

        #region Upsert
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new Unit());
            }
            else
            {
                //update
                Unit unit = FetchUnitById(id);
                return View(unit);
            }

        }

        [HttpPost]
        public IActionResult Upsert(Unit unit)
        {

            if (ModelState.IsValid)
            {

                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("UnitUpsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", unit.Id);
                    sqlCmd.Parameters.AddWithValue("UnitName", unit.UnitName);
                    sqlCmd.Parameters.AddWithValue("BaseUnit", unit.BaseUnit);
                    sqlCmd.Parameters.AddWithValue("UnitCode", unit.UnitCode);

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
                            TempData["error"] = "unit Name Already Exist!";
                            return View(unit);
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
                return View(unit);
            }
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int? id)
        {

            //Brand brandToBeDeleted = FetchBrandByID(id);
            var unitToBeDeleted = FetchUnitById(id);
            if (unitToBeDeleted == null)
            {
                TempData["error"] = "Unit vcan't be Delete.";
                return RedirectToAction("Index");
            }
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("UnitDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "unit Deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
