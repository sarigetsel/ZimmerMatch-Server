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

            var baseFiltered = query.ToList();

            if (string.IsNullOrWhiteSpace(searchParams.FreeText))
                return mapper.Map<List<ZimmerDto>>(baseFiltered);

            var searchText = searchParams.FreeText.ToLower();
            var words = searchText.Split(new[] { ' ', ',', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            var regions = new Dictionary<string, List<string>>
            {
                { "צפון", new List<string> { "צפת", "טבריה", "מירון", "חצור", "ראש פינה", "נהריה", "כרמיאל", "דלתון", "ספסופה", "חרמון" } },
                { "דרום", new List<string> { "אילת", "באר שבע", "ערד", "מצפה רמון", "נתיבות", "אשקלון" } },
                { "מרכז", new List<string> { "תל אביב", "נתניה", "הרצליה", "ירושלים", "בני ברק" } }
            };

            var scoredResults = baseFiltered.Select(z =>
            {
                int score = 0;
                string zName = (z.NameZimmer ?? "").ToLower();
                string zCity = (z.City ?? "").ToLower();
                string zDesc = (z.Description ?? "").ToLower();

                foreach (var word in words.Where(w => w.Length > 2))
                {
                    if (zName.Contains(word)) score += 20;
                    if (zCity.Contains(word)) score += 15;
                    if (zDesc.Contains(word)) score += 5;
                    if (CalculateLevenshteinDistance(word, zCity) <= 1) score += 10;
                }

                foreach (var region in regions)
                {
                    if (searchText.Contains(region.Key) && region.Value.Any(c => zCity.Contains(c.ToLower())))
                        score += 30;
                }

                for (int i = 0; i < words.Length; i++)
                {
                    if (int.TryParse(words[i], out int num))
                    {
                        bool isRooms = (i + 1 < words.Length && words[i + 1].Contains("חדר"));
                        bool isPrice = (i + 1 < words.Length && (words[i + 1].Contains("שקל") || words[i + 1].Contains("ש\"ח")));

                        if (isRooms && z.NumRooms >= num) score += 40;
                        if (isPrice && z.PricePerNight <= num) score += 40;
                        if (!isRooms && !isPrice && (z.NumRooms == num || z.PricePerNight <= num)) score += 10;
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
    }
}