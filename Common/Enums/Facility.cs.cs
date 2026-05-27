using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
        [Flags]
        public enum Facility
        {
            Pool = 1,               
            Parking = 2,           
            Garden = 4,             
            Wifi = 8,               
            Jacuzzi = 16,            
            Accessible = 32,         
            AirConditioning = 64,    
            BBQ = 128,                
            Kitchen = 256,            
            Heating = 512,            
            Playground = 1024,         
            Seaview = 2048,            
            PrivateParking = 4096,     
            BreakfastIncluded = 8192,  
            OutdoorSeating = 16384,     
            Laundry = 32768,             
            Sauna = 65536                
        }
    
}
