using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    public class AdminService : IAdminService
    {
        public NpgsqlContext db;
        public AdminService(NpgsqlContext _efContext)
        {
            db = _efContext;
        }
        /// <summary>
        /// 验证用户，成功则返回用户信息，否则返回null
        /// </summary>
        /// <param name="username"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public async Task<Admin> GetByStr(string username, string pwd)
        {
            
            Admin m = await db.Admin.Where(a => a.UserName == username && a.Password == pwd).SingleOrDefaultAsync();
            if (m != null)
            {
                return m;
            }
            else
            {
                return null;
            }
        }
    }
}
