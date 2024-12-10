using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using System.Security.Claims;
using kuaforBerberOtomasyon.Models;
using kuaforBerberOtomasyon.Models.DTO;
using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
            // Veritabanýndan tüm çalýþanlarý alýyoruz
            var employees = _context.Employees
                                    .Include(e => e.EmployeeServices) // EmployeeServices iliþkisinde yer alan hizmetleri de dahil ediyoruz
                                    .ThenInclude(es => es.Service)  // Hizmet adý bilgilerini almak için iliþkiyi takip ediyoruz
                                    .ToList();

            var employeeList = new List<EmployeeRespond>();

            foreach (var employee in employees)
            {
                var employeeRespond = new EmployeeRespond
                {
                    EmployeeId = employee.EmployeeID,
                    EmployeeName = employee.Name,
                    // Çalýþanýn aldýðý hizmetlerin adlarýný alýyoruz
                    ServiceNames = employee.EmployeeServices.Select(es => es.Service.Name).ToList()
                };

                employeeList.Add(employeeRespond);
            }

            // Modeli view'a gönderiyoruz
            return View(employeeList);
        }
        [Authorize]
        [HttpGet]
        public IActionResult RandevuAl()
        {
            _logger.LogInformation("Randevu al sayfasý yüklendi.");

            var serviceList = _context.Services.Select(h => new SelectListItem
            {
                Text = h.Name,
                Value = h.ServiceID.ToString()
            }).ToList();

            // Çalýþanlar baþlangýçta boþ bir liste olabilir
            var employeeList = new List<SelectListItem>();

            var model = new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceList, employeeList);

            _logger.LogInformation("Hizmet listesi baþarýyla yüklendi. Toplam hizmet: {Count}", serviceList.Count);
            return View(model);
        }

        [HttpPost]
        public IActionResult RandevuAl(IFormCollection form)
        {
            _logger.LogInformation("HTTP POST metodu çaðrýldý.");

            try
            {
                if (string.IsNullOrEmpty(form["id"]))
                {
                    _logger.LogWarning("Kullanýcý herhangi bir hizmet seçmeden form gönderdi.");
                    ModelState.AddModelError("id", "Lütfen bir hizmet seçin.");

                    var serviceListe = _context.Services.Select(h => new SelectListItem
                    {
                        Value = h.ServiceID.ToString(),
                        Text = h.Name
                    }).ToList();

                    return View(new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceListe, new List<SelectListItem>()));
                }

                int selectedServiceId = Convert.ToInt32(form["id"]);
                _logger.LogInformation("Kullanýcý {ServiceId} hizmetini seçti.", selectedServiceId);

                var employeesByService = _context.Employees
                    .Where(x => x.EmployeeServices
                        .Any(es => es.ServiceID == selectedServiceId)) // Belirtilen hizmete sahip çalýþanlarý al
                    .Select(d => new SelectListItem
                    {
                        Value = d.EmployeeID.ToString(),
                        Text = d.Name
                    })
                    .ToList();

                _logger.LogInformation("Hizmete baðlý çalýþanlar baþarýyla yüklendi. Çalýþan sayýsý: {Count}", employeesByService.Count);

                var serviceList = _context.Services.Select(h => new SelectListItem
                {
                    Value = h.ServiceID.ToString(),
                    Text = h.Name
                }).ToList();

                // Çalýþanlarý ve hizmetleri ayný sayfada göster
                return View(new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceList, employeesByService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu alýrken bir hata oluþtu.");
                ModelState.AddModelError("", "Bir hata oluþtu. Lütfen tekrar deneyin.");
                var serviceList = _context.Services.Select(h => new SelectListItem
                {
                    Value = h.ServiceID.ToString(),
                    Text = h.Name
                }).ToList();
                ModelState.Clear();
                return View(new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceList, new List<SelectListItem>()));
            }
        }

        [HttpGet]
        public IActionResult SelectEmployee(int Id)
        {
            var employeeCalisma = _context.WorkingHours
                .Where(x => x.EmployeeId == Id)
                .ToList();

            if (employeeCalisma == null || employeeCalisma.Count == 0)
            {
                ViewBag.Mesaj = "Çalýþanýn uygun randevusu bulunamadý.";
                return View();
            }

            var model = new Tuple<List<WorkingHours>, int>(employeeCalisma, Id);
            return View(model);
        }


        // idsine göre gelen doktoru bulan döndüren metod 
        public Employee getEmployeeValue(int Id)
        {
            var appointedEmployee = _context.Employees.Where(x => x.EmployeeID == Id).FirstOrDefault();
            return appointedEmployee;
        }
        [HttpPost]
        public IActionResult CreateAppointment(int employeeId, string selectedCardDate)
        {
            var currentUser = _context.User.FirstOrDefault(u => u.FirstName == User.Identity.Name);

            //randevu alýnan doktoru idsine göre bul
            var randevuAlinanCalisan = getEmployeeValue(employeeId);

            //randevu saatini al
            DateTime randevuSaati = DateTime.ParseExact(selectedCardDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);

            //Doktorun id si ve çalýþma saati veritabanýnda varsa yani böyle bir doktor varsa bunu deðiþkene ata çünk randevu alýnca bunu kaldýrmamýz gerekecek.
            var calismaSaatiniBul = _context.WorkingHours.ToList().Where(x => x.EmployeeId == employeeId && x.WorkingHour == randevuSaati).FirstOrDefault();


            var yeniRandevu = new Appointment
            {
                UserId = currentUser.UserID,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                RandevuSaati = randevuSaati,
                EmployeeName = randevuAlinanCalisan.Name,
                EmployeeId = randevuAlinanCalisan.EmployeeID
            };

            _context.Appointments.Add(yeniRandevu);
            if (calismaSaatiniBul != null)
                _context.WorkingHours.Remove(calismaSaatiniBul);

            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
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
