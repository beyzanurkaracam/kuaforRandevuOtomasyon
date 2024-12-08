using System.Diagnostics;
using System.IO.Pipelines;
using System.Security.Claims;
using kuaforBerberOtomasyon.Models;
using kuaforBerberOtomasyon.Models.DTO;
using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kuaforBerberOtomasyon.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Context _context;

        public HomeController(ILogger<HomeController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        // Ana sayfa
        public IActionResult Index()
        {
            _logger.LogInformation("Ana sayfa y�klendi.");
            return View();
        }

        // Kay�t sayfas�
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("Kay�t sayfas�na giri� yap�ld�.");
            return View();
        }

        // Kay�t i�lemi
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullan�c�n�n zaten kay�tl� olup olmad���n� kontrol et
                var mevcutKullanici = _context.User.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

                if (mevcutKullanici != null)
                {
                    // E�er kullan�c� mevcutsa, login sayfas�na y�nlendir
                    _logger.LogWarning("Kullan�c� zaten mevcut: {Email}", model.Email);
                    return RedirectToAction("Login");
                }

                _logger.LogInformation("Yeni kullan�c� kaydediliyor: {Email}", model.Email);

                var yeniKullanici = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
                };

                // Admin kontrol�
                if (model.Email.ToLower() == "g211210037@ogr.sakarya.edu.tr" && model.Password.ToLower() == "sau")
                {
                    yeniKullanici.Role = "admin";
                }
                else
                {
                    yeniKullanici.Role = "customer";
                }

                // Yeni kullan�c�y� veritaban�na ekle
                _context.User.Add(yeniKullanici);
                _context.SaveChanges();

                _logger.LogInformation("Kullan�c� ba�ar�yla kaydedildi: {Email}", model.Email);
                return RedirectToAction("Login");
            }

            _logger.LogWarning("Kay�t i�lemi s�ras�nda hata olu�tu. Model ge�erli de�il.");
            return View(model);
        }

        // Giri� sayfas�
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            _logger.LogInformation("Giri� sayfas� y�klendi.");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            return View();
        }

        // Giri� i�lemi
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Kullan�c� giri�i denendi: {Email}", model.Email);

                var user = _context.User.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    _logger.LogInformation("Kullan�c� giri�i ba�ar�l�: {Email}", model.Email);

                    var claims = new List<Claim>
                    {
                        
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var identity = new ClaimsIdentity(claims, "Login");
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(principal);

                    if (user.Role == "admin")
                    {
                        return RedirectToAction("Index", "Admin");
                    }

                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                _logger.LogWarning("Kullan�c� ad� veya �ifre hatal�: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Kullan�c� ad� veya �ifre hatal�.");
            }

            return View(model);
        }
         public IActionResult Services()
         {
              var services = _context.Services.ToList();
        

         var serviceModelList = new List<ServicesRespond>();

         foreach (var service in services)
         {
             
             var serviceModel = new ServicesRespond
             {
                 Name = service.Name,
                 Duration= service.Duration,
                 Price= service.Price
             };

                serviceModelList.Add(serviceModel);
         }

         return View(serviceModelList);
         }
        public IActionResult Employees()
        {
            
                var employees = _context.Employees.ToList();


                var employeeList = new List<EmployeeRespond>();

                foreach (var employee in employees)
                {

                    var employeeler = new EmployeeRespond
                    {
                        EmployeeId = employee.EmployeeID,
                        EmployeeName = employee.Name,
                        ServiceName = employee.ServiceName
                    };

                    employeeList.Add(employeeler);
                }

                return View(employeeList);
            
           
        }

        // ��k�� i�lemi
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Kullan�c� ��k�� yapt�.");
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        // Hata sayfas�
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("Hata olu�tu. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
