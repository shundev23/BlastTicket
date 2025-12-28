using BlastTicket.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlastTicket.Core.Interfaces
{
    public interface IOrderRepository
    {
        //非同期で注文を保持するメソッド
        Task CreateAsync(Order order);
    }
}