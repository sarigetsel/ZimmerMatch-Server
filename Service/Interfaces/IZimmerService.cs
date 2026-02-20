using Common.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IZimmerService:IService<ZimmerDto>
    {
        Task<List<ZimmerDto>> SearchZimmersAsync(ZimmerSearchDto searchParams);
        Task<List<string>> GetUniqueCitiesAsync();
    }
}
