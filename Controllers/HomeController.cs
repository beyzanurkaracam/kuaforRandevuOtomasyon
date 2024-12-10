using System.Diagnostics;
using System.Globalization;
using System.IO.Pipelines;
using System.Security.Claims;
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
                new Claim(ClaimTypes.Name, user.Email), // Burada Name claim ekleniyor
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    _logger.LogInformation("Kullan�c� Claims: {Claims}", string.Join(", ", principal.Claims.Select(c => $"{c.Type}: {c.Value}")));

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
            // Veritaban�ndan t�m �al��anlar� al�yoruz
            var employees = _context.Employees
                                    .Include(e => e.EmployeeServices) // EmployeeServices ili�kisinde yer alan hizmetleri de dahil ediyoruz
                                    .ThenInclude(es => es.Service)  // Hizmet ad� bilgilerini almak i�in ili�kiyi takip ediyoruz
                                    .ToList();

            var employeeList = new List<EmployeeRespond>();

            foreach (var employee in employees)
            {
                var employeeRespond = new EmployeeRespond
                {
                    EmployeeId = employee.EmployeeID,
                    EmployeeName = employee.Name,
                    // �al��an�n ald��� hizmetlerin adlar�n� al�yoruz
                    ServiceNames = employee.EmployeeServices.Select(es => es.Service.Name).ToList()
                };

                employeeList.Add(employeeRespond);
            }

            // Modeli view'a g�nderiyoruz
            return View(employeeList);
        }
        [Authorize]
        [HttpGet]
        public IActionResult RandevuAl()
        {
            _logger.LogInformation("Randevu al sayfas� y�klendi.");

            var serviceList = _context.Services.Select(h => new SelectListItem
            {
                Text = h.Name,
                Value = h.ServiceID.ToString()
            }).ToList();

            // �al��anlar ba�lang��ta bo� bir liste olabilir
            var employeeList = new List<SelectListItem>();

            var model = new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceList, employeeList);

            _logger.LogInformation("Hizmet listesi ba�ar�yla y�klendi. Toplam hizmet: {Count}", serviceList.Count);
            return View(model);
        }

        [HttpPost]
        public IActionResult RandevuAl(IFormCollection form)
        {
            _logger.LogInformation("HTTP POST metodu �a�r�ld�.");

            try
            {
                if (string.IsNullOrEmpty(form["id"]))
                {
                    _logger.LogWarning("Kullan�c� herhangi bir hizmet se�meden form g�nderdi.");
                    ModelState.AddModelError("id", "L�tfen bir hizmet se�in.");

                    var serviceListe = _context.Services.Select(h => new SelectListItem
                    {
                        Value = h.ServiceID.ToString(),
                        Text = h.Name
                    }).ToList();

                    return View(new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceListe, new List<SelectListItem>()));
                }

                int selectedServiceId = Convert.ToInt32(form["id"]);
                _logger.LogInformation("Kullan�c� {ServiceId} hizmetini se�ti.", selectedServiceId);

                var employeesByService = _context.Employees
                    .Where(x => x.EmployeeServices
                        .Any(es => es.ServiceID == selectedServiceId)) // Belirtilen hizmete sahip �al��anlar� al
                    .Select(d => new SelectListItem
                    {
                        Value = d.EmployeeID.ToString(),
                        Text = d.Name
                    })
                    .ToList();

                _logger.LogInformation("Hizmete ba�l� �al��anlar ba�ar�yla y�klendi. �al��an say�s�: {Count}", employeesByService.Count);

                var serviceList = _context.Services.Select(h => new SelectListItem
                {
                    Value = h.ServiceID.ToString(),
                    Text = h.Name
                }).ToList();

                // �al��anlar� ve hizmetleri ayn� sayfada g�ster
                return View(new Tuple<List<SelectListItem>, List<SelectListItem>>(serviceList, employeesByService));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Randevu al�rken bir hata olu�tu.");
                ModelState.AddModelError("", "Bir hata olu�tu. L�tfen tekrar deneyin.");
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
                // Kullan�c�y� almak
                var currentUser = _context.User.FirstOrDefault(u => u.Email == User.Identity.Name);
                if (currentUser == null)
                {
                    _logger.LogError("Current user not found: {userName}", User.Identity.Name);
                    return RedirectToAction("Error", "Home");
                }

                // �al��an� almak
                var appointedEmployee = getEmployeeValue(employeeId);
                if (appointedEmployee == null)
                {
                    _logger.LogError("Employee not found with employeeId: {employeeId}", employeeId);
                    return RedirectToAction("Error", "Home");
                }

                // Se�ilen hizmeti almak
                var selectedService = _context.Services.Find(serviceId);
                if (selectedService == null)
                {
                    _logger.LogError("Service not found with serviceId: {serviceId}", serviceId);
                    return RedirectToAction("Error", "Home");
                }

                // Tarih ve saatin d�zg�n �ekilde al�nd���ndan emin olal�m
                if (!selectedCardDate.HasValue)
                {
                    _logger.LogError("Selected date is null for employeeId: {employeeId} and serviceId: {serviceId}", employeeId, serviceId);
                    return RedirectToAction("Error", "Home");
                }

                DateTime RandevuTarihSaati = selectedCardDate.Value;

                // DateTimeKind kontrol� ve UTC'ye ayarlama
                if (RandevuTarihSaati.Kind == DateTimeKind.Unspecified)
                {
                    RandevuTarihSaati = DateTime.SpecifyKind(RandevuTarihSaati, DateTimeKind.Utc);
                }

                _logger.LogInformation("Parsed appointment date and time: {RandevuTarihSaati}", RandevuTarihSaati);

                // Tarih ve saat aral���n� hesapla
                DateTime tarih = RandevuTarihSaati.Date;
                TimeSpan baslangicZaman = RandevuTarihSaati.TimeOfDay;
                TimeSpan bitisZaman = baslangicZaman.Add(TimeSpan.FromMinutes(selectedService.Duration));

                _logger.LogInformation("Appointment time range: {baslangicZaman} to {bitisZaman}", baslangicZaman, bitisZaman);

                // �ak��an randevular� kontrol et
                var overlappingAppointments = _context.WorkingHours
                    .Where(wh => wh.EmployeeId == employeeId &&
                                 wh.baslangicWorkingHour != null &&
                                 wh.bitisWorkingHour != null &&
                                 wh.baslangicWorkingHour.Date == tarih &&
                                 (
                                     (baslangicZaman >= wh.baslangicWorkingHour.TimeOfDay && baslangicZaman < wh.bitisWorkingHour.TimeOfDay) ||
                                     (bitisZaman > wh.baslangicWorkingHour.TimeOfDay && bitisZaman <= wh.bitisWorkingHour.TimeOfDay) ||
                                     (baslangicZaman <= wh.baslangicWorkingHour.TimeOfDay && bitisZaman >= wh.bitisWorkingHour.TimeOfDay)
                                 ))
                    .Any();

                if (overlappingAppointments)
                {
                    _logger.LogWarning("Overlapping appointment found for employeeId: {employeeId} on {tarih}", employeeId, tarih);
                    return RedirectToAction("RandevuAl");
                }

                // Yeni �al��ma saati ekle
                var newWorkingHour = new WorkingHours
                {
                    EmployeeId = employeeId,
                    EmployeeName = appointedEmployee.Name,
                    baslangicWorkingHour = RandevuTarihSaati,
                    bitisWorkingHour = RandevuTarihSaati.AddMinutes(selectedService.Duration)
                };

                _context.WorkingHours.Add(newWorkingHour);
                _logger.LogInformation("New working hour added for employeeId: {employeeId}", employeeId);

                // Yeni randevuyu olu�tur
                var yeniRandevu = new Appointment
                {
                    UserId = currentUser.UserID,
                    FirstName = currentUser.FirstName,
                    LastName = currentUser.LastName,
                    RandevuSaati = RandevuTarihSaati,
                    EmployeeName = appointedEmployee.Name,
                    EmployeeId = appointedEmployee.EmployeeID
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
