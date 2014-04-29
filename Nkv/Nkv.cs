using Nkv.Interfaces;
using System;

namespace Nkv
{
    public sealed class Nkv
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

        public NkvSession BeginSession()
        {
            return new NkvSession(Provider);
        }
    }
}
