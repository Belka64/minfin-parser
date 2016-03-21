using Dapper;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;

namespace Minfin.Dal
{
    public class DapperRepo : IRepository
    {
        private static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["minfinlocaldb"].ConnectionString;

        public void AddRecord(DateTime dealTime, string rank, int sum, string phone, string city, string action, string curency, int bidId, decimal bidRate)
        {
            var q = @"INSERT INTO Records VALUES (@DealTime, @Sum, @Phone, @City, @Action, @Currency, @BidId, @BidRate)";
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                cnn.Execute(q, new
                {
                    DealTime = dealTime,
                    Sum = sum,
                    Phone = phone,
                    City = city,
                    Action = action,
                    Currency = curency,
                    BidId = bidId,
                    BidRate = bidRate
                });
            }
        }

        public bool IsExisted(int bidId)
        {
            var q = @"SELECT * FROM Records WHERE BidId = @BidId";
            Record res = null;
            using (SqlConnection cnn = new SqlConnection(ConnectionString))
            {
                res = cnn.Query<Record>(q, new { BidId = bidId }).FirstOrDefault();
            }

            return res == null ? false : true;
        }
    }
}
