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
    public class DBController : Controller
    {
       public SqlConnectionStringBuilder ConnectionBuilder()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "DESKTOP-ULB21CT\\SQLEXPRESS";
            builder.UserID = "serverusername";
            builder.Password = "ConnectDB1";
            builder.InitialCatalog = "Blogee_db";
            return builder;
        } 
    }
}