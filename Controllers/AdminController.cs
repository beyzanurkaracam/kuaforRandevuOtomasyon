using kuaforBerberOtomasyon.Models;
using kuaforBerberOtomasyon.Models.DTO;
using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace kuaforBerberOtomasyon.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly Context _context;

        public AdminController(ILogger<AdminController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }
        public bool AdminControl()
        {
            if (User.Identity.IsAuthenticated)
            {

                if (User.IsInRole("admin")) 
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public IActionResult Index()
        {
            if (AdminControl())
            {
                var services = _context.Services.ToList();
                return View(services);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult Services()
        {
            if (AdminControl())
            {
                var services = _context.Services.ToList();
                
                var servicesModelList = new List<ServicesRespond>();

                foreach (var service in services)
                {
                    
                    var serviceModel = new ServicesRespond
                    {
                        serviceId= service.ServiceID,
                        Name = service.Name,
                        Duration = service.Duration,
                        Price = service.Price
                    };

                    servicesModelList.Add(serviceModel);
                }

                return View(servicesModelList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult AddService()
        {
            return View(); // Servis ekleme formunu göstermek için bir View döner
        }

        [HttpPost]
        public IActionResult AddService(AdminServiceRequest request)
        {
            if (ModelState.IsValid) // Gelen verilerin doğruluğunu kontrol eder
            {
                // Servisin daha önce veritabanına eklenip eklenmediğini kontrol et
                var existingService = _context.Services
                    .FirstOrDefault(s => s.Name == request.Name); // İsimle arama yapabilirsiniz, isteğe bağlı olarak başka parametrelerle de kontrol edilebilir.

                if (existingService != null)
                {
                    // Eğer servis zaten varsa, kullanıcıya mesaj gösterin veya başka bir işlem yapın
                    ModelState.AddModelError("", "Bu servis zaten mevcut.");
                    return View(request); // Hata mesajı ile formu tekrar gösterir
                }

                // Servis yeni ise, yeni bir servis oluşturup veritabanına ekleyin
                var newService = new Services
                {
                    Name = request.Name,
                    Duration = request.Duration, // Dakika cinsinden süre
                    Price = request.Price,
                    CreatedAt = DateTime.Now.ToUniversalTime() // UTC'ye dönüştürülür
                };

                _context.Services.Add(newService); // Yeni servisi ekle
                _context.SaveChanges(); // Değişiklikleri kaydet

                return RedirectToAction("Services", "Admin"); // Servis listesine yönlendirir
            }

            return View(); // Eğer model geçerli değilse tekrar formu gösterir

        }
        [HttpGet]
        public IActionResult UpdateService(int id)
        {
            _logger.LogInformation("UpdateService GET request received for ServiceID: {ServiceID}", id);

            // Veritabanından ilgili hizmeti alıyoruz
            var service = _context.Services.FirstOrDefault(s => s.ServiceID == id);

            if (service == null)
            {
                _logger.LogWarning("Service with ServiceID: {ServiceID} not found.", id);
                return NotFound();
            }

            // Service'yi DTO'ya dönüştürüp View'a gönderiyoruz
            var serviceDto = new ServiceUpdateDto
            {
                ServiceID = service.ServiceID,
                Name = service.Name,
                Duration = service.Duration,
                Price = service.Price
            };

            _logger.LogInformation("Returning View with Service data for ServiceID: {ServiceID}.", id);
            return View(serviceDto);
        }


        [HttpPost]
        public IActionResult UpdateService(ServiceUpdateDto dto)
        {
            _logger.LogInformation("UpdateService POST request received for ServiceID: {ServiceID}.", dto.ServiceID);

            // Model doğrulaması
            if (ModelState.IsValid)
            {
                // Service'i veritabanından buluyoruz
                var service = _context.Services.FirstOrDefault(s => s.ServiceID == dto.ServiceID);

                if (service == null)
                {
                    _logger.LogWarning("Service with ServiceID: {ServiceID} not found during update.", dto.ServiceID);
                    return NotFound();
                }

                _logger.LogInformation("Updating Service with ServiceID: {ServiceID}.", dto.ServiceID);

                // DTO'dan gelen verilerle Service'i güncelliyoruz
                service.Name = dto.Name;
                service.Duration = dto.Duration;
                service.Price = dto.Price;

                // Değişiklikleri kaydediyoruz
                _context.SaveChanges();

                _logger.LogInformation("Service with ServiceID: {ServiceID} successfully updated.", dto.ServiceID);

                // Başarılı güncelleme mesajı
                TempData["SuccessMessage"] = "Hizmet başarıyla güncellendi.";
                return RedirectToAction("Services", "Admin");  // Hizmetler listesine yönlendir
            }

            // Eğer ModelState geçersizse, formu tekrar gösteriyoruz
            _logger.LogWarning("ModelState is invalid for ServiceID: {ServiceID}, returning to form.", dto.ServiceID);
            return View(dto);
        }


        public IActionResult Employees()
        {
            if (AdminControl())
            {
                // Tüm çalışanları al
                var employees = _context.Employees
     .Include(e => e.EmployeeServices) // EmployeeServices ilişkisini dahil et
     .ThenInclude(es => es.Service) // Hizmet bilgilerini de dahil et
     .ToList();


                var employeeList = new List<EmployeeRespond>();

                foreach (var employee in employees)
                {
                    // Çalışanın aldığı hizmetlerin adlarını al
                    var serviceNames = (employee.EmployeeServices ?? new List<EmployeeService>())
                                           .Select(es => es.Service.Name)
                                           .ToList();

                    // EmployeeRespond modelini oluştur
                    var employeeRespond = new EmployeeRespond
                    {
                        EmployeeId = employee.EmployeeID,
                        EmployeeName = employee.Name,
                        // Çalışanın aldığı hizmetlerin adlarını ServiceNames listesine atıyoruz
                        ServiceNames = serviceNames
                    };

                    employeeList.Add(employeeRespond);
                }

                return View(employeeList); // Çalışan listesini View'a gönderiyoruz
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }




        [HttpGet]
        public IActionResult AddEmployee()
        {
            _logger.LogInformation("AddEmployee GET request received.");

            if (AdminControl())
            {
                _logger.LogInformation("AdminControl passed, fetching services.");

                var serviceList = _context.Services.Select(h => new SelectListItem
                {
                    Text = h.Name,         // Kullanıcıya gösterilen metin (hizmet adı)
                    Value = h.ServiceID.ToString()
                }).ToList();

                var model = new AdminEmployeeRequest
                {
                    ServiceList = serviceList
                };

                _logger.LogInformation("Returning view with ServiceList.");
                return View(model); // Modeli view'a gönderiyoruz
            }
            else
            {
                _logger.LogWarning("AdminControl failed, redirecting to Home.");
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult AddEmployee(AdminEmployeeRequest request)
        {
            _logger.LogInformation("AddEmployee POST request received.");

            var serviceListInvalid = _context.Services.Select(h => new SelectListItem
            {
                Text = h.Name,         // Kullanıcıya gösterilen metin (hizmet adı)
                Value = h.ServiceID.ToString()
            }).ToList();

            request.ServiceList = serviceListInvalid;  // ServiceList'i tekrar atıyoruz

            
                _logger.LogInformation("ModelState is valid, checking for existing employee.");

                var existingEmployee = _context.Employees
                    .FirstOrDefault(s => s.Name == request.Name);

                if (existingEmployee != null)
                {
                    _logger.LogWarning("Employee already exists with the name: {EmployeeName}", request.Name);
                    ModelState.AddModelError("", "Bu çalışan zaten mevcut.");
                    return View(request); // Hata mesajı ile formu tekrar gösterir
                }

                _logger.LogInformation("Adding new employee: {EmployeeName}", request.Name);

                // Yeni çalışanı ekleyin
                var yeniEmployee = new Employee
                {
                    Name = request.Name,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                _context.Employees.Add(yeniEmployee); // Yeni çalışanı ekle
                _context.SaveChanges(); // Çalışanı kaydet

                // Seçilen hizmetleri ilişkilendir
                foreach (var serviceId in request.ServiceId)
                {
                    var service = _context.Services.FirstOrDefault(s => s.ServiceID == serviceId);
                    if (service != null)
                    {
                        _context.EmployeeServices.Add(new EmployeeService
                        {
                            EmployeeID = yeniEmployee.EmployeeID,
                            ServiceID = service.ServiceID
                        });
                    }
                }

                _context.SaveChanges(); // EmployeeServices tablosuna hizmetleri kaydet

                _logger.LogInformation("Employee added successfully, redirecting to Employees.");
                return RedirectToAction("Employees", "Admin");
            

            _logger.LogWarning("ModelState is not valid, returning the form with errors.");
            return View(request); // Model geçerli değilse, hata ile formu tekrar göster
        }
        [HttpGet]
        public async Task<IActionResult> UpdateEmployee(int id)
        {
            _logger.LogInformation("UpdateEmployee GET işlemi başlatıldı. Çalışan ID: {EmployeeID}", id);

            var employee = await _context.Employees
                .Include(e => e.EmployeeServices)
                .FirstOrDefaultAsync(e => e.EmployeeID == id);

            if (employee == null)
            {
                _logger.LogWarning("UpdateEmployee GET: Çalışan bulunamadı. ID: {EmployeeID}", id);
                return NotFound("Çalışan bulunamadı.");
            }

            var allServices = await _context.Services.ToListAsync();

            var viewModel = new EmployeeUpdateDto
            {
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                SelectedServiceIds = employee.EmployeeServices.Select(es => es.ServiceID).ToList(),
                AllServices = allServices
            };

            _logger.LogInformation("UpdateEmployee GET işlemi tamamlandı. Çalışan adı: {Name}", employee.Name);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(EmployeeUpdateDto model)
        {
            _logger.LogInformation("UpdateEmployee POST işlemi başlatıldı. Çalışan ID: {EmployeeID}", model.EmployeeID);

           
                model.AllServices = await _context.Services.ToListAsync();
                
           

            var employee = await _context.Employees
                .Include(e => e.EmployeeServices)
                .FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeID);

            if (employee == null)
            {
                _logger.LogError("UpdateEmployee POST: Çalışan bulunamadı. ID: {EmployeeID}", model.EmployeeID);
                return NotFound("Çalışan bulunamadı.");
            }

            // Çalışanın adını güncelle
            employee.Name = model.Name;
            _logger.LogInformation("UpdateEmployee POST: Çalışan adı güncellendi. Yeni ad: {Name}", model.Name);

            // Eski hizmetleri kaldır
            _context.EmployeeServices.RemoveRange(employee.EmployeeServices);
            _logger.LogInformation("UpdateEmployee POST: Eski hizmetler kaldırıldı. Çalışan ID: {EmployeeID}", employee.EmployeeID);

            // Yeni hizmetleri ekle
            foreach (var serviceId in model.SelectedServiceIds)
            {
                employee.EmployeeServices.Add(new EmployeeService
                {
                    EmployeeID = employee.EmployeeID,
                    ServiceID = serviceId
                });
            }
            _logger.LogInformation("UpdateEmployee POST: Yeni hizmetler eklendi. Çalışan ID: {EmployeeID}", employee.EmployeeID);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("UpdateEmployee POST işlemi başarıyla tamamlandı. Çalışan ID: {EmployeeID}", employee.EmployeeID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateEmployee POST sırasında bir hata oluştu. Çalışan ID: {EmployeeID}", employee.EmployeeID);
                return StatusCode(500, "Veritabanı işlemi sırasında bir hata oluştu.");
            }

            return RedirectToAction("Employees", "Admin");
        }
        public IActionResult Randevu()
        {
            if (AdminControl())
            {
                var randevular = _context.Appointments.ToList();
                return View(randevular);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
       /* [HttpGet]
        public IActionResult PendingAppointments()
        {
            var pendingAppointments = _context.Appointments
                .Where(a => !a.IsApproved) // Onaylanmamış randevuları al
                .ToList();
            return View(pendingAppointments);
        }*/

      
        public IActionResult ApproveAppointments(int Id)
        {
            var appointment = _context.Appointments.Find(Id);
            if (appointment != null)
            {
                appointment.IsApproved = true; // Randevuyu onayla
                _context.SaveChanges();
            }
            
            
            return RedirectToAction("Randevu", "Admin");
        }
        public IActionResult DeleteAppointment(int Id)
        {
            var silinecekRandevu = _context.Appointments.Find(Id);

            if (silinecekRandevu != null)
            {
                _context.Remove(silinecekRandevu);
                _context.SaveChanges();
                return RedirectToAction("Randevu", "Admin");
            }
            else
            {
                return RedirectToAction("Randevu", "Admin");
            }

        }

        public IActionResult DeleteEmployee(int Id)
        {
            var doktor = _context.Employees.Find(Id);
            _context.Employees.Remove(doktor);
            _context.SaveChanges();

            return RedirectToAction("Employees", "Admin");
        }
        public IActionResult DeleteService(int Id)
        {
            var service = _context.Services.Find(Id);
            _context.Services.Remove(service);
            _context.SaveChanges();

            return RedirectToAction("Services", "Admin");
        }



    }
}
