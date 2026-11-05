namespace Give_AID.Models
{
    // View model encapsulating all dynamic sections for the public Home page
    public class HomeViewModel
    {
        // Active welfare causes displayed in the causes carousel/grid
        public List<Causes> FeaturedCauses { get; set; } = new();

        // Upcoming welfare programmes conducted by partner NGOs
        public List<Programmes> UpcomingProgrammes { get; set; } = new();

        // Official corporate and organizational partners
        public List<Partners> Partners { get; set; } = new();

        // Associated partner NGOs
        public List<Ngos> AssociatedNgos { get; set; } = new();

        // Key impact metrics for the statistics counter strip
        public decimal TotalFundsRaised { get; set; }
        public int TotalDonationsCount { get; set; }
        public int TotalMembersCount { get; set; }
        public int TotalNgosCount { get; set; }
    }
}

