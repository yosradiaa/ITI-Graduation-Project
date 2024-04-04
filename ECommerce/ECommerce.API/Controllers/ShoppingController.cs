using ECommerce.API.DataAccess;
using ECommerce.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Numerics;
using ECommerce.API.DTO;



namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingController : ControllerBase
    {
        readonly IDataAccess dataAccess;
        private readonly string DateFormat;
        private readonly string _imageDirectory;
        public ShoppingController(IDataAccess dataAccess, IConfiguration configuration)
        {
            this.dataAccess = dataAccess;
            DateFormat = configuration["Constants:DateFormat"];
            _imageDirectory = "wwwroot/products/";
        }

        [HttpGet("GetCategoryList")]
        public IActionResult GetCategoryList()
        {
            var result = dataAccess.GetProductCategories();
            return Ok(result);
        }

        [HttpGet("GetProducts")]
        public IActionResult GetProducts(string category, string subcategory, int count)
        {
            var result = dataAccess.GetProducts(category, subcategory, count);
            return Ok(result);
        }

        [HttpGet("GetProduct/{id}")]
        public IActionResult GetProduct(int id)
        {
            var result = dataAccess.GetProduct(id);
            return Ok(result);
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] User user)
        {
            user.CreatedAt = DateTime.Now.ToString(DateFormat);
            user.ModifiedAt = DateTime.Now.ToString(DateFormat);

            var result = dataAccess.InsertUser(user);

            string? message;
            if (result) message = "inserted";
            else message = "email not available";
            return Ok(message);
        }

        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts()
        {
            var result = dataAccess.GetAllProducts();
            return Ok(result);
        }


        [HttpPost("LoginUser")]
        public IActionResult LoginUser([FromBody] User user)
        {
            var token = dataAccess.IsUserPresent(user.Email, user.Password);
            if (token == "") token = "invalid";
            return Ok(token);
        }

        [HttpPost("InsertReview")]
        public IActionResult InsertReview([FromBody] Review review)
        {
            review.CreatedAt = DateTime.Now.ToString(DateFormat);
            dataAccess.InsertReview(review);
            return Ok("inserted");
        }

        [HttpGet("GetProductReviews/{productId}")]
        public IActionResult GetProductReviews(int productId)
        {
            var result = dataAccess.GetProductReviews(productId);
            return Ok(result);
        }

        [HttpPost("InsertCartItem/{userid}/{productid}")]
        public IActionResult InsertCartItem(int userid, int productid)
        {
            var result = dataAccess.InsertCartItem(userid, productid);
            return Ok(result ? "inserted" : "not inserted");
        }



        [HttpDelete("RemoveCartItem/{cartItemId}")]
        public IActionResult RemoveCartItem(int cartItemId)
        {
            var result = dataAccess.RemoveCartItem(cartItemId);
            return Ok(result ? "removed" : "not removed");
        }


        [HttpGet("GetActiveCartOfUser/{id}")]
        public IActionResult GetActiveCartOfUser(int id)
        {
            var result = dataAccess.GetActiveCartOfUser(id);
            return Ok(result);
        }

        [HttpGet("GetAllPreviousCartsOfUser/{id}")]
        public IActionResult GetAllPreviousCartsOfUser(int id)
        {
            var result = dataAccess.GetAllPreviousCartsOfUser(id);
            return Ok(result);
        }

        [HttpGet("GetPaymentMethods")]
        public IActionResult GetPaymentMethods()
        {
            var result = dataAccess.GetPaymentMethods();
            return Ok(result);
        }

        [HttpGet("GetOffers")]
        public IActionResult GetOffers()
        {
            var result = dataAccess.GetOffers();
            return Ok(result);
        }

        [HttpPost("InsertPayment")]
        public IActionResult InsertPayment(Payment payment)
        {
            payment.CreatedAt = DateTime.Now.ToString();
            var id = dataAccess.InsertPayment(payment);
            return Ok(id.ToString());
        }

        [HttpPost("InsertOrder")]
        public IActionResult InsertOrder(Order order)
        {
            order.CreatedAt = DateTime.Now.ToString();
            var id = dataAccess.InsertOrder(order);
            return Ok(id.ToString());
        }




        [HttpDelete("DeleteProduct/{id}")]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                bool result = dataAccess.DeleteProduct(id);

                if (result)
                {
                    return Ok(new { success = true, message = "Product deleted successfully" });
                }
                else
                {
                    return NotFound(new { success = false, message = "Product not found or failed to delete" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("EditProduct/{id}")]
        public async Task<IActionResult> EditProduct([FromRoute]int id , [FromForm] ProductDto productDto)
        {

            Product product = dataAccess.GetProduct(id);

            if(product != null)
            {
                if(ModelState.IsValid)
                {
                    #region CheckImage
                    if (Request.Form.Files.Count > 0)
                    {
                        ResultFile result = await dataAccess.UploadFile(productDto.file!, "products");

                        if (result.Successed)
                        {

                            productDto.Image = result.Url;

                            //if already have image
                            if (!string.IsNullOrEmpty(product.Image))
                            {
                                dataAccess.RemoveFile(product.Image);
                            }

                        }
                        else
                        {
                            //Handle Response
                            return BadRequest("Something is wrong..!!");
                        }

                    }
                    else if (productDto.file is null && !string.IsNullOrEmpty(product.Image))
                    {
                        productDto.Image = product.Image;
                    }
                    #endregion
                }
                else
                {
                    return BadRequest("something is wrong..!!");
                }

                ProductCategory category = dataAccess.GetProductCategory(productDto.ProductCategory);
                Offer offer = dataAccess.GetOffer(productDto.Offer);



                product.Title = productDto.Title;
                product.Description = productDto.Description;
                product.ProductCategory = category;
                product.Price = productDto.Price;
                product.Offer = offer;
                product.Quantity = productDto.Quantity;
                product.Image = productDto.Image;




                bool isUpdated = dataAccess.EditProduct(product);
                if (isUpdated)
                {
                    return Ok("Product has been updated");
                }
                else
                {
                    return BadRequest("Something is wrong..!!");
                }
            }
            else
            {
                return BadRequest("Something is wrong..!!");
            }
        }

        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var result = dataAccess.GetAllUsers();
            return Ok(result);
        }

        [HttpPost("InsertUser")]
        public IActionResult AddUser([FromBody] User user)
        {
            var result = dataAccess.AddUser(user);
            if (result)
            {
                return Ok("User inserted successfully");
            }
            else
            {
                return BadRequest("Failed to insert user");
            }
        }

        [HttpPut("EditUser/{userId}")]
        public IActionResult EditUser(int userId, [FromBody] User user)
        {
            var result = dataAccess.EditUser(userId, user);
            if (result)
            {
                return Ok("User updated successfully");
            }
            else
            {
                return BadRequest("Failed to update user");
            }
        }

        [HttpDelete("DeleteUser/{userId}")]
        public IActionResult DeleteUser(int userId)
        {
            var result = dataAccess.DeleteUser(userId);
            if (result)
            {
                return Ok("User deleted successfully");
            }
            else
            {
                return NotFound("User not found or failed to delete");
            }
        }

        [HttpGet("GetPayment/{paymentId}")]
        public IActionResult GetPayment(int paymentId)
        {
            var payment = dataAccess.GetPayment(paymentId);
            if (payment != null)
            {
                return Ok(payment);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetPendingOrders")]
        public IActionResult GetPendingOrders()
        {
            var pendingOrders = dataAccess.GetPendingOrders();
            return Ok(pendingOrders);
        }

        [HttpPut("{orderId}/accept")]
        public IActionResult AcceptOrder(int orderId)
        {
            try
            {
                dataAccess.AcceptOrder(orderId);
                return Ok("Order Approved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{orderId}/reject")]
        public IActionResult RejectOrder(int orderId)
        {
            try
            {
                dataAccess.RejectOrder(orderId);
                return Ok("Order rejected successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("orders/thisweek")]
        public IActionResult GetOrdersThisWeek()
        {
            try
            {
                var orders = dataAccess.GetOrdersThisWeek();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("orders/lastmonth")]
        public IActionResult GetOrdersLastMonth()
        {
            try
            {
                var orders = dataAccess.GetOrdersLastMonth();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetPaymentMethod/{id}")]
        public IActionResult GetPaymentMethod(int id)
        {
            var paymentMethod = dataAccess.GetPaymentMethodById(id);

            if (paymentMethod == null)
            {
                return NotFound();
            }

            return Ok(paymentMethod);
        }

        [HttpGet("GetAllProductsFlat")]
        public IActionResult GetAllProductsFlat()
        {
            List<Product> products = dataAccess.GetAllProductsFlat();
            return Ok(products);
        }


        [HttpGet("GetAllContacts")]
        public IActionResult GetAllContacts()
        {
            var contacts = dataAccess.GetAllContacts();
            return Ok(contacts);
        }

        [HttpPost("InsertContact")]
        public IActionResult InsertContact([FromBody] Contact contact)
        {
            contact.SentAt = DateTime.Now;
            dataAccess.InsertContact(contact);
            return Ok("Inserted");
        }

        [HttpGet("GetContact/{userId}")]
        public IActionResult GetContactsByUserId(int userId)
        {
            var contact = dataAccess.GetContactByUserId(userId);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpPost]
        [Route("InsertProduct")]
        public IActionResult InsertProduct(Product product)
        {
            try
            {
                bool result = dataAccess.InsertProduct(product);
                if (result)
                {
                    return Ok(new { success = true, message = "Product inserted successfully" });
                }
                else
                {
                    return Ok(new { success = false, message = "Failed to insert product" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error inserting product: {ex.Message}");
            }
        }

        private bool IsImage(IFormFile file)
        {
            return file.ContentType.StartsWith("image/");
        }

        [HttpPost("UploadImage")]
        public IActionResult UploadImageProduct(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Invalid file");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                byte[] imageBytes = memoryStream.ToArray();

                return Ok(new { success = true, message = "Image uploaded successfully", imageBytes });
            }
        }


        [HttpPost]
        public IActionResult Add(int id, IFormFile fileimg)
        {
            string FileName = id.ToString() + "." + fileimg.FileName.Split('.').Last();
            using (FileStream f = new FileStream("wwwroot/products/" + FileName, FileMode.Create))
            {
                fileimg.CopyTo(f);
            }
            return Content("Image Uploaded!");
        }



        //[HttpDelete("RemoveCartItem/{cartItemId}")]
        //public IActionResult RemoveCartItem(int cartItemId)
        //{
        //    var result = dataAccess.RemoveCartItem(cartItemId);
        //    return Ok(result ? "removed" : "not removed");
        //}



        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromForm] ProductDto productdto)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    if(Request.Form.Files.Count > 0)
                    {
                        ResultFile result = await dataAccess.UploadFile(productdto.file, "products");
                        if (result.Successed)
                        {
                            productdto.Image = result.Url;

                        }
                        else
                        {
                            return BadRequest(result.ErrorMessage);
                        }

                    }

                }
                ProductCategory category = dataAccess.GetProductCategory(productdto.ProductCategory);
                Offer offer = dataAccess.GetOffer(productdto.Offer);
                Product product = new Product()
                {
                    Title = productdto.Title,
                    Description = productdto.Description,
                    ProductCategory = category,
                    Price = productdto.Price,
                    Offer = offer,
                    Quantity = productdto.Quantity,
                    Image = productdto.Image

                };

                bool insertResult = dataAccess.InsertProduct(product);
                if (!insertResult)
                {
                    return StatusCode(500, "Failed to insert product");
                }
                return Ok(new { success = true, message = "Product added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error adding product: {ex.Message}");
            }
        }
    }
    }













