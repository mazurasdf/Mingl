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


                return RedirectToAction("MatchingMain");
            }
            return View("LoginReg");
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
                return RedirectToAction("MatchingMain");
            }
            return View("LoginReg");
        }

        [HttpGet("/dashboard")]
        public IActionResult Dashboard()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.User = _context.Users.FirstOrDefault(use => use.UserId == (int)loggedUserId);


            return View();
        }

        [HttpGet("matching")]
        public IActionResult MatchingMain()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            //viewbag logged user
            ViewBag.LoggedUser = _context.Users.FirstOrDefault(user => user.UserId == (int)loggedUserId);
            
            //viewbag users who have sent a match request
            ViewBag.ReceivedRequests = _context.MatchRequests
                .Include(mr => mr.Sender)
                .Where(mr => mr.ReceiverId == (int)loggedUserId)
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
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            MatchRequest newMR = new MatchRequest();
            newMR.SenderId = (int)loggedUserId;
            newMR.ReceiverId = id;

            _context.Add(newMR);
            _context.SaveChanges();

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("matching/pass/{id}")]
        public IActionResult PassMatchRequest(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            //do something with the pass

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("matching/denyRequest/{id}")]
        public IActionResult DenyRequest(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            MatchRequest deleteMe = _context.MatchRequests
                .FirstOrDefault(mr => mr.SenderId == id && mr.ReceiverId == (int)loggedUserId);

            _context.MatchRequests.Remove(deleteMe);
            _context.SaveChanges();

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("matching/acceptRequest/{id}")]
        public IActionResult AcceptRequest(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            MatchRequest deleteMe = _context.MatchRequests
                .FirstOrDefault(mr => mr.SenderId == id && mr.ReceiverId == (int)loggedUserId);

            _context.MatchRequests.Remove(deleteMe);
            _context.SaveChanges();

            Conversation newConvo = new Conversation();
            newConvo.SenderId = id;
            newConvo.ReceiverId = (int)loggedUserId;
            _context.Add(newConvo);
            _context.SaveChanges();

            return RedirectToAction("MatchingMain");
        }

        [HttpGet("chats")]
        public IActionResult AllChats()
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            ViewBag.Conversations = _context.Conversations
                .Include(conv => conv.Sender)
                .Include(conv => conv.Receiver)
                .Include(conv => conv.Messages)
                .Where(conv => conv.SenderId == loggedUserId || conv.ReceiverId == loggedUserId)
                .ToList();

            ViewBag.User = _context.Users
                .FirstOrDefault(user => user.UserId == loggedUserId);

            return View();
        }     

        [HttpGet("chats/{id}")]
        public IActionResult IndividualChat(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            Conversation thisChat = _context.Conversations
                .FirstOrDefault(conv => conv.ConversationId == id);

            //if chat exists and logged user is one of the participants
            if(thisChat != null && (thisChat.ReceiverId == loggedUserId || thisChat.SenderId == loggedUserId))
            {
                ViewBag.User = _context.Users
                    .FirstOrDefault(user => user.UserId == loggedUserId);

                ViewBag.Conversation = thisChat;

                ViewBag.ChatMessages = _context.Messages
                    .Include(mess => mess.Sender)
                    .Where(mess => mess.ConversationId == id)
                    .ToList();

                return View();
            }

            return RedirectToAction("AllChats");
        }

        [HttpPost("chats/send")]
        public IActionResult SendChat(Message newMessage)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            Conversation thisChat = _context.Conversations
                .FirstOrDefault(conv => conv.ConversationId == newMessage.ConversationId);

            //if chat exists and logged user is one of the participants
            if(thisChat != null && (thisChat.ReceiverId == loggedUserId || thisChat.SenderId == loggedUserId))
            {
                _context.Add(newMessage);
                _context.SaveChanges();
            }

            return RedirectToAction("IndividualChat", new {id = newMessage.ConversationId});
        }   

        [HttpGet("chats/{id}/ready")]
        public IActionResult ReadyToMingl(int id)
        {
            int? loggedUserId = HttpContext.Session.GetInt32("LoggedUserId");
            if(loggedUserId==null) return RedirectToAction("Index");

            Conversation thisChat = _context.Conversations
                .FirstOrDefault(conv => conv.ConversationId == id);

            if(thisChat.ReceiverId == loggedUserId)
            {
                thisChat.ReceiverReady = true;
            }
            else if(thisChat.SenderId == loggedUserId)
            {
                thisChat.SenderReady = true;
            }

            _context.SaveChanges();
            return RedirectToAction("IndividualChat", new {id = id});
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