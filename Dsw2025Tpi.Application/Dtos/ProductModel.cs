using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    public class ProductModel
    {
        public record ProductRequest(
            string Sku,
            string InternalCode,
            string Name,
            string Description,
            decimal CurrentUnitPrice,
            int StockQuantity
        );

        public record ProductResponse (
            Guid Id,
            string Sku,
            string InternalCode,
            string Name,
            string Description,
            decimal CurrentUnitPrice,
            int StockQuantity,
            bool isActive
        );

        public record UpdateProductRequest(
            string Name,
            string Description,
            decimal CurrentUnitPrice,
            int StockQuantity
        );

        //admin
        public record FilterProduct (
            string? Status,
            string? Search,
            int? PageNumber,
            int? PageSize);

        public record ResponsePaginatation (
            List<ProductResponse> ProductItems,
            int Total);
    }
}
