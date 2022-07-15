using DevPartner.Nop.Plugin.CloudStorage.Services;
using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using System;
using System.Linq;

namespace DevPartner.Nop.Plugin.CloudStorage.Data
{
    public class PictureFileObjectContext : DbContext, IDbContext
    {
        public PictureFileObjectContext(DbContextOptions<PictureFileObjectContext> options) : base(options)
        { }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.ApplyConfiguration(new PictureMap());
            modelBuilder.ApplyConfiguration(new MovingItemMap());
            base.OnModelCreating(modelBuilder);
        }

        public virtual string GenerateCreateScript()
        {
            return this.Database.GenerateCreateScript();
        }

        public IQueryable<TQuery> QueryFromSql<TQuery>(string sql) where TQuery : class
        {
            throw new NotImplementedException();
        }

        public IQueryable<TEntity> EntityFromSql<TEntity>(string sql, params object[] parameters) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        public int ExecuteSqlCommand(RawSqlString sql, bool doNotEnsureTransaction = false, int? timeout = null,
            params object[] parameters)
        {
            using (var transaction = this.Database.BeginTransaction())
            {
                var result = this.Database.ExecuteSqlCommand(sql, parameters);
                transaction.Commit();

                return result;
            }
        }

        public void Detach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        public void Install()
        {
            //Database.SetInitializer<PictureFileObjectContext>(null);
            var installation = EngineContext.Current.Resolve<CloudStorageInstallationService>();
            installation.InstallData();
        }

        public void Uninstall()
        {
            Database.ExecuteSqlCommand("DROP TABLE [DP_CloudStorage_Queue]");
            SaveChanges();
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public void Detach(object entity)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TQuery> QueryFromSql<TQuery>(string sql, params object[] parameters) where TQuery : class
        {
            throw new NotImplementedException();
        }
    }
}
