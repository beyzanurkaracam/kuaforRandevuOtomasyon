using kuaforBerberOtomasyon.Models;
using kuaforBerberOtomasyon.Models.DTO;
using kuaforBerberOtomasyon.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Employees()
        {
            if (AdminControl())
            {
                var emploees = _context.Employees.ToList();
                var services = _context.Services.ToList();

                var serviceModelList = new List<EmployeeServiceRespond>();

                foreach (var employee in emploees)
                {
                    var hizmetler = services.FirstOrDefault(abd => abd.ServiceID == employee.ServiceID);

                    var employeeAndService = new EmployeeServiceRespond
                    {
                        DocName = employee.Name,
                        ServiceName = hizmetler?.Name
                    };

                    serviceModelList.Add(employeeAndService);
                }

                return View(serviceModelList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        public IActionResult DoktorEkle()
        {
            if (AdminControl())
            {
                var serviceList = _context.Services.Select(h => new SelectListItem
                {
                    Value = h.ServiceID.ToString(),
                    Text = h.Name
                }).ToList();

                return View(serviceList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpPost]
        public IActionResult DoktorEkle(IFormCollection form)
        {
            // Form verilerini almak için FormCollection nesnesini kullanımı

            string name = form["Name"];
            int selectedServiceId = Convert.ToInt32(form["id"]);
            var newEmployee = new Employee
            {
                Name = name,
                ServiceID = selectedServiceId,
                CreatedAt = DateTime.Now.ToUniversalTime() // UTC'ye dönüştürülür
            };

            _context.Employees.Add(newEmployee); // Yeni servisi ekle
            _context.SaveChanges(); // Değişiklikleri kaydet
           
            return RedirectToAction("Doktor", "Admin");
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
