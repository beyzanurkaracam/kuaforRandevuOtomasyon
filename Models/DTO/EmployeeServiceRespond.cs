﻿namespace kuaforBerberOtomasyon.Models.DTO
{
    public class EmployeeServiceRespond
    {
       
        public String EmployeeName { get; set; } // Enum kullanımı
        public String ServiceName { get; set; }
        public int Duration { get; set; } // Dakika cinsinden
        public decimal Price { get; set; }
    }
}
