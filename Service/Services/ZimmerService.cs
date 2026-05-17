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
    public class ZimmerService : IZimmerService
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
        public async Task<List<ZimmerDto>> SearchZimmersAsync(ZimmerSearchDto searchParams)
{
    var allZimmers = await repository.GetAll();
    var allAvailabilities = await availabilityRepository.GetAll();
    var query = allZimmers.AsQueryable();

    if (searchParams.MaxPrice.HasValue && searchParams.MaxPrice > 0)
        query = query.Where(z => z.PricePerNight <= searchParams.MaxPrice);

    if (searchParams.NumOfRooms.HasValue && searchParams.NumOfRooms > 0)
        query = query.Where(z => z.NumRooms >= searchParams.NumOfRooms);

    if (!string.IsNullOrWhiteSpace(searchParams.City))
        query = query.Where(z => z.City.Contains(searchParams.City));

    if (searchParams.HasPool == true)
        query = query.Where(z => z.Facilities.HasFlag(Facility.Pool));

    if (searchParams.HasJacuzzi == true)
        query = query.Where(z => z.Facilities.HasFlag(Facility.Jacuzzi));

    if (searchParams.HasSauna == true)
        query = query.Where(z => z.Facilities.HasFlag(Facility.Sauna));

    var baseFiltered = query.ToList();

    if (string.IsNullOrWhiteSpace(searchParams.FreeText))
        return mapper.Map<List<ZimmerDto>>(baseFiltered);

    var searchText = searchParams.FreeText.ToLower();
    var words = searchText.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

    int? minRooms = null;
    int? maxRooms = null;
    int? minPrice = null;
    int? maxPrice = null;

    for (int i = 0; i < words.Length; i++)
    {
        if (int.TryParse(words[i], out int num))
        {
            bool isRooms = (i + 1 < words.Length && (words[i + 1].StartsWith("חדר") || words[i + 1].StartsWith("חדש")));
            bool isPrice = (i + 1 < words.Length && (words[i + 1].StartsWith("שקל") || words[i + 1].Contains("ש\"ח") || words[i + 1].StartsWith("מחיר")));

            bool min = (i - 1 >= 0 && (words[i - 1].Contains("לפחות") || words[i - 1].Contains("מינימום") || words[i - 1].Contains("מעל")));
            bool max = (i - 1 >= 0 && (words[i - 1].Contains("עד") || words[i - 1].Contains("מקסימום") || words[i - 1].Contains("מתחת")));

            if (isRooms)
            {
                if (min) minRooms = num;
                else if (max) maxRooms = num;
                else maxRooms = num;
            }

            if (isPrice)
            {
                if (min) minPrice = num;
                else if (max) maxPrice = num;
                else maxPrice = num;
            }

            bool isPeople = (i + 1 < words.Length && (words[i + 1].StartsWith("איש") || words[i + 1].StartsWith("אנש") || words[i + 1].StartsWith("נופש") || words[i + 1].StartsWith("אורח") || words[i + 1].StartsWith("מבוג")));
            if (isPeople)
            {
                minRooms = (int)Math.Ceiling((double)num / 4.0);
            }
        }
    }

    var regions = new Dictionary<string, List<string>>
    {
        { "צפון", new List<string> { "צפת", "טבריה", "מירון", "חצור", "ראש פינה", "נהריה", "כרמיאל", "דלתון", "ספסופה", "חרמון","כרמל", "פרדס חנה", "ירכא","בצת", "רמת הגולן והכנרת", "גליל", "גולן", "עמקים", "קצרין", "מעלות" } },
        { "דרום", new List<string> { "אילת", "באר שבע", "ערד", "מצפה רמון", "נתיבות", "אשקלון","ירוחם", "נגב", "אשדוד", "קרית גת", "שדרות" } },
        { "מרכז", new List<string> { "תל אביב", "נתניה", "הרצליה", "ירושלים", "בני ברק", "מודיעין עילית", "גוש דן", "פתח תקוה", "ראשון לציון", "רחובות", "חולון", "רמת גן", "רעננה", "כפר סבא" } }
    };

    var scoredResults = baseFiltered.Select(z =>
    {
        int score = 0;
        string zName = (z.NameZimmer ?? "").ToLower();
        string zCity = (z.City ?? "").ToLower();
        string zDesc = (z.Description ?? "").ToLower();

        if (minRooms.HasValue)
        {
            if (z.NumRooms >= minRooms.Value) score += 40; 
            else if (z.NumRooms >= minRooms.Value - 1) score += 10; 
        }
        if (maxRooms.HasValue && z.NumRooms <= maxRooms.Value) score += 20;
        if (minPrice.HasValue && z.PricePerNight >= minPrice.Value) score += 20;
        if (maxPrice.HasValue && z.PricePerNight <= maxPrice.Value) score += 20;

        foreach (var word in words.Where(w => w.Length > 2))
        {
            bool matched = false;

            if (!matched && zName.Contains(word))
            {
                score += 20;
                matched = true;
            }
            else if (!matched && IsFuzzyMatch(word, zName))
            {
                score += 15;
                matched = true;
            }

            if (!matched && zCity.Contains(word))
            {
                score += 15;
                matched = true;
            }
            else if (!matched && IsFuzzyMatch(word, zCity))
            {
                score += 10;
                matched = true;
            }

            if (!matched && zDesc.Contains(word))
            {
                score += 5;
                matched = true;
            }
            else if (!matched && IsFuzzyMatch(word, zDesc))
            {
                score += 5;
            }
        }

        foreach (var region in regions)
        {
            if ((searchText.Contains(region.Key) || IsFuzzyMatch(region.Key, searchText)) && region.Value.Any(c => zCity.Contains(c.ToLower()) || IsFuzzyMatch(c.ToLower(), zCity)))
                score += 50; 
        }

        return new { Zimmer = z, Score = score };
    })
    .Where(x => x.Score > 0)
    .OrderByDescending(x => x.Score)
    .Select(x => x.Zimmer)
    .ToList();

    if (!scoredResults.Any())
    {
        return mapper.Map<List<ZimmerDto>>(baseFiltered);
    }

    return mapper.Map<List<ZimmerDto>>(scoredResults);
}

        private int CalculateLevenshteinDistance(string s, string t)
        {
            int n = s.Length, m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            if (n == 0) return m;
            if (m == 0) return n;
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            return d[n, m];
        }

        private bool IsFuzzyMatch(string source, string target)
        {
            var sourceWords = source.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var targetWords = target.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var s in sourceWords)
            {
                foreach (var t in targetWords)
                {
                    int distance = CalculateLevenshteinDistance(s, t);

                    if (s.Length <= 4 && distance <= 1)
                    {
                        return true;
                    }
                    else if (s.Length > 4 && s.Length <= 6 && distance <= 2)
                    {
                        return true;
                    }
                    else if (s.Length > 6 && distance <= 3)
                    {
                        return true;
                    }

                    if (t.Contains(s) || s.Contains(t))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task<ZimmerDto> AddItem(ZimmerDto zimmerDto) => mapper.Map<ZimmerDto>(await repository.AddItem(mapper.Map<Zimmer>(zimmerDto)));
        public async Task DeleteItem(int id) => await repository.DeleteItem(id);
        public async Task<List<ZimmerDto>> GetAll() => mapper.Map<List<ZimmerDto>>(await repository.GetAll());
        public async Task<ZimmerDto> GetById(int id) => mapper.Map<ZimmerDto>(await repository.GetById(id));
        public async Task<ZimmerDto> UpdateItem(int id, ZimmerDto zimmerDto) => mapper.Map<ZimmerDto>(await repository.UpdateItem(id, mapper.Map<Zimmer>(zimmerDto)));

        public async Task<List<string>> GetUniqueCitiesAsync()
        {
            var zimmers = await repository.GetAll();
            return zimmers.Select(z => z.City.Trim()).Distinct().OrderBy(c => c).ToList();
        }
        public async Task<List<ZimmerDto>> GetSimilarZimmersAsync(int id)
        {
            var allZimmers = await repository.GetAll();
            var currentZimmer = allZimmers.FirstOrDefault(z => z.ZimmerId == id);

            if (currentZimmer == null)
                return new List<ZimmerDto>();

            var similarZimmers = allZimmers
                .Where(z => z.ZimmerId != id &&
                           (z.City == currentZimmer.City ||
                            Math.Abs(z.PricePerNight - currentZimmer.PricePerNight) <= currentZimmer.PricePerNight * 0.2m))
                .Take(4) 
                .ToList();

            return mapper.Map<List<ZimmerDto>>(similarZimmers);
        }
    }
}