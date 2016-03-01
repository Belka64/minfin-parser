using System;
using System.ComponentModel.DataAnnotations;

namespace Minfin.Dal
{
    public class Record
    {
        [Key]
        public int Id { get; set; }

        public DateTime DealTime { get; set; }

        public string Rank { get; set; }

        public int Sum { get; set; }

        public string Phone { get; set; }

        public string City { get; set; }

        public string Action{ get; set; }

        public string Currency { get; set; }

        public int BidId { get; set; }

        public decimal BidRate { get; set; }

    }
}
