using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mingl.Models;

namespace Mingl.Controllers
{
    public class HomeController : Controller
    {
        private MyContext _context;

        public HomeController(MyContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
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
            // HttpContext.Session.GetInt32("LoggedUserId");
            int loggedUserId = 4;

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
