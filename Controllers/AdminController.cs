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
                var employees = _context.Employees.ToList();
                

                var employeeList = new List<EmployeeRespond>();

                foreach (var employee in employees)
                {
                    
                    var employeeler= new EmployeeRespond
                    {
                        EmployeeId= employee.EmployeeID,
                        EmployeeName = employee.Name,
                        ServiceName = employee.ServiceName
                    };

                    employeeList.Add(employeeler);
                }

                return View(employeeList);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        [HttpGet]
        public IActionResult AddEmployee()
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
        public IActionResult AddEmployee(AdminEmployeeRequest request)
        {
            if (ModelState.IsValid) // Gelen verilerin doğruluğunu kontrol eder
            {
                // Servisin daha önce veritabanına eklenip eklenmediğini kontrol et
                var existingEmployee = _context.Employees
                    .FirstOrDefault(s => s.EmployeeID == request.EmployeeID); // İsimle arama yapabilirsiniz, isteğe bağlı olarak başka parametrelerle de kontrol edilebilir.

                if (existingEmployee != null)
                {
                    // Eğer servis zaten varsa, kullanıcıya mesaj gösterin veya başka bir işlem yapın
                    ModelState.AddModelError("", "Bu çalışan zaten mevcut.");
                    return View(request); // Hata mesajı ile formu tekrar gösterir
                }

                // Servis yeni ise, yeni bir servis oluşturup veritabanına ekleyin
                var yeniEmployee = new Employee
                {
                    Name = request.Name,
                    EmployeeID= request.EmployeeID, 
                    ServiceName = request.ServiceName,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                };

                _context.Employees.Add(yeniEmployee); // Yeni servisi ekle
                _context.SaveChanges(); // Değişiklikleri kaydet

                return RedirectToAction("Doktor", "Admin");
            }

            return View(); // Eğer model geçerli değilse tekrar formu gösterir
           
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
