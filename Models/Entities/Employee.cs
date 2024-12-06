using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }
        public string Name { get; set; }
       
        public int ServiceID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Yabancı anahtarlar
        public ICollection<EmployeeService> EmployeeServices { get; set; } // Çalışanın hizmetleri
    }
}
