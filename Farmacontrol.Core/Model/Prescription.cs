using Farmacontrol.Model;

namespace Farmacontrol.Core.Model
{
    public class Prescription
    {
        public int Id { get; set; }
        public int SaleCode { get; set; }
        public Sale? Sale { get; set; }
        public string DoctorLicense { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public DateTime IssuedDate { get; set; }
        public string ImageOrFolioReference { get; set; } = string.Empty;
        public DateTime RegisteredAt { get; set; } = DateTime.Now;
        public string? DiagnosisOrNotes { get; set; }
        
        private Prescription() { }
        
        public Prescription(int saleCode, string docLicense, string docName, string patient, DateTime issuedDate, string folio)
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