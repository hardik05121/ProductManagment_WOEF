using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.Data.SqlClient;
using ProductManagment_DataAccess.Repository;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Data;
using System.Security.Cryptography.X509Certificates;

namespace ProductManagmentWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InquiryController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        public InquiryController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        #region Index
        public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
        {
            ViewData["CurrentFilter"] = term;
            term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

            InquiryIndexVM inquiryIndexVM = new InquiryIndexVM(); // page and serch mate
            List<Inquiry> objInquiryList = new List<Inquiry>(); // list mate

            DataTable dtbl = new DataTable(); // without ef mate

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select C.*,A.[CountryName] As CountryName,B.[StateName] As StateName,C.[CityName] As CityName,D.[InquirySourceName] As InquirySourceName,E.[InquiryStatusName] As InquiryStatusName,F.[Name] As Name from Inquiries G INNER JOIN Countries A ON G.CountryId = A.Id INNER JOIN States B ON G.StateId = B.Id INNER JOIN Cities C ON G.CityId = C.Id INNER JOIN InquirySources D ON G.InquirySourceId = D.Id INNER JOIN InquiryStatuses E ON G.InquiryStatusId = E.Id INNER JOIN Products F ON G.ProductId = F.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Inquiry inquiry = new Inquiry();
                    inquiry.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    inquiry.Organization = Convert.ToString(dtbl.Rows[i]["Organization"]);
                    inquiry.ContactPerson = Convert.ToString(dtbl.Rows[i]["ContactPerson"]);
                    inquiry.Email = Convert.ToString(dtbl.Rows[i]["Email"]);
                    inquiry.MobileNumber = Convert.ToInt64(dtbl.Rows[i]["MobileNumber"]);
                    inquiry.PhoneNumber = Convert.ToInt64(dtbl.Rows[i]["PhoneNumber"]);
                    inquiry.Website = Convert.ToString(dtbl.Rows[i]["Website"]);
                    inquiry.Address = Convert.ToString(dtbl.Rows[i]["Address"]);
                    inquiry.Message = Convert.ToString(dtbl.Rows[i]["Message"]);
                    inquiry.CountryId = Convert.ToInt32(dtbl.Rows[i]["CountryId"]);
                    inquiry.StateId = Convert.ToInt32(dtbl.Rows[i]["StateId"]);
                    inquiry.CityId = Convert.ToInt32(dtbl.Rows[i]["CityId"]);
                    inquiry.ProductId = Convert.ToInt32(dtbl.Rows[i]["ProductId"]);
                    inquiry.InquiryStatusId = Convert.ToInt32(dtbl.Rows[i]["InquiryStatusId"]);
                    inquiry.InquirySourceId = Convert.ToInt32(dtbl.Rows[i]["InquirySourceId"]);
                    inquiry.UserId = Convert.ToString(dtbl.Rows[i]["UserId"]);

                    // jeni Foregin key tenu Object lai levano
                    inquiry.Country = new Country
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(dtbl.Rows[i]["CountryName"])
                    };
                    inquiry.State = new State
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["StateId"]),
                        StateName = Convert.ToString(dtbl.Rows[i]["StateName"])
                    };
                    inquiry.City = new City
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CityId"]),
                        CityName = Convert.ToString(dtbl.Rows[i]["CityName"])
                    };
                    inquiry.Product = new Product
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["ProductId"]),
                        Name = Convert.ToString(dtbl.Rows[i]["Name"])
                    };
                    inquiry.InquiryStatus = new InquiryStatus
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["InquiryStatusId"]),
                        InquiryStatusName = Convert.ToString(dtbl.Rows[i]["InquiryStatusName"])
                    };
                    inquiry.InquirySource = new InquirySource
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["InquirySourceId"]),
                        InquirySourceName = Convert.ToString(dtbl.Rows[i]["InquirySourceName"])
                    };
                    objInquiryList.Add(inquiry);
                }
            }
            inquiryIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "email_desc" : "";
            var inquiries = (from data in objInquiryList /*_unitOfWork.State.GetAll(includeProperties: "Country").ToList()*/
                             where term == "" || data.Email.ToLower().
                              Contains(term) || data.InquirySource.InquirySourceName.ToLower().Contains(term)

                             select new Inquiry
                             {
                                 Id = data.Id,
                                 Organization = data.Organization,
                                 ContactPerson = data.ContactPerson,
                                 Email = data.Email,
                                 MobileNumber = data.MobileNumber,
                                 PhoneNumber = data.PhoneNumber,
                                 Website = data.Website,
                                 Address = data.Address,
                                 Country = data.Country,
                                 Product = data.Product,
                                 State = data.State,
                                 City = data.City,
                                 InquirySource = data.InquirySource,
                                 InquiryStatus = data.InquiryStatus,
                             });

            switch (orderBy)
            {
                case "email_desc":
                    inquiries = inquiries.OrderByDescending(a => a.Email);
                    break;

                default:
                    inquiries = inquiries.OrderBy(a => a.Email);
                    break;
            }
            int totalRecords = inquiries.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            inquiries = inquiries.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            inquiryIndexVM.Inquiries = inquiries;
            inquiryIndexVM.CurrentPage = currentPage;
            inquiryIndexVM.TotalPages = totalPages;
            inquiryIndexVM.Term = term;
            inquiryIndexVM.PageSize = pageSize;
            inquiryIndexVM.OrderBy = orderBy;
            return View(inquiryIndexVM);
        }
        #endregion


        [NonAction]
        public Inquiry FetchInquiryById(int? id)
        {
            Inquiry inquiry = new Inquiry();
            DataTable dtbl = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                //string str = "select S.*,C.[CountryName] As CountryName from States S INNER JOIN Countries C ON S.Id = Id";
                // SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                SqlDataAdapter sqlDa = new SqlDataAdapter("InquieryById", sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
                sqlDa.Fill(dtbl);

                if (dtbl.Rows.Count == 1)
                {
                    inquiry.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                    inquiry.Organization = dtbl.Rows[0]["Organization"].ToString();
                    inquiry.ContactPerson = dtbl.Rows[0]["ContactPerson"].ToString();
                    inquiry.Email = dtbl.Rows[0]["Email"].ToString();
                    inquiry.MobileNumber = Convert.ToInt32(dtbl.Rows[0]["MobileNumber"].ToString());
                    inquiry.PhoneNumber = Convert.ToInt32(dtbl.Rows[0]["PhoneNumber"].ToString());
                    inquiry.Website = dtbl.Rows[0]["Website"].ToString();
                    inquiry.Address = dtbl.Rows[0]["Address"].ToString();
                    inquiry.Message = dtbl.Rows[0]["Message"].ToString();
                    inquiry.CountryId = Convert.ToInt32(dtbl.Rows[0]["CountryId"]);
                    inquiry.StateId = Convert.ToInt32(dtbl.Rows[0]["StateId"]);
                    inquiry.CityId = Convert.ToInt32(dtbl.Rows[0]["CityId"]);
                    inquiry.UserId = Convert.ToString(dtbl.Rows[0]["UserId"]);
                    inquiry.ProductId = Convert.ToInt32(dtbl.Rows[0]["ProductId"]);
                    inquiry.InquiryStatusId = Convert.ToInt32(dtbl.Rows[0]["InquiryStatusId"]);
                    inquiry.InquirySourceId = Convert.ToInt32(dtbl.Rows[0]["InquirySourceId"]);
                }
                return inquiry;
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
            DataTable CIdtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select C.*,S.[StateName] As StateName,A.[CountryName] As CountryName from Cities C INNER JOIN States S ON C.StateId = S.Id INNER JOIN Countries A ON C.CountryId = A.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(CIdtbl);

                for (int i = 0; i < CIdtbl.Rows.Count; i++)
                {
                    City city = new City();
                    city.Id = Convert.ToInt32(CIdtbl.Rows[i]["Id"]);
                    city.CityName = Convert.ToString(CIdtbl.Rows[i]["CityName"]);
                    city.CountryId = Convert.ToInt32(CIdtbl.Rows[i]["CountryId"]);
                    city.StateId = Convert.ToInt32(CIdtbl.Rows[i]["StateId"]);
                    city.IsActive = Convert.ToBoolean(CIdtbl.Rows[i]["IsActive"]);

                    city.Country = new Country
                    {
                        Id = Convert.ToInt32(CIdtbl.Rows[i]["CountryId"]),
                        CountryName = Convert.ToString(CIdtbl.Rows[i]["CountryName"])
                    };
                    city.State = new State
                    {
                        Id = Convert.ToInt32(CIdtbl.Rows[i]["StateId"]),
                        StateName = Convert.ToString(CIdtbl.Rows[i]["StateName"])
                    };

                    objCityList.Add(city);
                }
            }

            List<InquirySource> objInquirySourceList = new List<InquirySource>();
            DataTable Idtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select * from InquirySources";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(Idtbl);

                for (int i = 0; i < Idtbl.Rows.Count; i++)
                {
                    InquirySource inquirySource = new InquirySource();
                    inquirySource.Id = Convert.ToInt32(Idtbl.Rows[i]["Id"]);
                    inquirySource.InquirySourceName = Convert.ToString(Idtbl.Rows[i]["InquirySourceName"]);
                    inquirySource.IsActive = Convert.ToBoolean(Idtbl.Rows[i]["IsActive"]);
                    objInquirySourceList.Add(inquirySource);
                }
            }

            List<InquiryStatus> objInquiryStatusList = new List<InquiryStatus>();
            DataTable ISdtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select * from InquiryStatuses";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(ISdtbl);

                for (int i = 0; i < ISdtbl.Rows.Count; i++)
                {
                    InquiryStatus inquiryStatus = new InquiryStatus();
                    inquiryStatus.Id = Convert.ToInt32(ISdtbl.Rows[i]["Id"]);
                    inquiryStatus.InquiryStatusName = Convert.ToString(ISdtbl.Rows[i]["InquiryStatusName"]);
                    inquiryStatus.IsActive = Convert.ToBoolean(ISdtbl.Rows[i]["IsActive"]);
                    objInquiryStatusList.Add(inquiryStatus);
                }
            }

            List<Product> objProductList = new List<Product>();
            DataTable dtbl = new DataTable();
            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                string str = "select P.*,B.[BrandName] As BrandName,C.[Name] As CategoryName,U.[BaseUnit] As BaseUnit,W.[WarehouseName],T.[Name] As Name from Products P INNER JOIN Brands B ON P.BrandId = B.Id INNER JOIN Categories C ON P.CategoryId = C.Id INNER JOIN Units U ON P.UnitId = U.Id INNER JOIN Warehouses W ON P.WarehouseId = W.Id INNER JOIN Taxs T ON P.TaxId = T.Id";
                SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
                sqlDa.SelectCommand.CommandType = CommandType.Text;
                sqlDa.Fill(dtbl);

                for (int i = 0; i < dtbl.Rows.Count; i++)
                {
                    Product product = new Product();
                    product.Id = Convert.ToInt32(dtbl.Rows[i]["Id"]);
                    product.Name = Convert.ToString(dtbl.Rows[i]["Name"]);
                    product.Code = Convert.ToString(dtbl.Rows[i]["Code"]);
                    product.SkuCode = Convert.ToString(dtbl.Rows[i]["SkuCode"]);
                    product.SkuName = Convert.ToString(dtbl.Rows[i]["SkuName"]);
                    product.SalesPrice = Convert.ToDouble(dtbl.Rows[i]["SalesPrice"]);
                    product.PurchasePrice = Convert.ToDouble(dtbl.Rows[i]["PurchasePrice"]);
                    product.Mrp = Convert.ToDouble(dtbl.Rows[i]["Mrp"]);
                    product.BarcodeNumber = Convert.ToInt64(dtbl.Rows[i]["BarcodeNumber"]);
                    product.Description = Convert.ToString(dtbl.Rows[i]["Description"]);
                    product.CreatedDate = Convert.ToDateTime(dtbl.Rows[i]["CreatedDate"]);
                    product.UpdatedDate = Convert.ToDateTime(dtbl.Rows[i]["UpdatedDate"]);
                    product.ProductImage = Convert.ToString(dtbl.Rows[i]["ProductImage"]);
                    product.BrandId = Convert.ToInt32(dtbl.Rows[i]["BrandId"]);
                    product.CategoryId = Convert.ToInt32(dtbl.Rows[i]["CategoryId"]);
                    product.UnitId = Convert.ToInt32(dtbl.Rows[i]["UnitId"]);
                    product.WarehouseId = Convert.ToInt32(dtbl.Rows[i]["WarehouseId"]);
                    product.TaxId = Convert.ToInt32(dtbl.Rows[i]["TaxId"]);
                    product.IsActive = Convert.ToBoolean(dtbl.Rows[i]["IsActive"]);

                    product.Brand = new Brand
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["BrandId"]),
                        BrandName = Convert.ToString(dtbl.Rows[i]["BrandName"])
                    };
                    product.Category = new Category
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["CategoryId"]),
                        Name = Convert.ToString(dtbl.Rows[i]["Name"])
                    };
                    product.Unit = new Unit
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["UnitId"]),
                        BaseUnit = Convert.ToString(dtbl.Rows[i]["BaseUnit"])
                    };

                    product.Warehouse = new Warehouse
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["WarehouseId"]),
                        WarehouseName = Convert.ToString(dtbl.Rows[i]["WarehouseName"])
                    };
                    product.Tax = new Tax
                    {
                        Id = Convert.ToInt32(dtbl.Rows[i]["TaxId"]),
                        Name = Convert.ToString(dtbl.Rows[i]["Name"])
                    };
                    objProductList.Add(product);
                }
            }

            InquiryVM inquiryVM = new()
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
                ProductList = objProductList.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                //UserList = _unitOfWork.User.GetAll().Select(u => new SelectListItem
                //{
                //    Text = u.FirstName,
                //    Value = u.Id.ToString()
                //}), 
                SourceList = objInquirySourceList.Select(u => new SelectListItem
                {
                    Text = u.InquirySourceName,
                    Value = u.Id.ToString()
                }),
                StatusList = objInquiryStatusList.Select(u => new SelectListItem
                {
                    Text = u.InquiryStatusName,
                    Value = u.Id.ToString()
                }),
                Inquiry = new Inquiry()
            };

            if (id == null || id == 0)
            {
                //create
                return View(inquiryVM);
            }
            else
            {
                //update
                inquiryVM.Inquiry = FetchInquiryById(id);
                return View(inquiryVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(InquiryVM inquiryVM)
        {
            if (ModelState.IsValid)
            {
                using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
                {
                    sqlConnection.Open();
                    SqlCommand sqlCmd = new SqlCommand("Upsert", sqlConnection);
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCmd.Parameters.AddWithValue("Id", inquiryVM.Inquiry.Id);
                    sqlCmd.Parameters.AddWithValue("Organization", inquiryVM.Inquiry.Organization);
                    sqlCmd.Parameters.AddWithValue("ContactPerson", inquiryVM.Inquiry.ContactPerson);
                    sqlCmd.Parameters.AddWithValue("Email", inquiryVM.Inquiry.Email);
                    sqlCmd.Parameters.AddWithValue("MobileNumber", inquiryVM.Inquiry.MobileNumber);
                    sqlCmd.Parameters.AddWithValue("PhoneNumber", inquiryVM.Inquiry.PhoneNumber);
                    sqlCmd.Parameters.AddWithValue("Website", inquiryVM.Inquiry.Website);
                    sqlCmd.Parameters.AddWithValue("Address", inquiryVM.Inquiry.Address);
                    sqlCmd.Parameters.AddWithValue("Message", inquiryVM.Inquiry.Message);
                    sqlCmd.Parameters.AddWithValue("CountryId", inquiryVM.Inquiry.CountryId);
                    sqlCmd.Parameters.AddWithValue("StateId", inquiryVM.Inquiry.StateId);
                    sqlCmd.Parameters.AddWithValue("CityId", inquiryVM.Inquiry.CityId);
                    sqlCmd.Parameters.AddWithValue("ProductId", inquiryVM.Inquiry.ProductId);
                    sqlCmd.Parameters.AddWithValue("UserId", inquiryVM.Inquiry.UserId);
                    sqlCmd.Parameters.AddWithValue("InquirySourceId", inquiryVM.Inquiry.InquirySourceId);
                    sqlCmd.Parameters.AddWithValue("InquiryStatusId", inquiryVM.Inquiry.InquiryStatusId);
                    
                    try
                    {
                        sqlCmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                        {
                            ModelState.AddModelError("", ex.Message);
                            TempData["error"] = "inquiry Name Already Exist!";
                            return View(inquiryVM);
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
                return View(inquiryVM);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Delete(int id)
        {
            var inquiryToBeDeleted = FetchInquiryById(id);
            if (inquiryToBeDeleted == null)
            {
                TempData["error"] = "Product can't be Delete.";
                return RedirectToAction("Index");
            }

            //var oldImagePath =
            //               Path.Combine(_webHostEnvironment.WebRootPath,
            //               inquiryToBeDeleted.InquiryImage.TrimStart('\\'));

            //if (System.IO.File.Exists(oldImagePath))
            //{
            //    System.IO.File.Delete(oldImagePath);
            //}

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("InquiryDeleteById", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", id);
                sqlCmd.ExecuteNonQuery();
            }
            TempData["success"] = "Inquiry Deleted successfully";
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
