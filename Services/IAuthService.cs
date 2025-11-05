// Services/IAuthService.cs
using AssetManagement.API.Models;
using System.Threading.Tasks;

namespace AssetManagement.API.Services
{
    public interface IAuthService
    {
        Task<MstUser> RegisterAsync(MstUser user, string password);
        Task<string> LoginAsync(string username, string password);
    }
}