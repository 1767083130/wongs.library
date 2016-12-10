using System.Data;

namespace Wongs.Common.Utilities
{
    public interface IHydratable
    {
        int KeyID { get; set; }

        void Fill(IDataReader dr);
    }
}
