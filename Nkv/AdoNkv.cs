using Nkv.Interfaces;
using System;

namespace Nkv
{
    public class AdoNkv : INkv
    {
        private IAdoProvider Provider { get; set; }

        public AdoNkv(IAdoProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            Provider = provider;
        }

        public INkvSession BeginSession()
        {
            return new AdoNkvSession(Provider);
        }
    }
}
