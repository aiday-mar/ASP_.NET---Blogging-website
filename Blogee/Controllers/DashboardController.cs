using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
// to add the extension from the controller, you must add the below mvc library
using System.Web.Mvc;
using Blogee.Models;
using System.Data.SqlClient;
using System.Text;
using System.Web.Helpers;
using System.Data;


namespace Blogee.Controllers
{
    public class DashboardController : Controller
    {

        string name = "";
        string username = "";
        string email = "";
        string hashedpassword = "";
        string password = "";
        bool isnewuser = false;
       
        public ActionResult Index()
        {
            // we found out that when the submit form is clicked then the program enters the Index method of the Dashboard Controller 
            return View();
        }

        // here we found out that when we write Dashboard/AddNewUser after localhost, then we access the AddNewUser method in the DashboardController
        // this means that all the cshtml files in the Dashboard folder of the View folder are related to some method in the action

        [HttpPost]
        public ActionResult Index(UserModel u)
        {
            // using the debug mode we find that this works
            name = u.Name;
            username = u.Username;
            email = u.Email;
            // using debug mode indeed we find that the below does output a string hashpassword
            password = u.Password;
            hashedpassword = Crypto.HashPassword(password);
            // the problem is that this boolean is still evaluated to false
            isnewuser = u.NewUser;

            ViewBag.User = u;
            
            // we ran the code below and found that this code really does store the name, username etc in the dbo.Users table
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
                builder.UserID = "serverusername";
                builder.Password = "ConnectDB1";
                builder.InitialCatalog = "Blogee_db";

                // string connectionString = "Server=tcp:sqlserver-aiday.database.windows.net,1433;Initial Catalog=Blogee_db;Persist Security Info=False;User ID=serverusername;Password=ConnectDB1;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
               
                if (isnewuser == true)
                {
                    using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                    {
                        string saveUser = "INSERT into dbo.Users (Username, Name, Email, Password) VALUES (@Username,@Name, @Email,@Password)";

                        using (SqlCommand querySaveUser = new SqlCommand(saveUser))
                        {
                            querySaveUser.Connection = openCon;
                            querySaveUser.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                            querySaveUser.Parameters.Add("@Name", SqlDbType.VarChar, 50).Value = name;
                            querySaveUser.Parameters.Add("@Email", SqlDbType.VarChar, 50).Value = email;
                            querySaveUser.Parameters.Add("@Password", SqlDbType.NVarChar, 250).Value = hashedpassword;
                            openCon.Open();
                            querySaveUser.ExecuteNonQuery();

                            // when the signup was done successfully you initialize the values of the static CurrentNewUser class

                            CurrentUserModel.CurrentUsername = username;
                            CurrentUserModel.CurrentName = name;
                            CurrentUserModel.CurrentEmail = email;
                            CurrentUserModel.CurrentPassword = password;
                            CurrentUserModel.CurrentLoggedIn = true;
                        }

                        openCon.Close();
                    }
                }

                // signup is working above, but the login below is not working, therefore I can work on this later
                // and make sure the user really is authenticated when the credentials are correct. So far even if the credentials are not correct
                // the user enters

                if (isnewuser == false)
                {
                    using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                    {
                        string verifyUserName = "SELECT Username FROM dbo.Users WHERE Username = @Username";
                        using (SqlCommand queryVerifyUserName = new SqlCommand(verifyUserName))
                        {
                            queryVerifyUserName.Connection = openCon;
                            queryVerifyUserName.Parameters.Add("@Username", SqlDbType.VarChar, 50).Value = username;
                            //queryVerifyUserName.Parameters.Add("@Password", SqlDbType.NVarChar, 250).Value = hashedpassword;
                            openCon.Open();

                            var queryResultOne = queryVerifyUserName.ExecuteScalar();

                            if (queryResultOne != null)
                            {
                                string currentUserName = queryResultOne.ToString();
                                string verifyPassWord = "SELECT Password FROM dbo.Users WHERE Username = @CurrentUsername";
                                using (SqlCommand queryVerifyPassWord = new SqlCommand(verifyPassWord))
                                {
                                    queryVerifyPassWord.Connection = openCon;
                                    queryVerifyPassWord.Parameters.Add("@CurrentUsername", SqlDbType.VarChar, 50).Value = currentUserName;

                                    var queryResultTwo = queryVerifyPassWord.ExecuteScalar();

                                    if(queryResultTwo != null)
                                    {
                                        string currentPassWord = queryResultTwo.ToString();
                                        bool isSame = Crypto.VerifyHashedPassword(currentPassWord, password);
                                        
                                        if (isSame == false)
                                        {
                                            RedirectToAction("Login", "Home");
                                        }

                                    } else
                                    {
                                        RedirectToAction("Login", "Home");
                                    }

                                }

                            } else
                            {
                                RedirectToAction("Login", "Home");
                            }

                            openCon.Close();
                        }
                        
                    }

                }

            }
            // to catch the exeption I had to put a breakpoint next to the exception to make it appear
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            CurrentUserModel.CurrentUsername = username;
            CurrentUserModel.CurrentName = name;
            CurrentUserModel.CurrentEmail = email;
            CurrentUserModel.CurrentPassword = password;
            CurrentUserModel.CurrentLoggedIn = true;

            
            return View();
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult SavePost(PostModel p)
        {
            // this is the case when we post a new post and are redirected back to our dashboard
            // first check the data actually gets here

            string title = p.Title;
            string description = p.Description;
            string location = p.Location;
            string tags = p.Tags;
            string content = p.Content;
            string author = CurrentUserModel.CurrentUsername;
            string date = DateTime.Now.ToString("dd/MM/yyyy");
            string postID = CurrentUserModel.CurrentUsername + "_" + p.Title;

            // this Viewbag shows us that the content of the new Post is indeed posted correctly, now we need to save it in a database
            ViewBag.Post = p;

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
                builder.UserID = "serverusername";
                builder.Password = "ConnectDB1";
                builder.InitialCatalog = "Blogee_db";
                
                using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                {
                    string saveUser = "INSERT into dbo.Posts (PostID, Title, Description, Location, Date, Author, Content, Tags) VALUES (@PostID,@Title, @Description,@Location, @Date, @Author, @Content, @Tags)";

                    using (SqlCommand querySaveUser = new SqlCommand(saveUser))
                    {
                        querySaveUser.Connection = openCon;
                        querySaveUser.Parameters.Add("@PostID", SqlDbType.VarChar, 100).Value = postID;
                        querySaveUser.Parameters.Add("@Title", SqlDbType.VarChar, 50).Value = title;
                        querySaveUser.Parameters.Add("@Description", SqlDbType.VarChar, 250).Value = description;
                        querySaveUser.Parameters.Add("@Location", SqlDbType.VarChar, 50).Value = location;
                        querySaveUser.Parameters.Add("@Date", SqlDbType.VarChar, 50).Value = date;
                        querySaveUser.Parameters.Add("@Author", SqlDbType.VarChar, 50).Value = author;
                        querySaveUser.Parameters.Add("@Tags", SqlDbType.VarChar, 100).Value = tags;
                        querySaveUser.Parameters.Add("@Content", SqlDbType.NVarChar, 1000).Value = content;

                        openCon.Open();
                        querySaveUser.ExecuteNonQuery();
                        openCon.Close();

                       }
                }

            }
            // to catch the exeption I had to put a breakpoint next to the exception to make it appear
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return View("~/Views/Dashboard/Index.cshtml");
        }

        public ActionResult NewPost() { return View(); }
        
        public ActionResult LogOut() {

            // upon the logout the static class is initialiazed back with null values

            CurrentUserModel.CurrentEmail = "";
            CurrentUserModel.CurrentName = "";
            CurrentUserModel.CurrentPassword = "";
            CurrentUserModel.CurrentLoggedIn = false;
            CurrentUserModel.CurrentUsername = "";

            return RedirectToAction("Index", "Home"); }
        
    }
}