namespace TrivaWebPage.Abstractions
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

        Task<int> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        Task<int> CountAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<TEntity>> GetByConditionAsync(
            string whereClause,
            object? parameters = null,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<TEntity> Items, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? whereClause = null,
            object? parameters = null,
            string? orderBy = null,
            CancellationToken cancellationToken = default);
    }
}
