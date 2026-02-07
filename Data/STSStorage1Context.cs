using Microsoft.EntityFrameworkCore;

using STSStorage1.Models;

namespace STSStorage1.Data
{
    public class STSStorage1Context : DbContext
    {
        public STSStorage1Context(DbContextOptions<STSStorage1Context> options)
            : base(options)
        {
        }

        // Regular entity sets
        public DbSet<InvCustomerModel> InventoryCustomer { get; set; } = default!;
        public DbSet<InvClassificationModel> InventoryClassification { get; set; } = default!;
        public DbSet<InvStatusModel> InventoryItemStatus { get; set; } = default!;
        public DbSet<InvPhaseModel> InventoryProjectPhase { get; set; } = default!;
        public DbSet<InvShelfModel> InventoryShelf { get; set; } = default!;
        public DbSet<InvUsersModel> InventoryUsers { get; set; } = default!;
        public DbSet<InvRoleModel> InventoryRole { get; set; } = default!;
        public DbSet<LoginModel> InventoryLogin { get; set; } = default!;
        public DbSet<InvRegisterModel> InventoryRegister { get; set; } = default!;

        // Keyless projection DbSets used for stored procedure mapping
        // Index view uses InvShortTermModel to retrieve the record list from a stored procedure.
        public DbSet<InvShortTermModel> InvShortTerm { get; set; } = default!;

        // Edit view returns a specific record (InventoryRecid) using InvShortTermEditModel.
        public DbSet<InvShortTermEditModel> InvShortTermEdit { get; set; } = default!;

        // CheckOut view uses InvCheckOutModel to retrieve checkout/checkin history from stored procedure
        public DbSet<InvCheckOutModel> InvCheckOut { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Keyless projection for stored-proc mapping (no underlying table/view)
            modelBuilder.Entity<InvShortTermModel>()
                        .HasNoKey()
                        .ToView(null);

            modelBuilder.Entity<InvShortTermEditModel>()
                        .HasNoKey()
                        .ToView(null);

            modelBuilder.Entity<InvCheckOutModel>()
                        .HasNoKey()
                        .ToView(null);

            base.OnModelCreating(modelBuilder);
        }
    }
}
