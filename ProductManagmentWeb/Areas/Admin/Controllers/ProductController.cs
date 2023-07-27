using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using ProductManagment_DataAccess.Repository.IRepository;
using ProductManagment_Models.Models;
using ProductManagment_Models.ViewModels;
using System.Collections.Generic;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ProductManagmentWeb.Areas.Admin.Controllers;

[Area("Admin")]
//[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;
    public ProductController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
    }

    public IActionResult Index(string term = "", string orderBy = "", int currentPage = 1)
    {
        ViewData["CurrentFilter"] = term;
        term = string.IsNullOrEmpty(term) ? "" : term.ToLower();

        ProductIndexVM productIndexVM = new ProductIndexVM();
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
                        if (dtbl.Rows[i]["UpdatedDate"] != DBNull.Value)
                        {
                            product.UpdatedDate = Convert.ToDateTime(dtbl.Rows[i]["UpdatedDate"]);
                        }
                        else
                        {
                            product.UpdatedDate = null;
                        }
                //if (dtbl.Rows[i]["UpdatedDate"] != DBNull.Value)
                //{
                //    if (DateTime.TryParse(dtbl.Rows[i]["UpdatedDate"].ToString(), out DateTime updatedDate))
                //    {
                //        product.UpdatedDate = updatedDate;
                //    }
                //    else
                //    {
                //        // Handle the case when the value in the DataTable is not a valid DateTime format.
                //        // You might choose to set a default value or take appropriate action based on your requirements.
                //    }
                //}
                //else
                //{
                //    // Handle the case when dtbl.Rows[i]["UpdatedDate"] is DBNull.Value (null).
                //    // You might choose to set product.UpdatedDate to null or take appropriate action based on your requirements.
                //    product.UpdatedDate = null; // Assuming the UpdatedDate property is nullable (DateTime?).
                //}


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

            productIndexVM.NameSortOrder = string.IsNullOrEmpty(orderBy) ? "name_desc" : "";
            var products = (from data in objProductList /* _unitOfWork.Product.GetAll(includeProperties: "Brand,Category,Unit,Warehouse,Tax").ToList()*/
                            where term == "" || data.Name.ToLower().
                            Contains(term) || data.Brand.BrandName.ToLower().
                            Contains(term) || data.Category.Name.ToLower().
                            Contains(term) || data.Unit.UnitName.ToLower().
                            Contains(term) || data.Warehouse.WarehouseName.ToLower().
                            Contains(term) || data.Tax.Name.ToLower().Contains(term)


                            select new Product
                            {
                                Id = data.Id,
                                Name = data.Name,
                                Category = data.Category,
                                SkuCode = data.SkuName,
                                SkuName = data.SkuName,
                                Mrp = data.Mrp,
                                SalesPrice = data.SalesPrice,
                                Code = data.Code,
                                Unit = data.Unit,
                                BarcodeNumber = data.BarcodeNumber,
                                Tax = data.Tax,
                                PurchasePrice = data.PurchasePrice,
                                Description = data.Description,
                                Warehouse = data.Warehouse,
                                Brand = data.Brand,
                                IsActive = data.IsActive,
                                ProductImage = data.ProductImage
                            });

            switch (orderBy)
            {
                case "stateName_desc":
                    products = products.OrderByDescending(a => a.Name);
                    break;

                default:
                    products = products.OrderBy(a => a.Name);
                    break;
            }
            int totalRecords = products.Count();
            int pageSize = 5;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            products = products.Skip((currentPage - 1) * pageSize).Take(pageSize);
            // current=1, skip= (1-1=0), take=5 
            // currentPage=2, skip (2-1)*5 = 5, take=5 ,
            productIndexVM.Products = products;
            productIndexVM.CurrentPage = currentPage;
            productIndexVM.TotalPages = totalPages;
            productIndexVM.Term = term;
            productIndexVM.PageSize = pageSize;
            productIndexVM.OrderBy = orderBy;
            return View(productIndexVM);
        }
    }

    [NonAction]
    public Product FetchProductById(int? id)
    {
        Product product = new Product();
        DataTable dtbl = new DataTable();

        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();
            SqlDataAdapter sqlDa = new SqlDataAdapter("ProductById", sqlConnection);
            sqlDa.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlDa.SelectCommand.Parameters.AddWithValue("Id", id);
            sqlDa.Fill(dtbl);

            if (dtbl.Rows.Count == 1)
            {
                product.Id = Convert.ToInt32(dtbl.Rows[0]["Id"].ToString());
                product.Name = dtbl.Rows[0]["Name"].ToString();
                product.Code = dtbl.Rows[0]["Code"].ToString();
                product.SkuCode = dtbl.Rows[0]["SkuCode"].ToString();
                product.SkuName = dtbl.Rows[0]["SkuName"].ToString();
                product.Description = dtbl.Rows[0]["Description"].ToString();
                product.IsActive = Convert.ToBoolean(dtbl.Rows[0]["IsActive"]);
                product.ProductImage = dtbl.Rows[0]["ProductImage"].ToString();
                product.CreatedDate = Convert.ToDateTime(dtbl.Rows[0]["CreatedDate"]);
                if (dtbl.Rows[0]["UpdatedDate"] != DBNull.Value)
                {
                    product.UpdatedDate = Convert.ToDateTime(dtbl.Rows[0]["UpdatedDate"]);
                }
                else
                {
                    product.UpdatedDate = null;
                }
                // product.UpdatedDate = Convert.ToDateTime(dtbl.Rows[0]["UpdatedDate"]);
                product.SalesPrice = Convert.ToDouble(dtbl.Rows[0]["SalesPrice"]);
                product.PurchasePrice = Convert.ToDouble(dtbl.Rows[0]["PurchasePrice"]);
                product.Mrp = Convert.ToDouble(dtbl.Rows[0]["Mrp"]);
                product.BarcodeNumber = Convert.ToInt64(dtbl.Rows[0]["BarcodeNumber"]);
                product.BrandId = Convert.ToInt32(dtbl.Rows[0]["BrandId"]);
                product.CategoryId = Convert.ToInt32(dtbl.Rows[0]["CategoryId"]);
                product.UnitId = Convert.ToInt32(dtbl.Rows[0]["UnitId"]);
                product.WarehouseId = Convert.ToInt32(dtbl.Rows[0]["WarehouseId"]);
                product.TaxId = Convert.ToInt32(dtbl.Rows[0]["TaxId"]);
            }
            return product;
        }
    }


    [HttpGet] // to grt the data on display.
    public IActionResult Upsert(int? id)
    {
        List<Brand> objBrandList = new List<Brand>();
        DataTable Bdtbl = new DataTable();
        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();
            string str = "select * from Brands";
            SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
            // SqlDataAdapter sqlDa = new SqlDataAdapter(GetBrand, sqlConnection);
            sqlDa.SelectCommand.CommandType = CommandType.Text;
            sqlDa.Fill(Bdtbl);

            for (int i = 0; i < Bdtbl.Rows.Count; i++)
            {
                Brand brand = new Brand();
                brand.Id = Convert.ToInt32(Bdtbl.Rows[i]["Id"]);
                brand.BrandName = Convert.ToString(Bdtbl.Rows[i]["BrandName"]);
                brand.BrandImage = Convert.ToString(Bdtbl.Rows[i]["BrandImage"]);
                objBrandList.Add(brand);
            }
        }

        List<Category> objCategoryList = new List<Category>();
        DataTable Cdtbl = new DataTable();
        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();
            string str = "select * from Categories";
            SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
            sqlDa.SelectCommand.CommandType = CommandType.Text;
            sqlDa.Fill(Cdtbl);

            for (int i = 0; i < Cdtbl.Rows.Count; i++)
            {
                Category category = new Category();
                category.Id = Convert.ToInt32(Cdtbl.Rows[i]["Id"]);
                category.Name = Convert.ToString(Cdtbl.Rows[i]["Name"]);
                category.Description = Convert.ToString(Cdtbl.Rows[i]["Description"]);
                category.IsActive = Convert.ToBoolean(Cdtbl.Rows[i]["IsActive"]);
                objCategoryList.Add(category);
            }
        }

        List<Tax> objTaxList = new List<Tax>();
        DataTable Tdtbl = new DataTable();
        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();
            string str = "select * from Taxs";
            SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
            sqlDa.SelectCommand.CommandType = CommandType.Text;
            sqlDa.Fill(Tdtbl);

            for (int i = 0; i < Tdtbl.Rows.Count; i++)
            {
                Tax tax = new Tax();
                tax.Id = Convert.ToInt32(Tdtbl.Rows[i]["Id"]);
                tax.Name = Convert.ToString(Tdtbl.Rows[i]["Name"]);
                tax.Percentage = Convert.ToDouble(Tdtbl.Rows[i]["Percentage"]);
                objTaxList.Add(tax);
            }
        }

        List<Unit> objUnitList = new List<Unit>();
        DataTable Udtbl = new DataTable();
        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();

            string str = "select * from Units";
            SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
            sqlDa.SelectCommand.CommandType = CommandType.Text;
            sqlDa.Fill(Udtbl);

            for (int i = 0; i < Udtbl.Rows.Count; i++)
            {
                Unit unit = new Unit();
                unit.Id = Convert.ToInt32(Udtbl.Rows[i]["Id"]);
                unit.UnitName = Convert.ToString(Udtbl.Rows[i]["UnitName"]);
                unit.BaseUnit = Convert.ToString(Udtbl.Rows[i]["BaseUnit"]);
                unit.UnitCode = Convert.ToInt32(Udtbl.Rows[i]["UnitCode"]);
                objUnitList.Add(unit);
            }
        }

        List<Warehouse> objWarehouseList = new List<Warehouse>();
        DataTable Wdtbl = new DataTable();
        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();
            string str = "select * from Warehouses";
            SqlDataAdapter sqlDa = new SqlDataAdapter(str, sqlConnection);
            sqlDa.SelectCommand.CommandType = CommandType.Text;
            sqlDa.Fill(Wdtbl);

            for (int i = 0; i < Wdtbl.Rows.Count; i++)
            {
                Warehouse warehouse = new Warehouse();
                warehouse.Id = Convert.ToInt32(Wdtbl.Rows[i]["Id"]);
                warehouse.WarehouseName = Convert.ToString(Wdtbl.Rows[i]["WarehouseName"]);
                warehouse.MobileNumber = Convert.ToInt64(Wdtbl.Rows[i]["MobileNumber"]);
                warehouse.ContactPerson = Convert.ToInt64(Wdtbl.Rows[i]["ContactPerson"]);
                warehouse.Email = Convert.ToString(Wdtbl.Rows[i]["Email"]);
                warehouse.Address = Convert.ToString(Wdtbl.Rows[i]["Address"]);
                warehouse.IsActive = Convert.ToBoolean(Wdtbl.Rows[i]["IsActive"]);
                objWarehouseList.Add(warehouse);
            }
        }

        ProductVM productVM = new()
        {
            BrandList = objBrandList.Select(u => new SelectListItem
            {
                Text = u.BrandName,
                Value = u.Id.ToString()
            }),
            CategoryList = objCategoryList.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            UnitList = objUnitList.Select(u => new SelectListItem
            {
                Text = u.BaseUnit,
                Value = u.Id.ToString()
            }),
            WarehouseList = objWarehouseList.Select(u => new SelectListItem
            {
                Text = u.WarehouseName,
                Value = u.Id.ToString()
            }),
            TaxList = objTaxList.Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }),
            Product = new Product()
        };

        if (id == null || id == 0)
        {
            //create
            return View(productVM);
        }
        else
        {
            //update
            productVM.Product = FetchProductById(id);
            return View(productVM);
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Upsert(ProductVM productVM, IFormFile? file)
    {
        if (ModelState.IsValid)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                if (!string.IsNullOrEmpty(productVM.Product.ProductImage))
                {
                    //delete the old image
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ProductImage.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                productVM.Product.ProductImage = @"\images\product\" + fileName;
            }

            using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
            {
                sqlConnection.Open();
                SqlCommand sqlCmd = new SqlCommand("ProductUpsert", sqlConnection);
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.Parameters.AddWithValue("Id", productVM.Product.Id);
                sqlCmd.Parameters.AddWithValue("Name", productVM.Product.Name);
                sqlCmd.Parameters.AddWithValue("Code", productVM.Product.Code);
                sqlCmd.Parameters.AddWithValue("SkuCode", productVM.Product.SkuCode);
                sqlCmd.Parameters.AddWithValue("SkuName", productVM.Product.SkuName);
                sqlCmd.Parameters.AddWithValue("SalesPrice", productVM.Product.SalesPrice);
                sqlCmd.Parameters.AddWithValue("Mrp", productVM.Product.Mrp);
                sqlCmd.Parameters.AddWithValue("PurchasePrice", productVM.Product.PurchasePrice);
                sqlCmd.Parameters.AddWithValue("BarcodeNumber", productVM.Product.BarcodeNumber);
                sqlCmd.Parameters.AddWithValue("Description", productVM.Product.Description);
                //if(productVM.Product.CreatedDate != null)
                //{
                sqlCmd.Parameters.AddWithValue("CreatedDate", productVM.Product.CreatedDate);
                //}
                //else
                //{
                //    sqlCmd.Parameters.AddWithValue("CreatedDate", DBNull.Value);
                //}
                if (productVM.Product.UpdatedDate != null)
                {
                    sqlCmd.Parameters.AddWithValue("UpdatedDate", productVM.Product.UpdatedDate);
                }
                else
                {
                    sqlCmd.Parameters.AddWithValue("UpdatedDate", DBNull.Value);
                }


                sqlCmd.Parameters.AddWithValue("ProductImage", productVM.Product.ProductImage);
                sqlCmd.Parameters.AddWithValue("BrandId", productVM.Product.BrandId);
                sqlCmd.Parameters.AddWithValue("CategoryId", productVM.Product.CategoryId);
                sqlCmd.Parameters.AddWithValue("UnitId", productVM.Product.UnitId);
                sqlCmd.Parameters.AddWithValue("WarehouseId", productVM.Product.WarehouseId);
                sqlCmd.Parameters.AddWithValue("TaxId", productVM.Product.TaxId);
                sqlCmd.Parameters.AddWithValue("IsActive", productVM.Product.IsActive);

                try
                {
                    sqlCmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 50000) // Custom error number used in the stored procedure RAISERROR statements
                    {
                        ModelState.AddModelError("", ex.Message);
                        TempData["error"] = ex.Message;
                        return View(productVM);
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

            return View(productVM);
        }

    }

    [HttpPost]
    [ValidateAntiForgeryToken]

    public IActionResult Delete(int id)
    {
        var productToBeDeleted = FetchProductById(id);
        if (productToBeDeleted == null)
        {
            TempData["error"] = "Product can't be Delete.";
            return RedirectToAction("Index");
        }

        var oldImagePath =
                       Path.Combine(_webHostEnvironment.WebRootPath,
                       productToBeDeleted.ProductImage.TrimStart('\\'));

        if (System.IO.File.Exists(oldImagePath))
        {
            System.IO.File.Delete(oldImagePath);
        }

        using (SqlConnection sqlConnection = new SqlConnection(_configuration.GetConnectionString("defaultconnection")))
        {
            sqlConnection.Open();
            SqlCommand sqlCmd = new SqlCommand("ProductDeleteById", sqlConnection);
            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.AddWithValue("Id", id);
            sqlCmd.ExecuteNonQuery();
        }
        TempData["success"] = "Product Deleted successfully";
        return RedirectToAction(nameof(Index));
    }
}

