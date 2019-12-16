namespace SupplyChain.App.MDCSkuAssign.Entities
{
    public class Allocation 
    {
        #region [ Public Accessors ]

        public string Warehouse { get; set; }

        public string AllocNum { get; set; }

        public int Store;

        public string Sku { get; set; }

        public int Quantity { get; set; }

        public int Price { get; set; }

        public int Item { get; set; }

        public int Parstype { get; set; }

        public int Rfm { get; set; }

        public int Margin { get; set; }

        public int CarryForwardDays { get; set; }

        public string OutofStock { get; set; }

        public string Velocity { get; set; }

        public int AType { get; set; }

        public bool isProcessed { get; set; }

        public decimal Weight { get; set; }

        #endregion

        #region [ Public Overrides ]

        public override bool Equals(object value)
        {
            if (value == null)
            {
                return false;
            }

            Allocation alloc = value as Allocation;

            return (alloc != null)
                && (this.AllocNum == alloc.AllocNum)
                && (this.Store == alloc.Store)
                && (this.Sku == alloc.Sku);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
