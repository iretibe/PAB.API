using Microsoft.EntityFrameworkCore;

namespace PAB.Entity
{
    public class PABContext : DbContext
    {
        public PABContext(DbContextOptions<PABContext> options) : base(options) { }

        public virtual DbSet<psPARContactAddress> psPARContactAddress { get; set; }
        public virtual DbSet<psPARContactEmail> psPARContactEmail { get; set; }
        public virtual DbSet<psPARContactName> psPARContactName { get; set; }
        public virtual DbSet<psPARContactOther> psPARContactOther { get; set; }
        public virtual DbSet<psPARContactPhone> psPARContactPhone { get; set; }
        public virtual DbSet<psPARContactWork> psPARContactWork { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }

            //optionsBuilder.UseSqlServer(
            //    "Server=psl-swd5;Database=PAB;User ID=sa;Password=persol123;Trusted_Connection=False;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<psPARContactAddress>(entity =>
            {
                entity.HasKey(e => e.pkId);

                entity.Property(e => e.pkId)
                    .HasColumnName("pkId")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.IContactNameId).HasColumnName("iContactNameId");

                entity.Property(e => e.SzBusinessAddress)
                    .HasColumnName("szBusinessAddress")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SzHomeAddress)
                    .HasColumnName("szHomeAddress")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SzOther)
                    .HasColumnName("szOther")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContactName)
                    .WithMany(p => p.ContactAddress)
                    .HasForeignKey(d => d.IContactNameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_psPARContactAddress_psPARContactName");
            });

            modelBuilder.Entity<psPARContactEmail>(entity =>
            {
                entity.HasKey(e => e.pkId);

                entity.Property(e => e.pkId)
                    .HasColumnName("pkId")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.IContactNameId).HasColumnName("iContactNameId");

                entity.Property(e => e.SzEmailAddress1)
                    .HasColumnName("szEmailAddress1")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SzEmailAddress2)
                    .HasColumnName("szEmailAddress2")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContactName)
                    .WithMany(p => p.ContactEmail)
                    .HasForeignKey(d => d.IContactNameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_psPARContactEmail_psPARContactName");
            });

            modelBuilder.Entity<psPARContactName>(entity =>
            {
                entity.HasKey(e => e.pkId);

                entity.Property(e => e.pkId)
                    .HasColumnName("pkId")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DCreatedate)
                    .HasColumnName("dCreatedate")
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IUserId).HasColumnName("iUserId");

                entity.Property(e => e.SzFirstName)
                    .IsRequired()
                    .HasColumnName("szFirstName")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SzLastName)
                    .HasColumnName("szLastName")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SzMiddleName)
                    .HasColumnName("szMiddleName")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SzNickName)
                    .HasColumnName("szNickName")
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SzSuffix)
                    .HasColumnName("szSuffix")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SzTitle)
                    .HasColumnName("szTitle")
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<psPARContactOther>(entity =>
            {
                entity.HasKey(e => e.pkId);

                entity.Property(e => e.pkId)
                    .HasColumnName("pkId")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.DAnniversary)
                    .HasColumnName("dAnniversary")
                    .HasColumnType("datetime");

                entity.Property(e => e.DBirthday)
                    .HasColumnName("dBirthday")
                    .HasColumnType("datetime");

                entity.Property(e => e.IContactNameId).HasColumnName("iContactNameId");

                entity.Property(e => e.SzPersonalWebPage)
                    .HasColumnName("szPersonalWebPage")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.SzSignificantOther)
                    .HasColumnName("szSignificantOther")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContactName)
                    .WithMany(p => p.ContactOther)
                    .HasForeignKey(d => d.IContactNameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_psPARContactOther_psPARContactName");
            });

            modelBuilder.Entity<psPARContactPhone>(entity =>
            {
                entity.HasKey(e => e.pkId);

                entity.Property(e => e.pkId)
                    .HasColumnName("pkId")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.IContactNameId).HasColumnName("iContactNameId");

                entity.Property(e => e.SzBusiness)
                    .HasColumnName("szBusiness")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SzBusinessFax)
                    .HasColumnName("szBusinessFax")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SzHome)
                    .HasColumnName("szHome")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SzHomeFax)
                    .HasColumnName("szHomeFax")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SzMobile1)
                    .HasColumnName("szMobile1")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.SzMobile2)
                    .HasColumnName("szMobile2")
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContactName)
                    .WithMany(p => p.ContactPhone)
                    .HasForeignKey(d => d.IContactNameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_psPARContactPhone_psPARContactName");
            });

            modelBuilder.Entity<psPARContactWork>(entity =>
            {
                entity.HasKey(e => e.pkId);

                entity.Property(e => e.pkId)
                    .HasColumnName("pkId")
                    .HasDefaultValueSql("(newid())");

                entity.Property(e => e.IContactNameId).HasColumnName("iContactNameId");

                entity.Property(e => e.SzCompany)
                    .HasColumnName("szCompany")
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.SzJobTitle)
                    .HasColumnName("szJobTitle")
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.HasOne(d => d.ContactName)
                    .WithMany(p => p.ContactWork)
                    .HasForeignKey(d => d.IContactNameId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_psPARContactWork_psPARContactName");
            });
        }

    }
}
