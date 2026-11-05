namespace Give_AID.Models
{
    // View model encapsulating metrics and recent activities for the Admin Dashboard
    public class AdminDashboardViewModel
    {
        // Core metrics
        public int TotalMembersCount { get; set; }
        public int TotalDonationsCount { get; set; }
        public decimal TotalFundsRaised { get; set; }
        public int OpenQueriesCount { get; set; }
        public int TotalCausesCount { get; set; }
        public int TotalNgosCount { get; set; }
        public int TotalProgrammesCount { get; set; }

        // Recent activity collections
        public List<Donations> RecentDonations { get; set; } = new();
        public List<Queries> RecentQueries { get; set; } = new();
        public List<Users> RecentMembers { get; set; } = new();
    }
}

