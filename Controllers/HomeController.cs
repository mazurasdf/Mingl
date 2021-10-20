using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Mingl.Models;

namespace Mingl.Contollers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private MyContext _context;

        public HomeController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View("LoginReg");
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(_context.Users.Any(user => user.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in system!");
                    return View("LoginReg");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                _context.Add(newUser);
                _context.SaveChanges();

                HttpContext.Session.SetInt32("LoggedUserId", newUser.UserId);


                return RedirectToAction("RegTwo");
            }
            return View("LoginReg");
        }

        [HttpGet("RegTwo")]
        public IActionResult RegTwo()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);


            return View();
        }
        [HttpPost("FinalizeUser")]
        public IActionResult FinalizeUser(User incompleteUser)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);
            if(incompleteUser.ProfilePicUrl != null)
            {
                ViewBag.User.ProfilePicUrl = incompleteUser.ProfilePicUrl;
            }
            ViewBag.User.Gender = incompleteUser.Gender;
            ViewBag.User.PreferredUsers = incompleteUser.PreferredUsers;
            ViewBag.User.Likes = incompleteUser.Likes;
            ViewBag.User.Bio = incompleteUser.Bio;
            ViewBag.User.DatePhysical = incompleteUser.DatePhysical;
            ViewBag.User.DateCasual = incompleteUser.DateCasual;
            ViewBag.User.DateFood = incompleteUser.DateFood;
            ViewBag.User.DateCoffee = incompleteUser.DateCoffee;
            ViewBag.User.DateBar = incompleteUser.DateBar;

            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }


        [HttpPost("login")]
        public IActionResult Login(LoginUser checkMe)
        {
            if(ModelState.IsValid)
            {
                User userInDb = _context.Users.FirstOrDefault(use => use.Email == checkMe.LoginEmail);
                if(userInDb == null)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Login!");
                    return View("LoginReg");
                }

                PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();

                var result = hasher.VerifyHashedPassword(checkMe, userInDb.Password, checkMe.LoginPassword);

                if(result ==0)
                {
                    ModelState.AddModelError("LoginEmail", "Invalid Login!");
                    return View("LoginReg");
                }

                HttpContext.Session.SetInt32("LoggedUserId", userInDb.UserId);
                return RedirectToAction("dashboard");
            }
            return View("LoginReg");
        }

        [HttpGet("/dashboard")]
        public IActionResult Dashboard()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);
            if(ViewBag.User.ProfilePicUrl =="https://cdn.pixabay.com/photo/2015/10/05/22/37/blank-profile-picture-973460_960_720.png")
            {
                return RedirectToAction("RegTwo");
            }

            return View();
        }
        [HttpGet("RegEdit")]
        public IActionResult RegEdit()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);

            return View("RegEdit");
        }

        [HttpPost("EditProfile")]
        public IActionResult EditProfile(User editingUser)
        {

            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");
            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == loggedUserId);
            if(_context.Users.Any(user => user.Email == editingUser.Email) && editingUser.Email != ViewBag.User.Email)
                {
                    ModelState.AddModelError("Email", "Email already in system!");
                    return View("RegEdit");
                }

            ViewBag.User.Email = editingUser.Email;
            ViewBag.User.Name = editingUser.Name;
            ViewBag.User.ProfilePicUrl = editingUser.ProfilePicUrl;
            ViewBag.User.Gender = editingUser.Gender;
            ViewBag.User.PreferredUsers = editingUser.PreferredUsers;
            ViewBag.User.Likes = editingUser.Likes;
            ViewBag.User.Bio = editingUser.Bio;
            ViewBag.User.DatePhysical = editingUser.DatePhysical;
            ViewBag.User.DateCasual = editingUser.DateCasual;
            ViewBag.User.DateFood = editingUser.DateFood;
            ViewBag.User.DateCoffee = editingUser.DateCoffee;
            ViewBag.User.DateBar = editingUser.DateBar;

            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }




        [HttpGet("matching")]
        public IActionResult MatchingMain()
        {
            //TODO testing with session, remove this later
            // HttpContext.Session.GetInt32("LoggedUserId");
            int loggedUserId = 4;

            //viewbag logged user
            ViewBag.LoggedUser = _context.Users.FirstOrDefault(user => user.UserId == loggedUserId);
            
            //viewbag users who have sent a match request
            ViewBag.ReceivedRequests = _context.MatchRequests
                .Include(mr => mr.Sender)
                .Where(mr => mr.ReceiverId == loggedUserId)
                .ToList();
            
            //search for random user who is looking for gender that logged user identifies as
            int OthersPrefer =   ViewBag.LoggedUser.Gender == "Female" ? 2
                                : ViewBag.LoggedUser.Gender == "Male" ? 3
                                : 1;
            string UserPrefers =    ViewBag.LoggedUser.PreferredUsers == 2 ? "Female"
                                    : ViewBag.LoggedUser.PreferredUsers == 3 ? "Male"
                                    : "Any";

            var RandoList = _context.Users
                .Where(user => user.UserId != loggedUserId)
                .Where(user => user.PreferredUsers == 1 || user.PreferredUsers == OthersPrefer)
                .Where(user => UserPrefers == "Any" || user.Gender == UserPrefers);
            Random rand = new Random();
            int toSkip = rand.Next(RandoList.Count());
            // Console.WriteLine(toSkip);
            //viewbag first random user who could match with logged user
            ViewBag.RandomUser = RandoList
                .Skip(toSkip)
                .FirstOrDefault();

            return View();

        }

        [HttpGet("matching/send/{id}")]
        public IActionResult SendMatchRequest(int id)
        {
            //TODO testing with session, remove this later
            // HttpContext.Session.GetInt32("LoggedUserId");
            int loggedUserId = 4;

            MatchRequest newMR = new MatchRequest();
            newMR.SenderId = loggedUserId;
            newMR.ReceiverId = id;

            _context.Add(newMR);
            _context.SaveChanges();

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("matching/pass/{id}")]
        public IActionResult PassMatchRequest(int id)
        {
            //TODO testing with session, remove this later
            HttpContext.Session.GetInt32("LoggedUserId");

            //do something with the pass

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("matching/denyRequest/{id}")]
        public IActionResult DenyRequest(int id)
        {
            Console.WriteLine("deny request");
            int loggedUserId = 4;
            MatchRequest deleteMe = _context.MatchRequests
                .FirstOrDefault(mr => mr.SenderId == id && mr.ReceiverId == loggedUserId);

            _context.MatchRequests.Remove(deleteMe);
            _context.SaveChanges();

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("matching/acceptRequest/{id}")]
        public IActionResult AcceptRequest(int id)
        {
            Console.WriteLine("accept request");
            int loggedUserId = 4;
            MatchRequest deleteMe = _context.MatchRequests
                .FirstOrDefault(mr => mr.SenderId == id && mr.ReceiverId == loggedUserId);

            _context.MatchRequests.Remove(deleteMe);
            _context.SaveChanges();

            Conversation newConvo = new Conversation();
            newConvo.SenderId = id;
            newConvo.ReceiverId = loggedUserId;
            _context.Add(newConvo);
            _context.SaveChanges();

            return RedirectToAction("MatchingMain");
        }        

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}