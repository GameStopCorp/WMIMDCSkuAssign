using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupplyChain.App.MDCSkuAssign
{
    public class AppSettings
    {
        /// <summary>
        /// Service URI
        /// </summary>
        public string WMIServiceURL { get; set; }

        /// <summary>
        /// Warehouse allocation file regular expression
        /// </summary>
        public string WMIWarehouseAllocFilePattern { get; set; }

        /// <summary>
        /// File destination path
        /// </summary>
        public string FileDestinationPath { get; set; }

        /// <summary>
        /// Connection string to WM
        /// </summary>
        public string WmConnectionString { get; set; }

        /// <summary>
        /// PTL Connection String
        /// </summary>
        public string PtlConnectionString { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int BOLType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CartonType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DistroCompany { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DistroDivision { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DistroStatusFlag { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DistroType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DivisionCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string InventoryType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OrderSfx { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OrderTypeDNS { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string OrderTypeRST { get; set; }

        /// <summary>
        /// GV1 OrderType
        /// </summary>
        public string StoreOrderType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PickTicketCompany { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PickTicketDivision { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PickTicketStatus { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PreStickerCode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SEDDistro { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int SEDPickticket { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SKU100PctInv { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SKUAttrib1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int WholesalePrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string WHSECode { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string WLocation { get; set; }

        /// <summary>
        /// Distro Command
        /// </summary>
        public string DistroCommand { get; set; }

        /// <summary>
        /// Pick Ticket Header Command
        /// </summary>
        public string PickTicketHeaderCommand { get; set; }

        /// <summary>
        /// Pick Ticket Header Command
        /// </summary>
        public string PickTicketDetailCommand { get; set; }

        /// <summary>
        /// Pick ticket update command for non-MUL
        /// </summary>
        public string PickTicketUpdateCommand { get; set; }

        /// <summary>
        /// Pick ticket release command for non-MUL
        /// </summary>
        public string PickTicketReleaseCommand { get; set; }

        /// <summary>
        /// TNT Command
        /// </summary>
        public string TNTCommand { get; set; }

        /// <summary>
        /// Pervasive Security Endpoint
        /// </summary>
        public string PervasiveSecEndpoint { get; set; }

        /// <summary>
        /// Pervasive DB Name
        /// </summary>
        public string PervasiveDBName { get; set; }

        /// <summary>
        /// Pervasive Server
        /// </summary>
        public string PervasiveServer { get; set; }

        /// <summary>
        /// Pervasive User
        /// </summary>
        public string PervasiveUser { get; set; }

        /// <summary>
        /// Pervasive IP
        /// </summary>
        public string PervasiveIP { get; set; }

        /// <summary>
        /// Pervasive DNS
        /// </summary>
        public string PervasiveDns { get; set; }

        /// <summary>
        /// Pervasive User Id
        /// </summary>
        public string PervasiveUserId { get; set; }

        /// <summary>
        /// Pervasive password
        /// </summary>
        public string PervasivePwd { get; set; }

        /// <summary>
        /// Command for pulling batch num from Pervasive
        /// </summary>
        public string BatchNumCommand { get; set; }

        /// <summary>
        /// Command for pulling allocation detail 
        /// </summary>
        public string AllocDetailCommand { get; set; }

        /// <summary>
        /// Starting pick ticket line sequence 
        /// </summary>
        public int PickLineSeqNum { get; set; }

        /// <summary>
        /// LOU e-mail Recipient 
        /// </summary>
        public string LOURecipient { get; set; }

        /// <summary>
        /// GV1 e-mail Recipient 
        /// </summary>
        public string GV1Recipient { get; set; }

        /// <summary>
        /// httpClientTimeout when waiting on response from web service
        /// </summary>
        public double httpClientTimeout { get; set; }

        /// <summary>
        /// GetAllocationDataTimeout when waiting on response from web service
        /// </summary>
        public int GetAllocationDataTimeout { get; set; }

        /// <summary>
        /// Number of retry attempts to check service avaliability
        /// </summary>
        public int RestServiecAvaliableRetryCount { get; set; }
    }
}
