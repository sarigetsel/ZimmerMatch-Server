using Common.Dto.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Interfaces
{
    public interface IChatService
    {
        Task<object> GetVacationAdviceAsync(UserRequest request);
    }
}
