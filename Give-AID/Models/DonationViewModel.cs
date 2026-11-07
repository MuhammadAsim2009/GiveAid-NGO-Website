using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Give_AID.Models
{
    // View model for the multi-channel donation form (Card, EasyPaisa, JazzCash, Bank Transfer)
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

        // Selected payment method: "Card", "EasyPaisa", "JazzCash", "BankTransfer"
        [Required(ErrorMessage = "Please select a payment method.")]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Card";

        // --- Card Payment Fields (Required only if PaymentMethod == "Card") ---
        [Display(Name = "Name on Card")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string? CardName { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(19, MinimumLength = 13, ErrorMessage = "Enter a valid card number.")]
        public string? CardNumber { get; set; }

        [Display(Name = "Expiry Date (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Enter expiry as MM/YY.")]
        public string? CardExpiry { get; set; }

        [Display(Name = "CVV")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
        public string? CardCvv { get; set; }

        // --- Manual Payment Fields (EasyPaisa, JazzCash, Bank Transfer) ---
        [Display(Name = "Transaction ID / Reference Number")]
        [StringLength(100, ErrorMessage = "Reference number cannot exceed 100 characters.")]
        public string? TransactionRef { get; set; }

        [Display(Name = "Proof of Payment / Transfer Receipt")]
        public IFormFile? ReceiptFile { get; set; }
    }
}
