namespace Nkv.Interfaces
{
    public interface INkv
    {
        INkvSession BeginSession();
    }
}
