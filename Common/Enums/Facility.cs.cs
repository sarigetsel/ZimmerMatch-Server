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
            Pool = 1,               // בריכה
            Parking = 2,            // חניה
            Garden = 4,             // חצר
            Wifi = 8,               // אינטרנט אלחוטי
            Jacuzzi = 16,            // ג'קוזי
            Accessible = 32,         // נגישות
            AirConditioning = 64,    // מיזוג אוויר
            BBQ = 128,                // עמדת על האש
            Kitchen = 256,            // מטבח מאובזר
            Heating = 512,            // חימום
            Playground = 1024,         // משחקיה לילדים
            Seaview = 2048,            // נוף לים (אם קיים)
            PrivateParking = 4096,     // חניה פרטית
            BreakfastIncluded = 8192,  // כולל ארוחת בוקר
            OutdoorSeating = 16384,     // פינת ישיבה חיצונית
            Laundry = 32768             // מכונת כביסה
        }
    
}
