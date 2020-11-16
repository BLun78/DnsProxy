namespace BAG.IT.Core.Wmi.Core
{
    public interface IWmiProvider
    {
        public void ReadData();
        public bool Exists();
    }
}