using System;

namespace Minfin.Dal
{
    public interface IRepository
    {
        void AddRecord(DateTime dealTime, string rank, int sum, string phone, string city,
           string action, string curency, int bidId, decimal bidRate);

        bool IsExisted(int bidId);
    }
}