using Common.Dto;
using System;
using System.Collections.Generic;
using System.Text;


namespace Service.Interfaces
{
    public interface IsExist<T>
    {
        public Task<T> Exist(Login l);
    }
}
