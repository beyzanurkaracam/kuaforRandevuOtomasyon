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
            _logger.LogInformation("Ana sayfa yüklendi.");
            return View();
        }

        // Kayýt sayfasý
        [HttpGet]
        public IActionResult Register()
        {
            _logger.LogInformation("Kayýt sayfasýna giriþ yapýldý.");
            return View();
        }

        // Kayýt iþlemi
        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kullanýcýnýn zaten kayýtlý olup olmadýðýný kontrol et
                var mevcutKullanici = _context.User.FirstOrDefault(u => u.Email.ToLower() == model.Email.ToLower());

                if (mevcutKullanici != null)
                {
                    // Eðer kullanýcý mevcutsa, login sayfasýna yönlendir
                    _logger.LogWarning("Kullanýcý zaten mevcut: {Email}", model.Email);
                    return RedirectToAction("Login");
                }

                _logger.LogInformation("Yeni kullanýcý kaydediliyor: {Email}", model.Email);

                var yeniKullanici = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
                };

                // Admin kontrolü
                if (model.Email.ToLower() == "g211210037@ogr.sakarya.edu.tr" && model.Password.ToLower() == "sau")
                {
                    yeniKullanici.Role = "admin";
                }
                else
                {
                    yeniKullanici.Role = "customer";
                }

                // Yeni kullanýcýyý veritabanýna ekle
                _context.User.Add(yeniKullanici);
                _context.SaveChanges();

                _logger.LogInformation("Kullanýcý baþarýyla kaydedildi: {Email}", model.Email);
                return RedirectToAction("Login");
            }

            _logger.LogWarning("Kayýt iþlemi sýrasýnda hata oluþtu. Model geçerli deðil.");
            return View(model);
        }

        // Giriþ sayfasý
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            _logger.LogInformation("Giriþ sayfasý yüklendi.");
            if (!string.IsNullOrEmpty(returnUrl))
            {
                TempData["ReturnUrl"] = returnUrl;
            }
            return View();
        }

        // Giriþ iþlemi
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> LoginAsync(LoginViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("Kullanýcý giriþi denendi: {Email}", model.Email);

                var user = _context.User.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    _logger.LogInformation("Kullanýcý giriþi baþarýlý: {Email}", model.Email);

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

                _logger.LogWarning("Kullanýcý adý veya þifre hatalý: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Kullanýcý adý veya þifre hatalý.");
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

        // Çýkýþ iþlemi
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Kullanýcý çýkýþ yaptý.");
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index");
        }

        // Hata sayfasý
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            _logger.LogError("Hata oluþtu. RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
