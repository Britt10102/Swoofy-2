using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Swoofy.API.Models;
using Swoofy.API.Services;
using System.Data;

namespace Swoofy.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public RegistrationController(IConfiguration configuration, JwtService jwtService)
        {
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [HttpPost]
        [Route("registration")]
        public string registration(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SwoofyCon").ToString());
            con.Open();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registration.Password);
            SqlCommand cmd = new SqlCommand("INSERT INTO Registration(UserName, Password, Email, IsActive) VALUES('" + registration.UserName + "','" + hashedPassword + "','" + registration.Email + "','" + registration.IsActive + "')", con);
            int i = cmd.ExecuteNonQuery();
            con.Close();
            if (i > 0) {
                return "Data inserted";
            } else {

                return "Error";
            }
        }

        [HttpPost]
        [Route("login")]
        public string login(Registration registration)
        {
            SqlConnection con = new SqlConnection(_configuration.GetConnectionString("SwoofyCon").ToString());
            con.Open();
            SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Registration WHERE Email = '" + registration.Email +"' AND IsActive = 1", con);
            DataTable dt = new DataTable();
            da.Fill(dt);
            con.Close();
            if (dt.Rows.Count > 0) {
                string hashedPasswordFromDB = dt.Rows[0]["Password"].ToString();
                bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(registration.Password, hashedPasswordFromDB);

                if (isPasswordCorrect) {
                    var token = _jwtService.GenerateToken(registration.Email);
                    return token;
                }
            }
            return "Invalid User";

        }
    }
}
