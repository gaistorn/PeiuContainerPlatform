using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeiuPlatform.DataAccessor
{
    public static class DataAccessorExtension
    {
       
        public static void AddMySqlDataAccessor(this IServiceCollection services)
        {
            MysqlDataAccessor dataAccessor = null;
#if DEBUG
            string mysql_conn = DataAccessorBase.CreateConnectionString("192.168.0.40", "3306", "power21", "123qwe");
            dataAccessor = new MysqlDataAccessor(mysql_conn);
#else
            dataAccessor = MysqlDataAccessor.CreateDataAccessFromEnvironment();
#endif
            services.AddSingleton<MysqlDataAccessor>(dataAccessor);
        }
    }
}
