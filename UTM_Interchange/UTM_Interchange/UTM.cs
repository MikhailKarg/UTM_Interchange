
namespace UTM_Interchange
{
    public class UTM
    {
        public UTM(string fsrar_id, string taxCode, string taxReason, string url, int utmId, string description, bool isActive)
        {
            FSRAR_Id = fsrar_id;
            TaxCode = taxCode;
            TaxReason = taxReason;
            URL = url;
            UTMId = utmId;
            Description = description;
            IsActive = isActive;
        }

        public string FSRAR_Id { get; set; }
        public string TaxCode { get; set; }
        public string TaxReason { get;set; }
        public string URL { get; set; }
        public int UTMId { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
