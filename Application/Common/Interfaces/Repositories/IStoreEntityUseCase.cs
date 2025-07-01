using System.Threading.Tasks;
namespace TranSPEi_Cifrado.Domain.Interfaces.Repositories
{
    public interface IStoreEntityUseCase
    {
        Task<T?> ExecuteAsync<T>(T entity) where T : class;
    }
}