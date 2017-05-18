using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Infrastructure.Annotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SC.BL.Domain;

namespace SC.DAL.EF
{
    [DbConfigurationType(typeof(SupportCenterDbConfiguration))]
    internal class SupportCenterDbContext : DbContext 
    {
        public SupportCenterDbContext()
          : base("SupportCenterDB_EFCodeFirst")
        {
            //Database.SetInitializer<SupportCenterDbContext>(new SupportCenterDbInitializer()); // moved to 'SupportCenterDbConfiguration'
        }

        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<HardwareTicket> HardwareTickets { get; set; }
        public DbSet<TicketResponse> TicketResponses { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Entity<Ticket>()
                        .Property(t => t.State)
                        .HasColumnAnnotation("Index", new IndexAnnotation(new IndexAttribute()));
            modelBuilder.Entity<Ticket>().HasKey(t => t.TicketNumber);

        }
    }
}
