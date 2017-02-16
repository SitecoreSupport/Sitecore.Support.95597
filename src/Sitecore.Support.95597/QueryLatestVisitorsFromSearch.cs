using Sitecore.Analytics.Model;
using Sitecore.Cintel.Configuration;
using Sitecore.Cintel.Reporting;
using Sitecore.Cintel.Reporting.Aggregate.Visitors;
using Sitecore.Cintel.Reporting.Utility;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Analytics.Models;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.SolrProvider;
using Sitecore.ContentSearch.Utilities;
using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Sitecore.Support.Cintel.Reporting.Aggregate.Visitors.Processors
{
    public class QueryLatestVisitorsFromSearch : Sitecore.Cintel.Reporting.Aggregate.Visitors.Processors.QueryLatestVisitorsFromSearch
    {
        private void BuildBaseResult(DataRow row, IndexedContact ic)
        {
            ContactIdentificationLevel result;
            if (!Enum.TryParse<ContactIdentificationLevel>(ic.IdentificationLevel, true, out result))
                result = ContactIdentificationLevel.None;
            row[Schema.ContactIdentificationLevel.Name] = (object)result;
            row[Schema.ContactId.Name] = (object)ic.ContactId;
            row[Schema.FirstName.Name] = (object)ic.FirstName;
            row[Schema.MiddleName.Name] = (object)ic.MiddleName;
            row[Schema.Surname.Name] = (object)ic.Surname;
            row[Schema.EmailAddress.Name] = (object)ic.PreferredEmail;
            row[Schema.Value.Name] = (object)ic.Value;
            row[Schema.VisitCount.Name] = (object)ic.VisitCount;
            row[Schema.ValuePerVisit.Name] = (object)Calculator.GetAverageValue((double)ic.Value, (double)ic.VisitCount);
        }

        private void PopulateLatestVisit(IndexedVisit visit, ref DataRow row)
        {
            row[Schema.LatestVisitValue.Name] = (object)visit.Value;
            row[Schema.LatestVisitStartDateTime.Name] = (object)visit.StartDateTime;
            row[Schema.LatestVisitEndDateTime.Name] = (object)visit.EndDateTime;
            row[Schema.LatestVisitDuration.Name] = (object)Calculator.GetDuration(visit.StartDateTime, visit.EndDateTime);
            if (visit.WhoIs == null)
                return;
            row[Schema.LatestVisitCityDisplayName.Name] = (object)visit.WhoIs.City;
            row[Schema.LatestVisitCountryDisplayName.Name] = (object)visit.WhoIs.Country;
            row[Schema.LatestVisitRegionDisplayName.Name] = (object)visit.WhoIs.Region;
        }

        public override void Process(ReportProcessorArgs args)
        {
            ISearchIndex index = ContentSearchManager.GetIndex(CustomerIntelligenceConfig.ContactSearch.SearchIndexName);
            if (index is SolrSearchIndex)
            {
                IProviderSearchContext ctx = (IProviderSearchContext)new SupportSolrAnalyticsSearchContext(index as SolrSearchIndex, SearchSecurityOptions.Default);
                try
                {
                    int pageSize = args.ReportParameters.PageSize;
                    IQueryable<IndexedContact> collection = ctx.GetQueryable<IndexedContact>().OrderByDescending<IndexedContact, DateTime>((Expression<Func<IndexedContact, DateTime>>)(r => r.LatestVisitDate)).Skip<IndexedContact>((args.ReportParameters.PageNumber - 1) * pageSize).Take<IndexedContact>(pageSize);
                    args.ResultSet.TotalResultCount = ctx.GetQueryable<IndexedContact>().Count<IndexedContact>();
                    Action<IndexedContact> action = (Action<IndexedContact>)(sr =>
                    {
                        DataRow row = args.ResultTableForView.NewRow();
                        this.BuildBaseResult(row, sr);
                        args.ResultTableForView.Rows.Add(row);
                        IndexedVisit visit = ctx.GetQueryable<IndexedVisit>().Where<IndexedVisit>((Expression<Func<IndexedVisit, bool>>)(iv => iv.ContactId == sr.ContactId)).OrderByDescending<IndexedVisit, DateTime>((Expression<Func<IndexedVisit, DateTime>>)(iv => iv.StartDateTime)).Take<IndexedVisit>(1).FirstOrDefault<IndexedVisit>();
                        if (visit == null)
                            return;
                        this.PopulateLatestVisit(visit, ref row);
                    });
                    collection.ForEach<IndexedContact>(action);
                }
                finally
                {
                    if (ctx != null)
                        ctx.Dispose();
                }
            }
            else
                base.Process(args);
        }
    }
}
