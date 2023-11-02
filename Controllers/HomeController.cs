using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;

namespace BankAccounts.Controllers;

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

        return View();
    }

    [HttpPost]
    [Route("procesa/registro")]
    public IActionResult ProcesaRegistro(User newUser)
    {
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
            _context.Users.Add(newUser);
            _context.SaveChanges();
            HttpContext.Session.SetString("UserEmail", newUser.Email);
            HttpContext.Session.SetInt32("UserId", newUser.UserId);
            return RedirectToAction("Success");
        }
        return View("Index");
    }
    [HttpGet("accounts")]
    [SessionCheck]
    public IActionResult Success()
    {
        IEnumerable<Transaction> transactions = _context.Transactions.Where(c => c.UserId == HttpContext.Session.GetInt32("UserId"));
        int totalAmount = transactions.Sum(t => t.Amount);
        ViewBag.Transactions = transactions;
        ViewBag.TotalAmount = totalAmount;
        return View("Success");
    }

    [HttpPost]
    [Route("procesa/login")]
    public IActionResult ProcesaLogin(LoginUser loginUser)
    {
        if (ModelState.IsValid)
        {
            User? user = _context.Users.FirstOrDefault(us => us.Email == loginUser.EmailLogin);

            if (user != null)
            {
                PasswordHasher<LoginUser> Hasher = new PasswordHasher<LoginUser>();
                var result = Hasher.VerifyHashedPassword(loginUser, user.Password, loginUser.PasswordLogin);

                if (result != 0)
                {
                    HttpContext.Session.SetString("UserEmail", user.Email);
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    return RedirectToAction("Success");
                }
                ModelState.AddModelError("PasswordLogin", "Credenciales incorrectas");


            }
            ModelState.AddModelError("EmailLogin", "El correo electronico no existe ne la base de datos.");
            return View("Index");
        }
        return View("Index");
    }

    [HttpPost("update/balance")]
    public IActionResult UpdateBalance(Transaction newTransaction)
    {
        IEnumerable<Transaction> transactions = _context.Transactions.Where(c => c.UserId == HttpContext.Session.GetInt32("UserId"));
        int totalAmount = transactions.Sum(t => t.Amount);
        if (newTransaction.Amount < totalAmount)
        {
            ModelState.AddModelError("Amount", "No tienes saldo suficiente para realizar este retiro.");
            return RedirectToAction("Success");

        }
        
        _context.Transactions.Add(newTransaction);
        _context.SaveChanges();

        return RedirectToAction("Success");
    }

    [HttpGet("logout")]
    public IActionResult LogOut()
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

    public class SessionCheckAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? email = context.HttpContext.Session.GetString("UserEmail");
            if (email == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }

}
