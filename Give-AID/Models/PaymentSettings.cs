using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model for managing platform payment channel details editable by Admin
    public class PaymentSettings
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "EasyPaisa account number is required.")]
        [Display(Name = "EasyPaisa Account Number")]
        [StringLength(50)]
        public string EasyPaisaNumber { get; set; } = "0300-1234567";

        [Display(Name = "EasyPaisa Account Title")]
        [StringLength(100)]
        public string EasyPaisaTitle { get; set; } = "GIVE-AID Foundation";

        [Required(ErrorMessage = "JazzCash account number is required.")]
        [Display(Name = "JazzCash Account Number")]
        [StringLength(50)]
        public string JazzCashNumber { get; set; } = "0301-7654321";

        [Display(Name = "JazzCash Account Title")]
        [StringLength(100)]
        public string JazzCashTitle { get; set; } = "GIVE-AID Foundation";

        [Required(ErrorMessage = "Bank name is required.")]
        [Display(Name = "Bank Name")]
        [StringLength(100)]
        public string BankName { get; set; } = "Meezan Bank Limited";

        [Required(ErrorMessage = "Bank account title is required.")]
        [Display(Name = "Account Title")]
        [StringLength(100)]
        public string BankAccountTitle { get; set; } = "GIVE-AID Welfare Trust";

        [Required(ErrorMessage = "Bank account number is required.")]
        [Display(Name = "Account Number")]
        [StringLength(50)]
        public string BankAccountNumber { get; set; } = "0102-0304050607";

        [Display(Name = "IBAN Number")]
        [StringLength(50)]
        public string BankIBAN { get; set; } = "PK12MEZN0001020304050607";

        [Display(Name = "Branch / Code")]
        [StringLength(100)]
        public string? BankBranch { get; set; } = "Main Boulevard Branch, Lahore";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

