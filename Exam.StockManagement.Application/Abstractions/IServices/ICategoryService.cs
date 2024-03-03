using Exam.StockManagement.Domain.Entities.Models;

namespace Exam.StockManagement.Application.Abstractions.IServices
{
    public interface ICategoryService
    {
        public Task<string> Create();
        public Task<List<Category>> GetAll();
        public Task<string> Update();
        public Task<string> Delete();
    }
}
