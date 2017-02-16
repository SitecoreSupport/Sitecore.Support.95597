using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.ContentSearch.Linq.Common;
using Sitecore.ContentSearch.Security;
using Sitecore.ContentSearch.SolrProvider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Sitecore.Support.Cintel.Reporting.Aggregate.Visitors.Processors
{
    public class SupportSolrAnalyticsSearchContext : SolrSearchContext
    {
        public SupportSolrAnalyticsSearchContext(SolrSearchIndex index, SearchSecurityOptions options = SearchSecurityOptions.Default)
          : base(index, options)
        {
        }

        public override IQueryable<TItem> GetQueryable<TItem>(params IExecutionContext[] executionContexts)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(Settings.DefaultLanguage);
            List<IExecutionContext> list = ((IEnumerable<IExecutionContext>)executionContexts).Where<IExecutionContext>((Func<IExecutionContext, bool>)(x => !(x is CultureExecutionContext))).ToList<IExecutionContext>();
            list.Add((IExecutionContext)new CultureExecutionContext(cultureInfo));
            return base.GetQueryable<TItem>(list.ToArray());
        }
    }
}
