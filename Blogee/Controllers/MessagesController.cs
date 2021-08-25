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
    public class MessagesController : Controller
    {

        // GET: Messages
        public ActionResult Messages() { return View(); }

        [HttpPost]
        public ActionResult NewContact(SearchModel s) {

            string potentialContact = s.SearchTerm;
            List<UserModel> listPotentialContacts = new List<UserModel>(); // needs to be populated hereby

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
                builder.UserID = "serverusername";
                builder.Password = "ConnectDB1";
                builder.InitialCatalog = "Blogee_db";

                using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                {
                    // here we try to find the user not the post
                    string findContact = "SELECT * FROM dbo.Users WHERE Username LIKE @PotentialContact ";

                    using (SqlCommand queryFindContact = new SqlCommand(findContact))
                    {
                        queryFindContact.Connection = openCon;
                        queryFindContact.Parameters.AddWithValue("@PotentialContact", "%" + potentialContact + "%");
                        openCon.Open();
                        var reader = queryFindContact.ExecuteReader();

                        // for some reason we don't enter into the while loop below
                        while (reader.Read())
                        {
                            listPotentialContacts.Add(item: new UserModel
                            {
                                Username = reader["Username"].ToString(),
                                Name = reader["Name"].ToString(),
                            });
                        }
                    }

                    s.ListUsers = listPotentialContacts;
                    openCon.Close();
                }


            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return View("~/Views/Messages/Messages.cshtml", model:s);
        }


        [HttpPost]
        public void ChooseChat(string username)
        {
            OtherUserModel.OtherUsername = username;
            
            // code below written in order to retrieve the rest of the information about the user like the
            // common contacts, the name, etc. So far only the username of the person is known.
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
                builder.UserID = "serverusername";
                builder.Password = "ConnectDB1";
                builder.InitialCatalog = "Blogee_db";

                using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                {
                    string findContactChatWith = "SELECT * FROM dbo.Users WHERE Username = @ChatWith";

                    using (SqlCommand queryFindContactChatWith = new SqlCommand(findContactChatWith))
                    {
                        queryFindContactChatWith.Connection = openCon;
                        queryFindContactChatWith.Parameters.AddWithValue("@ChatWith", username);
                        openCon.Open();
                        var reader = queryFindContactChatWith.ExecuteReader();

                        while (reader.Read())
                        {
                            OtherUserModel.OtherName = reader["Name"].ToString();
                            
                        }
                    }

                    openCon.Close();
                }

            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        public ActionResult Chat()
        {
            SearchModel contactChatWithModel = new SearchModel() ;
            contactChatWithModel.UserOfInterest.Username = OtherUserModel.OtherUsername;

            string otherUser = OtherUserModel.OtherUsername;
            string currentUser = CurrentUserModel.CurrentUsername;

            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
                builder.UserID = "serverusername";
                builder.Password = "ConnectDB1";
                builder.InitialCatalog = "Blogee_db";

                using (SqlConnection openCon = new SqlConnection(builder.ConnectionString))
                {
                    // when you select a person to chat with, then you also need to update the number of messages in our chat
                    // for some reason when together it doesn't work so split into two different strings
                    string verifyMessageNumber1 = "SELECT MAX(MessageNumber) FROM dbo.Messages WHERE (User1 = @User5 AND User2 = @User6)";
                    string verifyMessageNumber2 = "SELECT MAX(MessageNumber) FROM dbo.Messages WHERE (User1 = @User3 AND User2 = @User4)";
                    MessageModel.numberMessage = 0;

                    using (SqlCommand queryFindMessageNumber1 = new SqlCommand(verifyMessageNumber1))
                    {
                        queryFindMessageNumber1.Connection = openCon;
                        queryFindMessageNumber1.Parameters.Add("@User5", SqlDbType.VarChar, 50).Value = otherUser;
                        queryFindMessageNumber1.Parameters.Add("@User6", SqlDbType.VarChar, 50).Value = currentUser;

                        // for some reason after this statement we don't go to the evaluation of the next integer
                        object queryResult1 = queryFindMessageNumber1.ExecuteScalar();

                        // when aiday.mar signs in and talks to bobby query result returns one
                        // when bobby signs in and talks to aiday.mar query result returns two
                        // which means it does not take into account the difference in order
                        // in the select statement above

                        if (queryResult1 != DBNull.Value)
                        {
                            MessageModel.numberMessage += Convert.ToInt32(queryResult1);
                        }
                    }

                    using (SqlCommand queryFindMessageNumber2 = new SqlCommand(verifyMessageNumber2))
                    {
                        queryFindMessageNumber2.Connection = openCon;
                        queryFindMessageNumber2.Parameters.Add("@User3", SqlDbType.VarChar, 50).Value = currentUser;
                        queryFindMessageNumber2.Parameters.Add("@User4", SqlDbType.VarChar, 50).Value = otherUser;
                        
                        // for some reason after this statement we don't go to the evaluation of the next integer
                        object queryResult2 = queryFindMessageNumber2.ExecuteScalar();

                        // when aiday.mar signs in and talks to bobby query result returns one
                        // when bobby signs in and talks to aiday.mar query result returns two
                        // which means it does not take into account the difference in order
                        // in the select statement above
                        
                        if (queryResult2 != DBNull.Value)
                        {
                            MessageModel.numberMessage += Convert.ToInt32(queryResult2);
                        }
                    }
                }
            }

            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return View("/Views/Chat/Chat.cshtml", model:contactChatWithModel);
        }

    }
}