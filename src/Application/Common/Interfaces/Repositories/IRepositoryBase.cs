namespace Application.Common.Interfaces.Repositories;

public interface IRepositoryBase
{
    Task<bool> SaveChangesAsync();
    bool HasChangesAsync();
}