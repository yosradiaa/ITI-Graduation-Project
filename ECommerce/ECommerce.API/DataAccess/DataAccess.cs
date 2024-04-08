using ECommerce.API.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace ECommerce.API.DataAccess
{
    public class DataAccess : IDataAccess
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly string dbconnection;

        public DataAccess(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            this.configuration = configuration;
            this.webHostEnvironment = webHostEnvironment;
            dbconnection = this.configuration["ConnectionStrings:DB"];

        }

        public Cart GetActiveCartOfUser(int userid)
        {
            var cart = new Cart();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT COUNT(*) From Carts WHERE UserId=" + userid + " AND Ordered='false';";
                command.CommandText = query;

                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    return cart;
                }

                query = "SELECT CartId From Carts WHERE UserId=" + userid + " AND Ordered='false';";
                command.CommandText = query;

                int cartid = (int)command.ExecuteScalar();

                query = "select * from CartItems where CartId=" + cartid + ";";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CartItem item = new()
                    {
                        Id = (int)reader["CartItemId"],
                        Product = GetProduct((int)reader["ProductId"])
                    };
                    cart.CartItems.Add(item);
                }

                cart.Id = cartid;
                cart.User = GetUser(userid);
                cart.Ordered = false;
                cart.OrderedOn = "";
            }
            return cart;
        }

        public List<Cart> GetAllPreviousCartsOfUser(int userid)
        {
            var carts = new List<Cart>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                string query = "SELECT CartId FROM Carts WHERE UserId=" + userid + " AND Ordered='true';";
                command.CommandText = query;
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var cartid = (int)reader["CartId"];
                    carts.Add(GetCart(cartid));
                }
            }
            return carts;
        }

        public Cart GetCart(int cartid)
        {
            var cart = new Cart();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT * FROM CartItems WHERE CartId=" + cartid + ";";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    CartItem item = new()
                    {
                        Id = (int)reader["CartItemId"],
                        Product = GetProduct((int)reader["ProductId"])
                    };
                    cart.CartItems.Add(item);
                }
                reader.Close();

                query = "SELECT * FROM Carts WHERE CartId=" + cartid + ";";
                command.CommandText = query;
                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    cart.Id = cartid;
                    cart.User = GetUser((int)reader["UserId"]);
                    cart.Ordered = bool.Parse((string)reader["Ordered"]);
                    cart.OrderedOn = (string)reader["OrderedOn"];
                }
                reader.Close();
            }
            return cart;
        }

        public Offer GetOffer(int id)
        {
            var offer = new Offer();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Offers WHERE OfferId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                while (r.Read())
                {
                    offer.Id = (int)r["OfferId"];
                    offer.Title = (string)r["Title"];
                    offer.Discount = (int)r["Discount"];
                }
            }
            return offer;
        }

        public List<PaymentMethod> GetPaymentMethods()
        {
            var result = new List<PaymentMethod>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM PaymentMethods;";
                command.CommandText = query;

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    PaymentMethod paymentMethod = new()
                    {
                        Id = (int)reader["PaymentMethodId"],
                        Type = (string)reader["Type"],
                        Provider = (string)reader["Provider"],
                        Available = bool.Parse((string)reader["Available"]),
                        Reason = (string)reader["Reason"]
                    };
                    result.Add(paymentMethod);
                }
            }
            return result;
        }

        public Product GetProduct(int id)
        {
            var product = new Product();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Products WHERE ProductId = @ProductId", connection);
                command.Parameters.AddWithValue("@ProductId", id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    product.Id = (int)reader["ProductId"];
                    product.Title = (string)reader["Title"];
                    product.Description = (string)reader["Description"];
                    product.Price = (double)reader["Price"];
                    product.Quantity = (int)reader["Quantity"];
                    product.Image = (reader["Image"] == DBNull.Value) ? string.Empty : reader["Image"].ToString();
                    var categoryId = (int)reader["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryId);

                    var offerId = (int)reader["OfferId"];
                    product.Offer = GetOffer(offerId);
                }
            }
            return product;
        }

        public List<ProductCategory> GetProductCategories()
        {
            var productCategories = new List<ProductCategory>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                string query = "SELECT * FROM ProductCategories;";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var category = new ProductCategory()
                    {
                        Id = (int)reader["CategoryId"],
                        Category = (string)reader["Category"],
                        SubCategory = (string)reader["SubCategory"]
                    };
                    productCategories.Add(category);
                }
            }
            return productCategories;
        }

        public ProductCategory GetProductCategory(int id)
        {
            var productCategory = new ProductCategory();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM ProductCategories WHERE CategoryId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader r = command.ExecuteReader();
                while (r.Read())
                {
                    productCategory.Id = (int)r["CategoryId"];
                    productCategory.Category = (string)r["Category"];
                    productCategory.SubCategory = (string)r["SubCategory"];
                }
            }

            return productCategory;
        }

        public List<Review> GetProductReviews(int productId)
        {
            var reviews = new List<Review>();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Reviews WHERE ProductId=" + productId + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var review = new Review()
                    {
                        Id = (int)reader["ReviewId"],
                        Value = (string)reader["Review"],
                        CreatedAt = (string)reader["CreatedAt"]
                    };

                    var userid = (int)reader["UserId"];
                    review.User = GetUser(userid);

                    var productid = (int)reader["ProductId"];
                    review.Product = GetProduct(productid);

                    reviews.Add(review);
                }
            }
            return reviews;
        }

        public List<Product> GetProducts(string category, string subcategory, int count)
        {
            var products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand()
                {
                    Connection = connection
                };

                string query = "SELECT TOP " + count + " * FROM Products WHERE CategoryId=(SELECT CategoryId FROM ProductCategories WHERE Category=@c AND SubCategory=@s) ORDER BY newid();";
                command.CommandText = query;
                command.Parameters.Add("@c", SqlDbType.NVarChar).Value = category;
                command.Parameters.Add("@s", SqlDbType.NVarChar).Value = subcategory;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var product = new Product()
                    {
                        Id = (int)reader["ProductId"],
                        Title = (string)reader["Title"],
                        Description = (string)reader["Description"],
                        Price = (double)reader["Price"],
                        Quantity = (int)reader["Quantity"],
                        Image = (reader["Image"] == DBNull.Value) ? string.Empty : reader["Image"].ToString()
                    };

                    // Retrieve image data as byte array


                    var categoryId = (int)reader["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryId);

                    var offerId = (int)reader["OfferId"];
                    product.Offer = GetOffer(offerId);

                    products.Add(product);
                }
            }
            return products;
        }


        public List<Offer> GetOffers()
        {
            var offers = new List<Offer>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Offers;", connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var offer = new Offer
                    {
                        Id = (int)reader["OfferId"],
                        Title = (string)reader["Title"],
                        Discount = (int)reader["Discount"]
                    };
                    offers.Add(offer);
                }
            }
            return offers;
        }


        public User GetUser(int id)
        {
            var user = new User();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "SELECT * FROM Users WHERE UserId=" + id + ";";
                command.CommandText = query;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user.Id = (int)reader["UserId"];
                    user.FirstName = (string)reader["FirstName"];
                    user.LastName = (string)reader["LastName"];
                    user.Email = (string)reader["Email"];
                    user.Address = (string)reader["Address"];
                    user.Mobile = (string)reader["Mobile"];
                    user.Password = (string)reader["Password"];
                    user.CreatedAt = (string)reader["CreatedAt"];
                    user.ModifiedAt = (string)reader["ModifiedAt"];
                }
            }
            return user;
        }

        public bool InsertCartItem(int userId, int productId)
        {
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                connection.Open();
                string query = "SELECT COUNT(*) FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
                command.CommandText = query;
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    query = "INSERT INTO Carts (UserId, Ordered, OrderedOn) VALUES (" + userId + ", 'false', '');";
                    command.CommandText = query;
                    command.ExecuteNonQuery();
                }

                query = "SELECT CartId FROM Carts WHERE UserId=" + userId + " AND Ordered='false';";
                command.CommandText = query;
                int cartId = (int)command.ExecuteScalar();


                query = "INSERT INTO CartItems (CartId, ProductId) VALUES (" + cartId + ", " + productId + ");";
                command.CommandText = query;
                command.ExecuteNonQuery();
                return true;
            }
        }


        //public bool RemoveCartItem(int cartItemId)
        //{
        //    using (SqlConnection connection = new SqlConnection(dbconnection))
        //    {
        //        SqlCommand command = new SqlCommand
        //        {
        //            Connection = connection
        //        };

        //        connection.Open();
        //        string query = "DELETE FROM CartItems WHERE CartItemId = @CartItemId;";
        //        command.CommandText = query;
        //        command.Parameters.AddWithValue("@CartItemId", cartItemId);

        //        int rowsAffected = command.ExecuteNonQuery();

        //        return rowsAffected > 0;
        //    }
        //}



        public int InsertOrder(Order order)
        {
            int value = 0;
            string dateformat = DateTime.Now.ToString();

            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = "INSERT INTO Orders (UserId, CartId, PaymentId, CreatedAt) values (@uid, @cid, @pid, @cat);";

                command.CommandText = query;
                command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = order.User.Id;
                command.Parameters.Add("@cid", System.Data.SqlDbType.Int).Value = order.Cart.Id;
                command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = dateformat;
                command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = order.Payment.Id;

                connection.Open();
                value = command.ExecuteNonQuery();

                if (value > 0)
                {
                    query = "UPDATE Carts SET Ordered='true', OrderedOn='" + DateTime.Now.ToString(dateformat) + "' WHERE CartId=" + order.Cart.Id + ";";
                    command.CommandText = query;
                    command.ExecuteNonQuery();

                    query = "SELECT TOP 1 Id FROM Orders ORDER BY Id DESC;";
                    command.CommandText = query;
                    value = (int)command.ExecuteScalar();
                }
                else
                {
                    value = 0;
                }
            }

            return value;
        }
        public int InsertPayment(Payment payment)
        {
            int value = 0;
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                string query = @"INSERT INTO Payments (PaymentMethodId, UserId, TotalAmount, ShippingCharges, AmountReduced, AmountPaid, CreatedAt) 
                                VALUES (@pmid, @uid, @ta, @sc, @ar, @ap, @cat);";

                command.CommandText = query;
                command.Parameters.Add("@pmid", System.Data.SqlDbType.Int).Value = payment.PaymentMethod.Id;
                command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = payment.User.Id;
                command.Parameters.Add("@ta", System.Data.SqlDbType.NVarChar).Value = payment.TotalAmount;
                command.Parameters.Add("@sc", System.Data.SqlDbType.NVarChar).Value = payment.ShipingCharges;
                command.Parameters.Add("@ar", System.Data.SqlDbType.NVarChar).Value = payment.AmountReduced;
                command.Parameters.Add("@ap", System.Data.SqlDbType.NVarChar).Value = payment.AmountPaid;
                command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = payment.CreatedAt;

                connection.Open();
                value = command.ExecuteNonQuery();

                if (value > 0)
                {
                    query = "SELECT TOP 1 Id FROM Payments ORDER BY Id DESC;";
                    command.CommandText = query;
                    value = (int)command.ExecuteScalar();
                }
                else
                {
                    value = 0;
                }
            }
            return value;
        }
        public void InsertReview(Review review)
        {
            using SqlConnection connection = new(dbconnection);
            SqlCommand command = new()
            {
                Connection = connection
            };

            string query = "INSERT INTO Reviews (UserId, ProductId, Review, CreatedAt) VALUES (@uid, @pid, @rv, @cat);";
            command.CommandText = query;
            command.Parameters.Add("@uid", System.Data.SqlDbType.Int).Value = review.User.Id;
            command.Parameters.Add("@pid", System.Data.SqlDbType.Int).Value = review.Product.Id;
            command.Parameters.Add("@rv", System.Data.SqlDbType.NVarChar).Value = review.Value;
            command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = review.CreatedAt;

            connection.Open();
            command.ExecuteNonQuery();
        }
        public bool InsertUser(User user)
        {
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };
                connection.Open();

                string query = "SELECT COUNT(*) FROM Users WHERE Email='" + user.Email + "';";
                command.CommandText = query;
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    connection.Close();
                    return false;
                }

                query = "INSERT INTO Users (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) values (@fn, @ln, @add, @mb, @em, @pwd, @cat, @mat);";

                command.CommandText = query;
                command.Parameters.Add("@fn", System.Data.SqlDbType.NVarChar).Value = user.FirstName;
                command.Parameters.Add("@ln", System.Data.SqlDbType.NVarChar).Value = user.LastName;
                command.Parameters.Add("@add", System.Data.SqlDbType.NVarChar).Value = user.Address;
                command.Parameters.Add("@mb", System.Data.SqlDbType.NVarChar).Value = user.Mobile;
                command.Parameters.Add("@em", System.Data.SqlDbType.NVarChar).Value = user.Email;
                command.Parameters.Add("@pwd", System.Data.SqlDbType.NVarChar).Value = user.Password;
                command.Parameters.Add("@cat", System.Data.SqlDbType.NVarChar).Value = user.CreatedAt;
                command.Parameters.Add("@mat", System.Data.SqlDbType.NVarChar).Value = user.ModifiedAt;

                command.ExecuteNonQuery();
            }
            return true;
        }
        public bool EditProduct(Product product)
        {

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                string query = @"
            UPDATE Products 
            SET Title = @Title, 
                Description = @Description, 
                CategoryId = @CategoryId, 
                OfferId = @OfferId, 
                Price = @Price, 
                Quantity = @Quantity, 
                Image = @Image 
            WHERE ProductId = @ProductId;";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", product.Title);
                command.Parameters.AddWithValue("@Description", product.Description);
                command.Parameters.AddWithValue("@CategoryId", product.ProductCategory.Id);
                command.Parameters.AddWithValue("@OfferId", product.Offer.Id);
                command.Parameters.AddWithValue("@Price", product.Price);
                command.Parameters.AddWithValue("@Quantity", product.Quantity);
                command.Parameters.AddWithValue("@ProductId", product.Id);
                command.Parameters.AddWithValue("@Image", product.Image);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }
        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Products;", connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var product = new Product
                    {
                        Id = (int)reader["ProductId"],
                        Title = (string)reader["Title"],
                        Description = (string)reader["Description"],
                        Price = (double)reader["Price"],
                        Quantity = (int)reader["Quantity"],
                        Image = (reader["Image"] == DBNull.Value) ? string.Empty : reader["Image"].ToString()
                    };



                    var categoryId = (int)reader["CategoryId"];
                    product.ProductCategory = GetProductCategory(categoryId);

                    var offerId = (int)reader["OfferId"];
                    product.Offer = GetOffer(offerId);

                    products.Add(product);
                }
            }
            return products;
        }

        public bool DeleteProduct(int productId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("DELETE FROM Products WHERE ProductId = @ProductId;", connection);
                command.Parameters.AddWithValue("@ProductId", productId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }


        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Users;", connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var user = new User
                    {
                        Id = (int)reader["UserId"],
                        FirstName = (string)reader["FirstName"],
                        LastName = (string)reader["LastName"],
                        Email = (string)reader["Email"],
                        Address = (string)reader["Address"],
                        Mobile = (string)reader["Mobile"],
                        Password = (string)reader["Password"],
                        CreatedAt = (string)reader["CreatedAt"],
                        ModifiedAt = (string)reader["ModifiedAt"]
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        public bool AddUser(User user)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand(@"INSERT INTO Users (FirstName, LastName, Address, Mobile, Email, Password, CreatedAt, ModifiedAt) 
                                              VALUES (@FirstName, @LastName, @Address, @Mobile, @Email, @Password, @CreatedAt, @ModifiedAt);", connection);

                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Address", user.Address);
                command.Parameters.AddWithValue("@Mobile", user.Mobile);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
                command.Parameters.AddWithValue("@ModifiedAt", user.ModifiedAt);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        public bool EditUser(int userId, User user)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand(@"UPDATE Users 
                                               SET FirstName = @FirstName, 
                                                   LastName = @LastName, 
                                                   Address = @Address, 
                                                   Mobile = @Mobile, 
                                                   Email = @Email, 
                                                   Password = @Password, 
                                                   ModifiedAt = @ModifiedAt 
                                               WHERE UserId = @UserId;", connection);

                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Address", user.Address);
                command.Parameters.AddWithValue("@Mobile", user.Mobile);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.Parameters.AddWithValue("@ModifiedAt", user.ModifiedAt);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        public bool DeleteUser(int userId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("DELETE FROM Users WHERE UserId = @UserId;", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }

        public string IsUserPresent(string email, string password)
        {
            User user = new();
            using (SqlConnection connection = new(dbconnection))
            {
                SqlCommand command = new()
                {
                    Connection = connection
                };

                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
                command.CommandText = query;
                int count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    connection.Close();
                    return "";
                }

                query = "SELECT * FROM Users WHERE Email='" + email + "' AND Password='" + password + "';";
                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    user.Id = (int)reader["UserId"];
                    user.FirstName = (string)reader["FirstName"];
                    user.LastName = (string)reader["LastName"];
                    user.Email = (string)reader["Email"];
                    user.Address = (string)reader["Address"];
                    user.Mobile = (string)reader["Mobile"];
                    user.Password = (string)reader["Password"];
                    user.CreatedAt = (string)reader["CreatedAt"];
                    user.ModifiedAt = (string)reader["ModifiedAt"];
                }

                string key = "MNU66iBl3T5rh6H52i69";
                string duration = "60";
                var symmetrickey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(symmetrickey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("firstName", user.FirstName),
                    new Claim("lastName", user.LastName),
                    new Claim("address", user.Address),
                    new Claim("mobile", user.Mobile),
                    new Claim("email", user.Email),
                    new Claim("createdAt", user.CreatedAt),
                    new Claim("modifiedAt", user.ModifiedAt)
                };

                var jwtToken = new JwtSecurityToken(
                    issuer: "localhost",
                    audience: "localhost",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Int32.Parse(duration)),
                    signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }
            return "";
        }

        public Order GetOrder(int orderId)
        {
            Order order = null;
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Orders WHERE Id = @OrderId", connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    order = new Order
                    {
                        Id = (int)reader["Id"],
                        User = GetUser((int)reader["UserId"]),
                        Cart = GetCart((int)reader["CartId"]),
                        Payment = GetPayment((int)reader["PaymentId"]),
                        CreatedAt = (string)reader["CreatedAt"],
                        Status = (string)reader["Status"]
                    };
                }
            }
            return order;
        }

        public List<Order> GetAllOrders()
        {
            List<Order> orders = new List<Order>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Orders", connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Order order = new Order
                    {
                        Id = (int)reader["Id"],
                        User = GetUser((int)reader["UserId"]),
                        Cart = GetCart((int)reader["CartId"]),
                        Payment = GetPayment((int)reader["PaymentId"]),
                        CreatedAt = (string)reader["CreatedAt"],
                        Status = (string)reader["Status"]
                    };
                    orders.Add(order);
                }
            }
            return orders;
        }

        public List<Order> GetPendingOrders()
        {
            List<Order> pendingOrders = new List<Order>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Orders WHERE Status = 'Pending'", connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Order order = new Order
                    {
                        Id = (int)reader["Id"],
                        User = GetUser((int)reader["UserId"]),
                        Cart = GetCart((int)reader["CartId"]),
                        Payment = GetPayment((int)reader["PaymentId"]),
                        CreatedAt = (string)reader["CreatedAt"],
                        Status = (string)reader["Status"]
                    };
                    pendingOrders.Add(order);
                }
            }
            return pendingOrders;
        }

        public void AcceptOrder(int orderId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("UPDATE Orders SET Status = 'Approved' WHERE Id = @OrderId", connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public void RejectOrder(int orderId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("UPDATE Orders SET Status = 'Rejected' WHERE Id = @OrderId", connection);
                command.Parameters.AddWithValue("@OrderId", orderId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public Payment GetPayment(int paymentId)
        {
            Payment payment = null;

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Payments WHERE Id = @PaymentId", connection);
                command.Parameters.AddWithValue("@PaymentId", paymentId);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    payment = new Payment
                    {
                        Id = (int)reader["Id"],
                        TotalAmount = (int)reader["TotalAmount"],
                        ShipingCharges = (int)reader["ShippingCharges"],
                        AmountReduced = (int)reader["AmountReduced"],
                        AmountPaid = (int)reader["AmountPaid"],
                        CreatedAt = (string)reader["CreatedAt"]
                    };

                    int userId = (int)reader["UserId"];
                    User user = GetUser(userId);
                    payment.User = user;

                    int paymentMethodId = (int)reader["PaymentMethodId"];
                    PaymentMethod paymentMethod = GetPaymentMethodById(paymentMethodId);
                    payment.PaymentMethod = paymentMethod;
                }
            }

            return payment;
        }
        public PaymentMethod GetPaymentMethodById(int id)
        {
            PaymentMethod paymentMethod = null;

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM PaymentMethods WHERE PaymentMethodId = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    paymentMethod = new PaymentMethod
                    {
                        Id = (int)reader["PaymentMethodId"],
                        Type = (string)reader["Type"],
                        Provider = (string)reader["Provider"],
                        Available = Convert.ToBoolean(reader["Available"]), // Convert to boolean
                        Reason = (string)reader["Reason"]
                    };
                }
            }

            return paymentMethod;
        }


        public List<Order> GetOrdersThisWeek()
        {
            var ordersThisWeek = new List<Order>();

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = connection.CreateCommand();
                connection.Open();

                DateTime currentDate = DateTime.Now.Date;
                DateTime firstDayOfWeek = currentDate.AddDays(-(int)currentDate.DayOfWeek);
                DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);

                string query = "SELECT * FROM Orders WHERE CAST(CreatedAt AS DATE) BETWEEN @FirstDayOfWeek AND @LastDayOfWeek";

                command.Parameters.AddWithValue("@FirstDayOfWeek", firstDayOfWeek);
                command.Parameters.AddWithValue("@LastDayOfWeek", lastDayOfWeek);

                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Order order = new Order
                    {
                        Id = (int)reader["Id"],
                    };

                    ordersThisWeek.Add(order);
                }
            }

            return ordersThisWeek;
        }

        public List<Order> GetOrdersLastMonth()
        {
            var ordersLastMonth = new List<Order>();

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = connection.CreateCommand();
                connection.Open();

                DateTime currentDate = DateTime.Now.Date;
                DateTime firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                string query = "SELECT * FROM Orders WHERE CAST(CreatedAt AS DATE) BETWEEN @FirstDayOfMonth AND @LastDayOfMonth";

                command.Parameters.AddWithValue("@FirstDayOfMonth", firstDayOfMonth);
                command.Parameters.AddWithValue("@LastDayOfMonth", lastDayOfMonth);

                command.CommandText = query;

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Order order = new Order
                    {
                        Id = (int)reader["Id"],
                    };

                    ordersLastMonth.Add(order);
                }
            }

            return ordersLastMonth;
        }
        public string GetProductImage(int id)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT ImageName FROM Products WHERE ProductId = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                connection.Open();
                object ImageName = command.ExecuteScalar();
                if (ImageName != DBNull.Value)
                {
                    return (string)ImageName;
                }
                else
                {
                    return null;
                }
            }
        }


        public List<Product> GetAllProductsFlat()
        {
            var products = new List<Product>();
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("SELECT * FROM Products;", connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var product = new Product
                    {
                        Id = (int)reader["ProductId"],
                        Title = (string)reader["Title"],
                        Description = (string)reader["Description"],
                        Price = (double)reader["Price"],
                        Quantity = (int)reader["Quantity"],
                        Image = (reader["Image"] == DBNull.Value) ? string.Empty : reader["Image"].ToString()
                    };

                    products.Add(product);
                }
            }
            return products;
        }

        public void SaveImageData(int productId, byte[] ImageName)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand("UPDATE Products SET ImageName = @ImageName WHERE ProductId = @ProductId", connection);
                command.Parameters.AddWithValue("@ImageName", ImageName);
                command.Parameters.AddWithValue("@ProductId", productId);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }


        public IEnumerable<Contact> GetAllContacts()
        {
            List<Contact> contacts = new List<Contact>();

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                string query = "SELECT * FROM Contact";
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Contact contact = new Contact
                    {
                        ContactId = Convert.ToInt32(reader["ContactId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Message = Convert.ToString(reader["Message"]),
                        SentAt = Convert.ToDateTime(reader["SentAt"])
                    };

                    contact.User = GetUser(contact.UserId);

                    contacts.Add(contact);
                }
            }

            return contacts;
        }


        public async Task<int> InsertContact(Contact contact)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                string query = "INSERT INTO Contact (UserId, Message, SentAt) VALUES (@UserId, @Message, @SentAt); SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", contact.UserId);
                command.Parameters.AddWithValue("@Message", contact.Message);
                command.Parameters.AddWithValue("@SentAt", contact.SentAt);

                await connection.OpenAsync();
                var result = await command.ExecuteScalarAsync();

                return result != null ? int.Parse(result.ToString()) : 0;
            }
        }




        public Contact GetContactByUserId(int userId)
        {
            Contact contact = null;

            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                string query = @"
                SELECT c.ContactId, c.UserId, c.Message, c.SentAt,
                u.FirstName AS UserFirstName, u.LastName AS UserLastName,
                u.Email AS UserEmail, u.Mobile AS UserMobile
                FROM Contact c
                INNER JOIN Users u ON c.UserId = u.UserId
                WHERE c.UserId = @UserId";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    contact = new Contact
                    {
                        ContactId = Convert.ToInt32(reader["ContactId"]),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Message = reader["Message"].ToString(),
                        SentAt = Convert.ToDateTime(reader["SentAt"]),
                        User = new User
                        {
                            FirstName = reader["UserFirstName"].ToString(),
                            LastName = reader["UserLastName"].ToString(),
                            Email = reader["UserEmail"].ToString(),
                            Mobile = reader["UserMobile"].ToString()
                        }
                    };
                }
            }

            return contact;
        }


        public bool InsertProduct(Product product)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;

                    if (string.IsNullOrEmpty(product.Image))
                    {
                        command.CommandText = @"INSERT INTO Products (Title, Description, CategoryId, OfferId, Price, Quantity) 
                                        VALUES (@Title, @Description, @CategoryId, @OfferId, @Price, @Quantity);
                                        SELECT SCOPE_IDENTITY();";

                        command.Parameters.AddWithValue("@Title", product.Title);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@CategoryId", product.ProductCategory.Id);
                        command.Parameters.AddWithValue("@OfferId", product.Offer.Id);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Quantity", product.Quantity);
                    }
                    else
                    {
                        command.CommandText = @"INSERT INTO Products (Title, Description, CategoryId, OfferId, Price, Quantity, Image) 
                                        VALUES (@Title, @Description, @CategoryId, @OfferId, @Price, @Quantity, @Image);
                                        SELECT SCOPE_IDENTITY();";

                        command.Parameters.AddWithValue("@Title", product.Title);
                        command.Parameters.AddWithValue("@Description", product.Description);
                        command.Parameters.AddWithValue("@CategoryId", product.ProductCategory.Id);
                        command.Parameters.AddWithValue("@OfferId", product.Offer.Id);
                        command.Parameters.AddWithValue("@Price", product.Price);
                        command.Parameters.AddWithValue("@Quantity", product.Quantity);
                        command.Parameters.AddWithValue("@Image", product.Image);
                    }

                    int? productId = Convert.ToInt32(command.ExecuteScalar());
                    if (productId.HasValue)
                    {
                        product.Id = productId;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting product: {ex.Message}");
                return false;
            }
        }

        public bool UploadImage(int productId, byte[] imageBytes)
        {
            throw new NotImplementedException();
        }
        public int GetProductId(Product product)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(dbconnection))
                {
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    command.CommandType = CommandType.Text;
                    command.CommandText = "SELECT ProductId FROM Products WHERE Title = @Title AND Description = @Description";
                    command.Parameters.AddWithValue("@Title", product.Title);
                    command.Parameters.AddWithValue("@Description", product.Description);
                    object result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    else
                    {
                        throw new InvalidOperationException("ProductId not found");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting ProductId: {ex.Message}");
                throw;
            }
        }



        public IEnumerable<string> AllowedExtensions { get; set; } = new List<string>() { ".jpg", ".png", ".jpeg" };

        public int MaxFileSize { get; set; } = 2097152;

        public async Task<ResultFile> UploadFile(IFormFile formFile, string folderName)
        {

            var extension = Path.GetExtension(formFile.FileName);
            if (AllowedExtensions.Contains(extension))
            {

                if (formFile.Length < MaxFileSize)
                {
                    string imageName = $"{Guid.NewGuid()}-{formFile.FileName}";
                    string path = Path.Combine($"{webHostEnvironment.WebRootPath}/images/{folderName}", imageName);
                    using var stream = File.Create(path);
                    await formFile.CopyToAsync(stream);
                    stream.Dispose();


                    return new ResultFile() { Successed = true, Url = $"/images/{folderName}/{imageName}" };

                }
                else
                {
                    return new ResultFile() { Successed = false, ErrorMessage = "Error, image must be less than 2 mb" };
                }
            }
            else
            {
                return new ResultFile() { Successed = false, ErrorMessage = "Error, allowed extensions is jpg , jpeg , png" };
            }
        }



        public bool RemoveCartItem(int cartItemId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                SqlCommand command = new SqlCommand
                {
                    Connection = connection
                };

                connection.Open();
                string query = "DELETE FROM CartItems WHERE CartItemId = @CartItemId;";
                command.CommandText = query;
                command.Parameters.AddWithValue("@CartItemId", cartItemId);

                int rowsAffected = command.ExecuteNonQuery();

                return rowsAffected > 0;
            }
        }


        public void RemoveFile(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                string _imagePath = $"{webHostEnvironment.WebRootPath}{imageUrl}";

                if (File.Exists(_imagePath))
                {
                    System.IO.File.Delete(_imagePath);
                }


            }
        }

        public int GetProductsCount()
        {
            return GetCount("SELECT COUNT(*) FROM Products");
        }

        public int GetUsersCount()
        {
            return GetCount("SELECT COUNT(*) FROM Users");
        }

        public int GetPendingOrdersCount()
        {
            return GetCount("SELECT COUNT(*) FROM Orders WHERE Status = 'Pending'");
        }

        public int GetContactsCount()
        {
            return GetCount("SELECT COUNT(*) FROM Contact");
        }

        private int GetCount(string query)
        {
            using (var connection = new SqlConnection(dbconnection))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    return (int)command.ExecuteScalar();
                }
            }
        }

        public void DeleteUserAndRelatedData(int userId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {

                        SqlCommand deleteContactsCommand = new SqlCommand("DELETE FROM Contact WHERE UserId = @UserId", connection, transaction);
                        deleteContactsCommand.Parameters.AddWithValue("@UserId", userId);
                        deleteContactsCommand.ExecuteNonQuery();

                        SqlCommand deleteOrdersCommand = new SqlCommand("DELETE FROM Orders WHERE UserId = @UserId", connection, transaction);
                        deleteOrdersCommand.Parameters.AddWithValue("@UserId", userId);
                        deleteOrdersCommand.ExecuteNonQuery();

                        SqlCommand deleteReviewsCommand = new SqlCommand("DELETE FROM Reviews WHERE UserId = @UserId", connection, transaction);
                        deleteReviewsCommand.Parameters.AddWithValue("@UserId", userId);
                        deleteReviewsCommand.ExecuteNonQuery();

                        SqlCommand deletePaymentsCommand = new SqlCommand("DELETE FROM Payments WHERE UserId = @UserId", connection, transaction);
                        deletePaymentsCommand.Parameters.AddWithValue("@UserId", userId);
                        deletePaymentsCommand.ExecuteNonQuery();


                        SqlCommand deleteUserCommand = new SqlCommand("DELETE FROM Users WHERE UserId = @UserId", connection, transaction);
                        deleteUserCommand.Parameters.AddWithValue("@UserId", userId);
                        deleteUserCommand.ExecuteNonQuery();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }
        }


        public bool DeleteContact(int contactId)
        {
            using (SqlConnection connection = new SqlConnection(dbconnection))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        SqlCommand deleteContactsCommand = new SqlCommand("DELETE FROM Contact WHERE ContactId = @ContactId", connection, transaction);
                        deleteContactsCommand.Parameters.AddWithValue("@ContactId", contactId);
                        deleteContactsCommand.ExecuteNonQuery();

                        transaction.Commit();
                        return true;
                    
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine($"Error deleting contact: {ex.Message}");
                        return false;
                    }

                }
            }
        }
    }
}
