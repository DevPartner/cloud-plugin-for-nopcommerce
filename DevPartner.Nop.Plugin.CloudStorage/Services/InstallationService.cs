using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Installation;

namespace DevPartner.Nop.Plugin.CloudStorage.Services
{
    public class CloudStorageInstallationService : SqlFileInstallationService
    {
        private readonly INopFileProvider _fileProvider;
        public CloudStorageInstallationService(IRepository<Language> languageRepository, IRepository<Customer> customerRepository, IRepository<Store> storeRepository, IDbContext dbContext, IWebHelper webHelper, INopFileProvider fileProvider)
            : base(dbContext,fileProvider,customerRepository,languageRepository,storeRepository,webHelper)
        {
            _fileProvider = fileProvider;
        }

        public virtual void InstallData()
        {
            ExecuteSqlFile(_fileProvider.MapPath("~/Plugins/DevPartner.CloudStorage/SQL/create_script.sql"));
        }

    }
}
