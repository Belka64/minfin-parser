using System;
using System.Linq;

namespace Minfin.Dal
{
    public class Repository
    {
        private MinfinDbContext _context;
        public Repository()
        {
            _context = new MinfinDbContext();
        }
        public void AddRecord(DateTime dealTime, string rank, int sum, string phone, string city,
            string action, string curency, int bidId)
        {

            _context.Record.Add(new Record()
            {
                DealTime = dealTime,
                Rank = rank,
                Sum = sum,
                Phone = phone,
                City = city,
                Action = action,
                Currency = curency,
                BidId = bidId
            });
            _context.SaveChanges();
        }

        public bool Existed(int bidId)
        {
            if (_context.Set<Record>().FirstOrDefault(x => x.BidId == bidId) != null) return false;
            return true;
        }
    }
}
