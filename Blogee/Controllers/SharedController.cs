using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Blogee.Models;
using System.Data.SqlClient;
using System.Text;
using System.Web.Helpers;
using System.Data;

namespace Blogee.Controllers
{
    public class SharedController : Controller
    {
        [HttpPost]
        public ActionResult Search(SearchModel s)
        {
            string searchterm = s.SearchTerm;
            List<PostModel> listposts = new List<PostModel>(); // needs to be populated hereby

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
                builder.UserID = "serverusername";
                builder.Password = "ConnectDB1";
                builder.InitialCatalog = "Blogee_db";

                using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                {
                    string findPosts = "SELECT * FROM dbo.Posts WHERE Tags LIKE @Tag ";

                    using (SqlCommand queryFindPosts = new SqlCommand(findPosts))
                    {
                        queryFindPosts.Connection = openCon;
                        queryFindPosts.Parameters.AddWithValue("@Tag", "%" + searchterm + "%");
                        openCon.Open();
                        var reader = queryFindPosts.ExecuteReader();

                        // for some reason we don't enter into the while loop below
                        while (reader.Read())
                        {
                            listposts.Add(item: new PostModel
                            {
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Location = reader["Location"].ToString(),
                                Date = reader["Date"].ToString(),
                                Author = reader["Author"].ToString(),
                                Content = reader["Content"].ToString(),
                                Tags = reader["Tags"].ToString()
                            });
                        }
                    }

                    s.ListPosts = listposts;
                    openCon.Close();
                }


            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return View(s);
        }
    }
}