namespace Application.Common.Interfaces.Persistence.Repositories;

public interface IRepositoryBase
{
    Task<bool> SaveChangesAsync();
    bool HasChangesAsync();
}