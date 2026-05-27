namespace Farmacontrol.Model
{
    public class Prescription
    {
        public int Id { get; set; }
        public int SaleCode { get; set; }
        public Sale? Sale { get; set; }
        public string DoctorLicense { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string IssuedDate { get; set; } = string.Empty;
        public string ImageOrFolioReference { get; set; } = string.Empty;
        
        private Prescription() { }
        
        public Prescription(int saleCode, string docLicense, string docName, string patient, string issuedDate, string folio)
        {
            SaleCode = saleCode;
            DoctorLicense = docLicense;
            DoctorName = docName;
            PatientName = patient;
            IssuedDate = issuedDate;
            ImageOrFolioReference = folio;
        }
    }
}