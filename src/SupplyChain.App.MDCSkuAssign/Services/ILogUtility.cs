namespace SupplyChain.App.MDCSkuAssign.Services
{
    public interface ILogUtility
    {
        string SavFilepath { get; }
        void SaveLog();
        bool TryToDelete();
    }
}
