namespace Application.Contracts.Persistence.Repositories;

public interface IRepositoryBase
{
    Task<bool> SaveChangesAsync();
    bool HasChangesAsync();
}