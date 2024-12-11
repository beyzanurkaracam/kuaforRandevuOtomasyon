using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using System.Security.Claims;
using kuaforBerberOtomasyon.AIService;
using kuaforBerberOtomasyon.Enums;
using kuaforBerberOtomasyon.Models;
using kuaforBerberOtomasyon.Models.DTO;
using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
        private readonly HairCutRecommendationService _hairCutRecommendationService;
       /* private readonly FaceRecognitionService _faceRecognitionService;
        public HairCutRecommendationController()
        {
            // Azure Face API'yi kullanmak için FaceRecognitionService'i baþlatýyoruz
            string apiKey = "YOUR_AZURE_API_KEY";
            string endpoint = "YOUR_AZURE_ENDPOINT";
            _faceRecognitionService = new FaceRecognitionService(apiKey, endpoint);

            _hairCutRecommendationService = new HairCutRecommendationService();
        }*/
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
                if (model.Email.ToLower() == "g211210054@ogr.sakarya.edu.tr" && model.Password.ToLower() == "sau")
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
                new Claim(ClaimTypes.Name, user.Email), // Burada Name claim ekleniyor
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _logger.LogInformation("Kullanýcý Claims: {Claims}", string.Join(", ", principal.Claims.Select(c => $"{c.Type}: {c.Value}")));

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
        public IActionResult SelectEmployee(int serviceId)
        {
            _logger.LogInformation("SelectEmployee action called with serviceId: {serviceId}", serviceId);

            try
            {
                var services = _context.Services.ToList();
                var employees = _context.Employees.ToList();

                _logger.LogInformation("Fetched {serviceCount} services and {employeeCount} employees from the database.", services.Count, employees.Count);

                var serviceSelectList = services.Select(s => new SelectListItem
                {
                    Text = s.Name,
                    Value = s.ServiceID.ToString()
                }).ToList();

                var employeeSelectList = employees.Select(e => new SelectListItem
                {
                    Text = e.Name,
                    Value = e.EmployeeID.ToString()
                }).ToList();

                var model = new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceSelectList, employeeSelectList);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in SelectEmployee action.");
                return StatusCode(500, "Internal server error.");
            }
        }

        public Employee getEmployeeValue(int Id)
        {
            _logger.LogInformation("getEmployeeValue called with Id: {Id}", Id);

            try
            {
                var employee = _context.Employees.FirstOrDefault(x => x.EmployeeID == Id);
                if (employee == null)
                {
                    _logger.LogWarning("No employee found with Id: {Id}", Id);
                }
                else
                {
                    _logger.LogInformation("Employee found: {employeeName}", employee.Name);
                }

                return employee;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in getEmployeeValue.");
                throw;
            }
        }
        [HttpPost]
        public IActionResult CreateAppointment(int employeeId, DateTime? selectedCardDate, int serviceId)
        {
            _logger.LogInformation("CreateAppointment action called with employeeId: {employeeId}, selectedCardDate: {selectedCardDate}, serviceId: {serviceId}", employeeId, selectedCardDate, serviceId);
            _logger.LogInformation("User.Identity.Name value: {Email}", User.Identity.Name);

            try
            {
                var currentUser = _context.User.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (currentUser == null)
                {
                    _logger.LogError("Current user not found: {userName}", User.Identity.Name);
                    return RedirectToAction("Error", "Home");
                }

                var appointedEmployee = getEmployeeValue(employeeId);
                if (appointedEmployee == null)
                {
                    _logger.LogError("Employee not found with employeeId: {employeeId}", employeeId);
                    return RedirectToAction("Error", "Home");
                }

                var selectedService = _context.Services.Find(serviceId);
                if (selectedService == null)
                {
                    _logger.LogError("Service not found with serviceId: {serviceId}", serviceId);
                    return RedirectToAction("Error", "Home");
                }

                if (!selectedCardDate.HasValue)
                {
                    _logger.LogError("Selected date is null for employeeId: {employeeId} and serviceId: {serviceId}", employeeId, serviceId);
                    return RedirectToAction("Error", "Home");
                }

                DateTime RandevuTarihSaati = selectedCardDate.Value;

                if (RandevuTarihSaati.Kind == DateTimeKind.Unspecified)
                {
                    RandevuTarihSaati = DateTime.SpecifyKind(RandevuTarihSaati, DateTimeKind.Utc);
                }

                _logger.LogInformation("Parsed appointment date and time: {RandevuTarihSaati}", RandevuTarihSaati);

                TimeSpan baslangicZaman = RandevuTarihSaati.TimeOfDay;
                TimeSpan bitisZaman = baslangicZaman.Add(TimeSpan.FromMinutes(selectedService.Duration));

                // Çalýþma saatleri kontrolü
                TimeSpan workStart = new TimeSpan(9, 0, 0); // 09:00
                TimeSpan workEnd = new TimeSpan(18, 0, 0); // 18:00

                if (baslangicZaman < workStart || bitisZaman > workEnd)
                {
                    _logger.LogWarning("Appointment time is outside working hours: {baslangicZaman} to {bitisZaman}", baslangicZaman, bitisZaman);
                    TempData["ErrorMessage"] = "Seçtiðiniz saat 09:00 ile 18:00 arasýnda olmalýdýr. Lütfen geçerli bir saat seçin.";
                    return RedirectToAction("RandevuAl");
                }

                var overlappingAppointments = _context.WorkingHours
                    .Where(wh => wh.EmployeeId == employeeId &&
                                 wh.baslangicWorkingHour != null &&
                                 wh.bitisWorkingHour != null &&
                                 wh.baslangicWorkingHour.Date == RandevuTarihSaati.Date &&
                                 (
                                     (baslangicZaman >= wh.baslangicWorkingHour.TimeOfDay && baslangicZaman < wh.bitisWorkingHour.TimeOfDay) ||
                                     (bitisZaman > wh.baslangicWorkingHour.TimeOfDay && bitisZaman <= wh.bitisWorkingHour.TimeOfDay) ||
                                     (baslangicZaman <= wh.baslangicWorkingHour.TimeOfDay && bitisZaman >= wh.bitisWorkingHour.TimeOfDay)
                                 ))
                    .Any();

                if (overlappingAppointments)
                {
                    _logger.LogWarning("Overlapping appointment found for employeeId: {employeeId} on {tarih}", employeeId, RandevuTarihSaati.Date);
                    TempData["ErrorMessage"] = "Seçtiðiniz tarihte çalýþan müsait deðil. Lütfen farklý bir tarih seçin.";
                    return RedirectToAction("RandevuAl");
                }

                var newWorkingHour = new WorkingHours
                {
                    EmployeeId = employeeId,
                    EmployeeName = appointedEmployee.Name,
                    baslangicWorkingHour = RandevuTarihSaati,
                    bitisWorkingHour = RandevuTarihSaati.AddMinutes(selectedService.Duration)
                };

                _context.WorkingHours.Add(newWorkingHour);
                _logger.LogInformation("New working hour added for employeeId: {employeeId}", employeeId);

                var yeniRandevu = new Appointment
                {
                    UserId = currentUser.UserID,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    Email = currentUser.Email,
                    RandevuSaati = RandevuTarihSaati,
                    EmployeeName = appointedEmployee.Name,
                    EmployeeId = appointedEmployee.EmployeeID,
                    ServiceName = selectedService.Name
                };

                _context.Appointments.Add(yeniRandevu);
                _context.SaveChanges();

                _logger.LogInformation("New appointment created for userId: {userId}", currentUser.UserID);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error occurred: {Message}", ex.Message);
                return RedirectToAction("Error", "Home");
            }
        }

        public IActionResult CancelAppointment(int Id)
        {
            var silinecekRandevu = _context.Appointments.Find(Id);

            if (silinecekRandevu != null)
            {
                _context.Remove(silinecekRandevu);
                _context.SaveChanges();
                return RedirectToAction("ViewAppointments", "Home");
            }
            else
            {
                return RedirectToAction("ViewAppointments", "Home");
            }
        }

        [Authorize]
        public IActionResult ViewAppointments()
        {
            var aktifRandevular = _context.Appointments.Where(x => x.Email == User.Identity.Name).ToList(); // aktif kullanýcýnýn adýyla bir randevu var mý.
            return View(aktifRandevular);
        }
       /* [HttpPost]
        public async Task<IActionResult> GetHairCutRecommendation(string imagePath)
        {
            // Yüz þekli tespitini Azure Face API ile yapýyoruz
            var faceShape = await _faceRecognitionService.GetFaceShape(imagePath);

            // Yüz þekline göre öneri alýyoruz
            if (faceShape.HasValue)
            {
                var suggestion = _hairCutRecommendationService.GetSuggestion(faceShape.Value);
                return View("HairCutRecommendation", suggestion);
            }

            return View("Error", "Yüz þekli tespit edilemedi.");
        }*/

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
