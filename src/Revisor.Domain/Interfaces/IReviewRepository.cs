using Revisor.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Revisor.Domain.Interfaces
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task<Review?> GetAsync(string id);
        Task<List<Review>> ListAsync(int limit = 50);
    }
}
