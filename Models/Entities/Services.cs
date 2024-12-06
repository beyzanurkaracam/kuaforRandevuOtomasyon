using kuaforBerberOtomasyon.Enums;
using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class Services
    {
        [Key]
        public int ServiceID { get; set; }
        public String Name { get; set; } // Enum kullanımı
        public int Duration { get; set; } // Dakika cinsinden
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Yabancı anahtarlar

        public ICollection<EmployeeService> EmployeeServices { get; set; } // Hizmetin çalışanları
    }
}
