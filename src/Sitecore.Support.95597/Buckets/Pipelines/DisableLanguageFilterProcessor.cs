using Sitecore.Buckets.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Buckets.Pipelines.UI.Search;
using Sitecore.Configuration;
using Sitecore.ContentSearch;
using Sitecore.Data.Items;
using Sitecore.Globalization;

namespace Sitecore.Support.Buckets.Pipelines
{
    public class DisableLanguageFilterProcessor : BucketsPipelineProcessor<UISearchArgs>
    {
        public override void Process(UISearchArgs args)
        {
            Item item = args.StartLocation.Item.Database.GetItem(args.StartLocation.Item.ID, Language.Parse(Settings.DefaultLanguage));
            args.StartLocation = new SitecoreIndexableItem(item);
        }

    }
}