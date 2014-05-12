using Nkv.Interfaces;
using System;

namespace Nkv
{
    public class Nkv : INkv
    {
        private IProvider Provider { get; set; }

        public Nkv(IProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }

            Provider = provider;
        }

        public INkvSession BeginSession()
        {
            return new NkvSession(Provider);
        }
    }
}
