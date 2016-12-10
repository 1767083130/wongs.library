#region Usings

using System;
using System.Data;
using System.Globalization;

using Wongs.Common.Utilities;

#endregion

namespace Wongs.Services.Localization
{
    /// <summary>
    ///   <para>The Locale class is a custom business object that represents a locale, which is the language and country combination.</para>
    /// </summary>
    [Serializable]
    public class Locale 
    {
        public Locale()
        {
        }

        #region Public Properties

        public string Code { get; set; }

        public CultureInfo Culture
        {
            get
            {
                return CultureInfo.CreateSpecificCulture(Code);
            }
        }

        public string EnglishName
        {
            get
            {
                string _Name = string.Empty;
                if (Culture != null)
                {
                    _Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Culture.EnglishName);
                }
                return _Name;
            }
        }

        public string Fallback { get; set; }

        public Locale FallBackLocale { get; set; }

        public bool IsPublished { get; set; }

        public int LanguageId { get; set; }

        public string NativeName { get; set; }

        public int PortalId { get; set; }

        public string Text { get; set; }

        #endregion

	}
}
