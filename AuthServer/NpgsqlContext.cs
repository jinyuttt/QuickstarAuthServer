using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    public class NpgsqlContext : DbContext
    {
        public NpgsqlContext(DbContextOptions<NpgsqlContext> options) : base(options)
        {

        }

        #region 实体集

        public DbSet<Admin> Admin { get; set; }//注意 这里这个Admin不能写成Admins否则会报错找不到Admins 因为我们现在数据库和表是现成的 这里就相当于实体对应的数据库是Admin

        #endregion
    }
}
