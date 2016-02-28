using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Minfin.Dal
{
    public class Record
    {
        [Key]
        public int Id { get; set; }

        public string DealTime { get; set; }

        public string Rank { get; set; }

        public string Sum { get; set; }

        public string Phone { get; set; }

        public string City { get; set; }

        public string Action{ get; set; }

        public string Currency { get; set; }

        public int BidId { get; set; }

        public DateTime EstimatedCreatedTime { get; set; }

    }
}
