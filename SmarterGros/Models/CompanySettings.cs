namespace SmarterGros.Models
{
    public class CompanySettings
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "SmarterGros";
        public string? CompanyType { get; set; }
        public string? RC { get; set; }
        public string? NIF { get; set; }
        public DateTime? FoundingDate { get; set; }
        public string? LogoPath { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string Currency { get; set; } = "دج";
    }
}