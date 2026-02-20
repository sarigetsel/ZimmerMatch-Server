using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Common.Dto;
using Common.Enums;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

namespace Service.Services
{
    public class ZimmerService: IZimmerService
    {
        private readonly IRepository<Zimmer> repository;
        private readonly IRepository<Availability> availabilityRepository;
        private readonly IMapper mapper;

        public ZimmerService(IRepository<Zimmer> repository, IRepository<Availability> availabilityRepository, IMapper map)
        {
            this.repository = repository;
            this.availabilityRepository = availabilityRepository;
            this.mapper = map;
        }

        public async Task<ZimmerDto> AddItem(ZimmerDto zimmerDto)
        {
            var created = await repository.AddItem(mapper.Map<Zimmer>(zimmerDto));
            return mapper.Map<Zimmer,ZimmerDto>(created);
        }

        public async Task DeleteItem(int id)
        {
            await repository.DeleteItem(id);
        }

        public async Task<List<ZimmerDto>> GetAll()
        {
            var zimmers = await repository.GetAll();
            return mapper.Map<List<ZimmerDto>>(zimmers);
        }

        public async Task<ZimmerDto> GetById(int id)
        {
            var zimmer =await repository.GetById(id);
            return mapper.Map<ZimmerDto>(zimmer);
        }

        public async Task<ZimmerDto> UpdateItem(int id, ZimmerDto zimmerDto)
        {
            var updateZimmer = await repository.UpdateItem(id, mapper.Map<Zimmer>(zimmerDto));
            return mapper.Map<ZimmerDto>(updateZimmer);
        }

        public async Task<List<ZimmerDto>> SearchZimmersAsync(ZimmerSearchDto searchParams)
        {
            var allZimmers = await repository.GetAll();
            var allAvailabilities = await availabilityRepository.GetAll();
            var query = allZimmers.AsQueryable();

            if (searchParams.FromDate.HasValue && searchParams.ToDate.HasValue)
            {
                var occupiedIds = allAvailabilities
                    .Where(a => a.IsBooked && !(searchParams.ToDate <= a.StartDate || searchParams.FromDate >= a.EndTime))
                    .Select(a => a.ZimmerId).Distinct().ToList();
                query = query.Where(z => !occupiedIds.Contains(z.ZimmerId));
            }

            if (searchParams.MaxPrice.HasValue && searchParams.MaxPrice > 0)
                query = query.Where(z => z.PricePerNight <= searchParams.MaxPrice);

            if (searchParams.NumOfRooms.HasValue && searchParams.NumOfRooms > 0)
                query = query.Where(z => z.NumRooms >= searchParams.NumOfRooms);

            var filteredResults = query.ToList();

            var regions = new Dictionary<string, List<string>>
            {
                { "צפון", new List<string> { "צפת", "טבריה", "מירון", "חצור", "ראש פינה", "נהריה", "כרמיאל", "קרית שמונה","דלתון","ספסופה","חרמון","קריית שמונה"} },
                { "דרום", new List<string> { "אילת", "באר שבע", "ערד", "מצפה רמון", "נתיבות", "אשקלון","שדה צבי" } },
                { "מרכז", new List<string> { "תל אביב", "נתניה", "הרצליה", "ראשון לציון",
                    "פתח תקווה", "ירושלים","בני ברק","מודיעין","חשמונאים","מתיתיהו","הר שמואל" } }
            };

            if (!string.IsNullOrWhiteSpace(searchParams.FreeText))
            {
                var searchText = searchParams.FreeText.ToLower();
                var searchWords = searchText.Split(new[] { ' ', ',', '.' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                var numbersInText = searchWords
                    .Where(w => int.TryParse(w, out _))
                    .Select(int.Parse)
                    .ToList();

                bool hasRoomKeyword = searchWords.Any(w => w.Contains("חדר") || w.Contains("מיטות") || w.Contains("חדרים"));
                bool hasPriceKeyword = searchWords.Any(w => w.Contains("מחיר") || w.Contains("שח") || w.Contains("עלות"));

                var scoredResults = filteredResults.Select(z =>
                {
                    int score = 0;
                    string name = (z.NameZimmer ?? "").ToLower();
                    string city = (z.City ?? "").ToLower();
                    string description = (z.Description ?? "").ToLower();

                    var zimmerNameWords = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in searchWords.Where(w => w.Length >= 3))
                    {
                        if (name.Contains(word)) score += 5;
                        if (city.Contains(word)) score += 3;
                        if (description.Contains(word)) score += 1;


                        if (CalculateLevenshteinDistance(word, city) <= 1) score += 4;

                        foreach (var zWord in zimmerNameWords)
                        {
                            if (CalculateLevenshteinDistance(word, zWord) <= 1)
                            {
                                score += 3;
                                break; 
                            }
                        }
                    }

                    foreach (var region in regions)
                    {
                        if (searchText.Contains(region.Key) && region.Value.Contains(z.City))
                            score += 10;
                    }

                    foreach (var num in numbersInText)
                    {
                        if (z.NumRooms == num)
                        {
                            score += 20;
                        }
                        else if (hasRoomKeyword && z.NumRooms >= num && z.NumRooms <= num + 1)
                        {
                            score += 10; 
                        }

                        if (num >= 100)
                        {
                            if (z.PricePerNight <= num) score += 15;
                            else if (hasPriceKeyword && z.PricePerNight <= num + 200) score += 5;
                        }
                    }

                    return new { Zimmer = z, Score = score };
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .Select(x => x.Zimmer)
                .ToList();

                return mapper.Map<List<ZimmerDto>>(scoredResults);
            }

            return mapper.Map<List<ZimmerDto>>(filteredResults);
        }

        // אלגוריתם תכנון דינמי לחישוב מרחק לבנשטיין בין מחרוזות
        private int CalculateLevenshteinDistance(string source, string target)
        {
            if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
            if (string.IsNullOrEmpty(target)) return source.Length;

            int n = source.Length;
            int m = target.Length;
            int[,] distance = new int[n + 1, m + 1];

            for (int i = 0; i <= n; distance[i, 0] = i++) ;
            for (int j = 0; j <= m; distance[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }
            return distance[n, m];
        }

        public async Task<List<string>> GetUniqueCitiesAsync()
        {
            var allZimmers = await repository.GetAll();
            return allZimmers
                .Where(z => !string.IsNullOrWhiteSpace(z.City))
                .Select(z => z.City.Trim())
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }

    }

}
