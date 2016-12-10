#region Usings

using System;
using System.Collections;

#endregion

namespace Wongs.Services.Localization
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The LocaleCollectionWrapper class provides a simple wrapper around a
    /// <see cref="T:Wongs.Services.Localization.LocaleCollection" />.
    /// </summary>
    /// <remarks>
    /// The <see cref="T:Wongs.Services.Localization.LocaleCollection" /> class
    /// implements a custom dictionary class which does not provide for simple databinding.
    /// This wrapper class exposes the individual objects of the underlying dictionary class
    /// thereby allowing for simple databinding to the colleciton of objects.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [Obsolete("Deprecated in DNN 5.0.")]
    public class LocaleCollectionWrapper : IEnumerator, IEnumerable
    {
        private readonly LocaleCollection _locales;
        private int _index;

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Wongs.Services.Localization.LocaleCollectionWrapper" />
        ///  class containing the specified collection <see cref="T:Wongs.Services.Localization.Locale" /> objects.
        /// </summary>
        /// <param name="Locales">A <see cref="T:Wongs.Services.Localization.LocaleCollection" /> object 
        /// which is wrapped by the collection. </param>
        /// <remarks>This overloaded constructor copies the <see cref="T:Wongs.ModuleAction" />s
        ///  from the indicated array.</remarks>
        public LocaleCollectionWrapper(LocaleCollection Locales)
        {
            _locales = Locales;
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return this;
        }

        #endregion

        #region IEnumerator Members

        public object Current
        {
            get
            {
                return _locales[_index];
            }
        }

        public bool MoveNext()
        {
            _index += 1;
            return _index < _locales.Keys.Count;
        }

        public void Reset()
        {
            _index = 0;
        }

        #endregion
    }
}
