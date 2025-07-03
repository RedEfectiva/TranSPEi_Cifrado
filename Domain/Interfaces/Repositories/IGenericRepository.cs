using System.Threading.Tasks;
namespace TranSPEi_Cifrado.Domain.Interfaces.Repositories
{
    public interface IGenericRepository
    {
        Task<T?> StoreAsync<T>(T entity) where T : class;
    }
}