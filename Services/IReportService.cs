using System;
using System.Threading.Tasks;

namespace AlphaMaterials.Services
{
    public interface IReportService
    {
        /// <summary>
        /// Генерирует отчет об остатках: колонки
        /// Название, Категория, Поставщик, Остаток
        /// </summary>
        Task<byte[]> GenerateStockReportAsync();

        /// <summary>
        /// Генерирует отчет по операциям с заданными фильтрами:
        /// Дата, Тип, Документ, Сотрудник, Кол-во позиций
        /// </summary>
        Task<byte[]> GenerateOperationsReportAsync(
            DateTime? fromDate,
            DateTime? toDate,
            string? type,
            int? employeeId);
    }
}
