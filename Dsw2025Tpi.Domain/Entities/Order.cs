using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order : EntityBase
    {
        public DateTime Date { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public Order()
        {
            
        }
        public Order(DateTime dateTime, string shippingAddress, string billingAddress, string notes, decimal totalAmount)
        {
            Date = dateTime;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Notes = notes;
            TotalAmount = totalAmount;
        }

    }
}
