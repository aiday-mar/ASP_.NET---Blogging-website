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
    public class ChatController : Controller
    {

       [HttpPost]
        public ActionResult SendMessage(SearchModel s)
        {
            // Debug shows the SendMessage property of the SearchModel is correctly initialized
            string message = s.SendMessage;
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

                    // the problem here is that the numberMessage changes and does not remain one value

                    int NextMessageNumberInt = MessageModel.numberMessage + 1;

                    string NextMessageID = "";

                    if (string.Compare(currentUser, otherUser) == 1)
                    {
                        // implies that currentUser > otherUser, the smaller string first
                        NextMessageID = otherUser + ":" + currentUser + ":" + NextMessageNumberInt.ToString();
                    }
                    else
                    {
                        NextMessageID = currentUser + ":" + otherUser + ":" + NextMessageNumberInt.ToString();
                    }

                    // when all these values above are found, now we can insert into the table, the new message
                    string InsertMessage = "INSERT into dbo.Messages (MessageID, User1, User2, Message, MessageNumber, Author) VALUES (@MessageID, @User1, @User2,@Message, @MessageNumber, @Author)";
                        
                    using (SqlCommand queryInsertMessage = new SqlCommand(InsertMessage))
                    {
                        queryInsertMessage.Connection = openCon;
                        queryInsertMessage.Parameters.Add("@MessageID", SqlDbType.VarChar, 150).Value = NextMessageID;
                        queryInsertMessage.Parameters.Add("@User1", SqlDbType.VarChar, 50).Value = currentUser;
                        queryInsertMessage.Parameters.Add("@User2", SqlDbType.VarChar, 50).Value = otherUser;
                        queryInsertMessage.Parameters.Add("@Message", SqlDbType.NVarChar, 500).Value = message;
                        queryInsertMessage.Parameters.Add("@MessageNumber", SqlDbType.Int).Value = NextMessageNumberInt;
                        queryInsertMessage.Parameters.Add("@Author", SqlDbType.VarChar, 50).Value = currentUser;
                        openCon.Open();
                        queryInsertMessage.ExecuteNonQuery();

                    }

                    openCon.Close();
                }

                // when you have appended the message into the database, you need to update the view and the number of messages by calling the update function

                UpdateChat(OtherUserModel.OtherUsername);
                                                                          
            }

            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            SearchModel searchModel = new SearchModel();
            searchModel.UserOfInterest.Username = OtherUserModel.OtherUsername;

            return View("~/Views/Chat/Chat.cshtml", model:searchModel);
        }

        // will need to remove the username parameter later because for the moment it is not needed
        // since we access the username through the static class

        [HttpPost]
        public ActionResult UpdateChat(string username)
        {
            // here you need to keep a list of the last 5 messages and if not the same after retrieval you need to append them in a div on the page before
            string otherUser = OtherUserModel.OtherUsername;
            string currentUser = CurrentUserModel.CurrentUsername;
            SearchModel searchModel = new SearchModel();
            searchModel.UserOfInterest.Username = OtherUserModel.OtherUsername;

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
                    string verifyMessageNumber = "SELECT MAX(MessageNumber) FROM dbo.Messages WHERE (User1 = @User1 AND User2 = @User2) OR (User1 = @User3 AND User2 = @User4)";
                    using (SqlCommand queryFindMessageNumber = new SqlCommand(verifyMessageNumber))
                    {
                        queryFindMessageNumber.Connection = openCon;
                        queryFindMessageNumber.Parameters.Add("@User1", SqlDbType.VarChar, 50).Value = otherUser;
                        queryFindMessageNumber.Parameters.Add("@User2", SqlDbType.VarChar, 50).Value = currentUser;
                        queryFindMessageNumber.Parameters.Add("@User3", SqlDbType.VarChar, 50).Value = currentUser;
                        queryFindMessageNumber.Parameters.Add("@User4", SqlDbType.VarChar, 50).Value = otherUser;
                        openCon.Open();
                        // for some reason after this statement we don't go to the evaluation of the next integer
                        object queryResult = queryFindMessageNumber.ExecuteScalar();

                        if (queryResult != DBNull.Value)
                        {
                            // in this case you need to compare the query result with the number of messages there were last time we checked in the static Message model
                            // if the difference is bigger than 0 then we need to append these messages into the chat window
                            int previousMessageNumber = MessageModel.numberMessage;
                            int currentMessageNumber = Convert.ToInt32(queryResult);
                            int diff = currentMessageNumber - previousMessageNumber;

                            if (diff != 0)
                            {
                                // this is the only moment when we update the number of messages in the static class
                                MessageModel.numberMessage = currentMessageNumber;

                                // now you need to display these, but what you actually need to do is to create a partial view with the cache, and here you would append only the last couple of messages
                                // the other ones already being there becasue of caching

                                // if this doesn't work we will just need to update completely the view every 2 seconds
                                // for now we can just display the latest messages between the previous time and the current time 
                                // using partial view
                                // for now we can decide not to differentiate between who sent the message and who didn't, we can add that later
                                // maybe here you could decide to show the latest 15 messages if a message was indeed appended

                                // what we will do is we will assign the last 15 messages into the list of messages, send them to the  chat and if update is true
                                // we update. Then when you exit the chat, all the attributes of the message model should be set to null ready for another chat.

                                MessageModel.update = true;
                                MessageModel.listMessages = null;

                                List<IndividualMessageModel> listMessagesTemp = new List<IndividualMessageModel>();

                                // need to erase previous list and append the last 15 messages into the list of Messages
                                try
                                {
                                    // when you order in descending order then you see the messages displayed in descending order too
                                    string findMessages = "SELECT Message, Author FROM dbo.Messages WHERE (User1 = @User1 AND User2 = @User2) OR (User1 = @User3 AND User2 = @User4) ORDER BY MessageNumber";

                                    using (SqlCommand queryFindMessages = new SqlCommand(findMessages))
                                    {
                                        queryFindMessages.Connection = openCon;
                                        queryFindMessages.Parameters.Add("@User1", SqlDbType.VarChar, 50).Value = otherUser;
                                        queryFindMessages.Parameters.Add("@User2", SqlDbType.VarChar, 50).Value = currentUser;
                                        queryFindMessages.Parameters.Add("@User3", SqlDbType.VarChar, 50).Value = currentUser;
                                        queryFindMessages.Parameters.Add("@User4", SqlDbType.VarChar, 50).Value = otherUser;

                                        var reader = queryFindMessages.ExecuteReader();

                                        int i = 1;
                                        // for some reason we don't enter into the while loop below
                                        while ((reader.Read()) && (i < 15))
                                        {
                                            listMessagesTemp.Add(item: new IndividualMessageModel
                                            {
                                                Message = reader["Message"].ToString(),
                                                Author = reader["Author"].ToString(),
                                            });

                                            i += 1;
                                            // the above selects only the last 15 messages. Indeed here we select the last messages because
                                            // the result is ordered in decreasing orde against the MessageNumber.
                                        }
                                    }

                                    MessageModel.listMessages = listMessagesTemp;
                                    openCon.Close();
                         
                                }                               
                                catch (SqlException e)
                                {
                                    System.Diagnostics.Debug.WriteLine(e.ToString());
                                }

                            }

                            else
                            {
                                // the returned divs should be permanent
                                return View("~/Views/Chat/Chat.cshtml");
                            }
                        }

                        // else this means that there are no messages
                    }
                }
            }
            catch (SqlException e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return View("~/Views/Chat/Chat.cshtml", model: searchModel);
        }
    }
}