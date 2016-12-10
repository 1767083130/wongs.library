
using System;

namespace Wongs.Services.Localization.Internal
{
    public class TestableLocalization : ILocalization
    {

        public static ILocalization Instance
        {
            get
            {
                if (_localization != null)
                {
                    _localization = new TestableLocalization();
                }
                return _localization;
            }
            //return ComponentFactory.GetComponent<CachingProvider>();
        }

        private static ILocalization _localization = null;

        //protected override Func<ILocalization> GetFactory()
        //{
        //    return () => new LocalizationImpl();
        //}

        public string BestCultureCodeBasedOnBrowserLanguages(System.Collections.Generic.IEnumerable<string> cultureCodes, string fallback)
        {
            throw new NotImplementedException();
        }

        public string BestCultureCodeBasedOnBrowserLanguages(System.Collections.Generic.IEnumerable<string> cultureCodes)
        {
            throw new NotImplementedException();
        }

        public System.Globalization.CultureInfo GetPageLocale()
        {
            throw new NotImplementedException();
        }

        public void SetThreadCultures(System.Globalization.CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
    }
}