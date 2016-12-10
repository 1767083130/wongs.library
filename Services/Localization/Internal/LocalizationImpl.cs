
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Wongs.Common;
using System.Web;

namespace Wongs.Services.Localization.Internal
{
    internal class LocalizationImpl : ILocalization
    {
        public string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes, string fallback)
        {
            if(cultureCodes == null)
            {
                throw new ArgumentException("cultureCodes cannot be null");
            }

            if (fallback == null)
            {
                throw new ArgumentException("fallback cannot be null");
            }

            var values = cultureCodes.ToList();
            
            //todo
            foreach (string langHeader in HttpContext.Current.Request.UserLanguages ?? new string[0])
            {
                string lang = langHeader;
                //strip any ;q=xx
                lang = lang.Split(';')[0];

                //check for exact match e.g. de-DE == de-DE
                if (lang.Contains('-'))
                {
                    var match = values.FirstOrDefault(x => x == lang);
                    if(match != null)
                    {
                        return match;
                    }
                }

                //only keep the initial language value
                if (lang.Length > 1)
                {
                    lang = lang.Substring(0, 2);

                    //check for language match e.g. en-GB == en-US because en == en
                    var match = values.FirstOrDefault(x => x.StartsWith(lang));
                    if (match != null)
                    {
                        return match;
                    }
                }
            }

            return fallback;
        }

        public string BestCultureCodeBasedOnBrowserLanguages(IEnumerable<string> cultureCodes)
        {
            return BestCultureCodeBasedOnBrowserLanguages(cultureCodes, Localization.SystemLocale);
        }

        public CultureInfo GetPageLocale()
        {
            return Localization.GetPageLocale();
        }

        public void SetThreadCultures(CultureInfo cultureInfo)
        {
            Localization.SetThreadCultures(cultureInfo);
        }
    }
}