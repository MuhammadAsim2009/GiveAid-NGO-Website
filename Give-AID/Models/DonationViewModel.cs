using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // View model for the donation form with dummy payment card validation
    public class DonationViewModel
    {
        // The charitable cause selected by the donor
        [Required(ErrorMessage = "Please select a cause to support.")]
        [Display(Name = "Charitable Cause")]
        public int CauseId { get; set; }

        // Optional NGO to direct the donation to
        [Display(Name = "Partner NGO (Optional)")]
        public int? NgoId { get; set; }

        // Amount the donor wishes to contribute
        [Required(ErrorMessage = "Donation amount is required.")]
        [Range(1, 1000000, ErrorMessage = "Amount must be between Rs. 1 and Rs. 1,000,000.")]
        [Display(Name = "Donation Amount (Rs.)")]
        public decimal Amount { get; set; }

        // Cardholder full name as printed on the payment card
        [Required(ErrorMessage = "Cardholder name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Name on Card")]
        public string CardName { get; set; } = string.Empty;

        // Full 16-digit card number (validated then discarded — only last 4 stored)
        [Required(ErrorMessage = "Card number is required.")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Enter a valid card number.")]
        [Display(Name = "Card Number")]
        public string CardNumber { get; set; } = string.Empty;

        // Card expiry in MM/YY format
        [Required(ErrorMessage = "Card expiry date is required.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Enter expiry as MM/YY.")]
        [Display(Name = "Expiry Date (MM/YY)")]
        public string CardExpiry { get; set; } = string.Empty;

        // CVV security code (3 or 4 digits, never stored)
        [Required(ErrorMessage = "CVV security code is required.")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        [Display(Name = "CVV")]
        public string CardCvv { get; set; } = string.Empty;
    }
}

