using System.ComponentModel.DataAnnotations;

namespace kuaforBerberOtomasyon.Models.Entities
{
    public class EmployeeService
    {
        [Key]
        public int EmployeeServiceID { get; set; }

        // Yabancı Anahtarlar
        public int EmployeeID { get; set; }
        public Employee Employee { get; set; }

        public int ServiceID { get; set; }
        public Services Service { get; set; }
    }
}
