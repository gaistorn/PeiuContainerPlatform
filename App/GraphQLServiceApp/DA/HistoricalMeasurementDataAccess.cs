using GraphQL.Types;
using PeiuPlatform.App.Model;
using PeiuPlatform.DataAccessor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeiuPlatform.App.DA
{
    public interface IHistoricalMeasurementDataAccess
    {

    }
    public class HistoricalMeasurementDataAccess : IHistoricalMeasurementDataAccess
    {
        readonly IInfluxDataAccess influxDataAccess;
        public HistoricalMeasurementDataAccess(IInfluxDataAccess influxDataAccess)
        {
            this.influxDataAccess = influxDataAccess;
        }

        public async Task<HistoricalMeasurementBySite> RequestLatestMinuteHistoryBySite(ResolveFieldContext context, int siteid, int beforemin)
        {
            if (context.ThrowIfInvalidAuthorization() == false)
                return null;

            var sites = context.GetAllSiteIds();
            if(sites.Contains(siteid) == false)
            {
                context.Errors.Add(new GraphQL.ExecutionError("대상 사이트는 소유하지 않았습니다"));
                return null;
            }
            HistoricalMeasurementBySite summary = new HistoricalMeasurementBySite();
            double sumOfChg = await influxDataAccess.Query("sum_chg_1m", "sumOfChg", site, start, end);
        }
    }
}
