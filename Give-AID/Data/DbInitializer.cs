using Give_AID.Models;
using Microsoft.EntityFrameworkCore;

namespace Give_AID.Data
{
    // Class responsible for initializing and seeding initial database data
    public static class DbInitializer
    {
        // Seeds initial Admin user and essential starter data if missing
        public static void SeedData(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Give_AIDContext>();

            try
            {
                // Ensure the database is created / migrations are applied
                // Only seed if Users table exists or can be queried
                if (!context.Users.Any(u => u.Role == "Admin"))
                {
                    // Create default administrator account
                    var adminUser = new Users
                    {
                        Name = "Administrator",
                        Email = "admin@giveaid.org",
                        Password = PasswordHelper.HashPassword("Admin@123"),
                        Role = "Admin",
                        Phone = "0300-1234567",
                        Address = "GIVE-AID Main Secretariat",
                        Occupation = "System Administrator",
                        Organization = "GIVE-AID Welfare Foundation"
                    };

                    context.Users.Add(adminUser);
                    context.SaveChanges();
                }

                // Seed initial causes if none exist
                if (!context.Causes.Any())
                {
                    var initialCauses = new List<Causes>
                    {
                        new Causes { Name = "Child Welfare & Nutrition", Description = "Providing nourishment, shelter, and care to underprivileged and street children.", IsActive = true },
                        new Causes { Name = "Quality Education For All", Description = "Sponsoring school supplies, tuition, and digital classrooms for children in low-income areas.", IsActive = true },
                        new Causes { Name = "Healthcare & Medical Camps", Description = "Offering free clinical checkups, emergency surgeries, and medicines to impoverished families.", IsActive = true },
                        new Causes { Name = "Support For Specially Challenged", Description = "Distributing wheelchairs, assistive devices, and providing vocational training.", IsActive = true },
                        new Causes { Name = "Women Empowerment", Description = "Vocational stitching, micro-business funding, and skill development for independent livelihoods.", IsActive = true },
                        new Causes { Name = "Elderly Care & Support", Description = "Providing medical checkups, dignified living support, and care packages to senior citizens.", IsActive = true }
                    };

                    context.Causes.AddRange(initialCauses);
                    context.SaveChanges();
                }

                // Seed initial static CMS pages if none exist
                if (!context.PageContents.Any())
                {
                    var initialPages = new List<PageContents>
                    {
                        new PageContents
                        {
                            Slug = "what-we-do",
                            Title = "What We Do",
                            Body = "At GIVE-AID, we connect compassionate donors and established welfare NGOs to transform vulnerable lives through direct relief, education, healthcare, and sustainable community empowerment programs."
                        },
                        new PageContents
                        {
                            Slug = "our-mission",
                            Title = "Our Mission & Vision",
                            Body = "Our mission is to eliminate poverty-driven barriers through transparency and collective philanthropy, ensuring every dollar donated delivers measurable human impact."
                        },
                        new PageContents
                        {
                            Slug = "our-team",
                            Title = "Our Leadership Team",
                            Body = "GIVE-AID is steered by passionate humanitarian advocates, experienced social sector leaders, and dedicated volunteers committed to ethical stewardship."
                        },
                        new PageContents
                        {
                            Slug = "career-with-us",
                            Title = "Career With Us",
                            Body = "Join our mission-driven team. Discover meaningful careers and internships in non-profit management, community outreach, and software technologies."
                        },
                        new PageContents
                        {
                            Slug = "our-achievements",
                            Title = "Our Achievements",
                            Body = "Over the past 5 years, we have facilitated care for over 50,000 children, distributed 100,000+ meals, and funded hundreds of student scholarships across partner NGOs."
                        },
                        new PageContents
                        {
                            Slug = "our-supporters",
                            Title = "Our Supporters",
                            Body = "We are deeply grateful to our global network of philanthropic donors, corporate partners, and grassroots volunteers who make our humanitarian initiatives possible."
                        },
                        new PageContents
                        {
                            Slug = "read-about-us",
                            Title = "Read About Us",
                            Body = "Featured in regional and national welfare publications, GIVE-AID continues to set the benchmark for accountable, community-first charitable operations."
                        },
                        new PageContents
                        {
                            Slug = "help-centre",
                            Title = "Help Centre & FAQs",
                            Body = "Have questions regarding donations, tax receipts, or partner NGO verification? Find instant answers or submit an inquiry through our Help Centre query portal."
                        }
                    };

                    context.PageContents.AddRange(initialPages);
                    context.SaveChanges();
                }
            }
            catch
            {
                // If tables do not exist yet (before migrations), silently continue
            }
        }
    }
}

