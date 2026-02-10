using Repository.Entities;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using ZimmerMatch.Interfaces;

namespace Repository.Seeders
{

    public static class ZimmerSeederFull
    {
        public static void Seed(IContext context)
        {
            if (!context.Users.Any(u => u.Role == UserRole.Owner))
            {
                // יוצרים 3 בעלי צימרים לדוגמה
                var owners = new List<User>
            {
                new User { Name = "דניאל", Email = "owner1@mail.com", Password = "123", Role = UserRole.Owner },
                new User { Name = "שרה", Email = "owner2@mail.com", Password = "123", Role = UserRole.Owner },
                new User { Name = "יוסי", Email = "owner3@mail.com", Password = "123", Role = UserRole.Owner }
            };
                context.Users.AddRange(owners);
                context.Save();
            }

            if (context.Zimmers.Any()) return;

            var rnd = new Random();
            var cities = new[] { "דלתון", "ספסופס", "מירון", "ירושלים", "טבריה", "צפת", "ראש פינה", "נהריה", "נתניה", "שדה צבי", "יבניאל" };
            var ownersList = context.Users.Where(u => u.Role == UserRole.Owner).ToList();
            var zimmers = new List<Zimmer>();

            for (int i = 1; i <= 50; i++)
            {
                var city = cities[rnd.Next(cities.Length)];
                var name = $"סוויטות {city} {i}";

                // מתקנים אקראיים
                Facility fac = 0;
                var facilitiesArray = new[] { Facility.Pool, Facility.Jacuzzi, Facility.BBQ, Facility.OutdoorSeating, Facility.Parking };
                foreach (var f in facilitiesArray)
                    if (rnd.Next(0, 2) == 1) fac |= f;

                var rooms = rnd.Next(1, 5);
                var priceMidWeek = rnd.Next(500, 800);
                var priceWeekend = rnd.Next(700, 1200);

                var shortDescription = $"סוויטות {name} במתחם ירוק עם מתקנים: {fac}.";
                var fullDescription = $"צימר {name} ב{city}. מתחם יוקרתי עם {rooms} חדרי שינה, " +
                    $"בריכה {(fac.HasFlag(Facility.Pool) ? "מחוממת ומקורה" : "לא קיימת")}, " +
                    $"ג'קוזי {(fac.HasFlag(Facility.Jacuzzi) ? "חיצוני" : "לא קיים")}, פינות ישיבה, מטבח מאובזר. " +
                    $"מתאים לזוגות ומשפחות. ניתן להזמין ארוחת בוקר בתשלום נפרד. " +
                    $"מחיר: אמצ\"ש החל מ{priceMidWeek}₪, סופ\"ש החל מ{priceWeekend}₪. מתקנים: {fac}.";

                // תמונות placeholder
                var images = new List<string>
            {
                $"/images/zimmer{i % 10 + 1}.jpg",
                $"/images/zimmer{i % 10 + 2}.jpg",
                $"/images/zimmer{i % 10 + 3}.jpg"
            };

                var owner = ownersList[rnd.Next(ownersList.Count)];

                zimmers.Add(new Zimmer
                {
                    OwnerId = owner.Id,
                    NameZimmer = name,
                    City = city,
                    Address = $"{city}, רחוב {i}",
                    NumRooms = rooms,
                    PricePerNight = priceWeekend,
                    CreatedAt = DateTime.Now.AddDays(-rnd.Next(0, 365)),
                    Facilities = fac,
                    Description = fullDescription,
                    ImageUrls = images
                });
            }

            context.Zimmers.AddRange(zimmers);
            context.Save();
        }
    }
}