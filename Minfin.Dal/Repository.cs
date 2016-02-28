using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minfin.Dal
{
    public class Repository
    {
        private MinfinDbContext _context;
        public Repository()
        {
            _context = new MinfinDbContext();
        }
        public void AddRecord(string dealTime, string rank, string sum, string phone, string city,
            string action, string curency, int bidId, DateTime ect)
        {

            _context.Record.Add(new Record() { 
            DealTime = dealTime,
            Rank = rank,
            Sum= sum,
            Phone=phone,
            City = city,
            Action = action,
            Currency = curency,
            BidId = bidId,
            EstimatedCreatedTime = ect
            });
        }
    }
}
