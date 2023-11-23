using Autofac;
using Autofac.Builder;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Repositories;

namespace MusicPlayerBackend;

public sealed class DataDiRegistrationModule(Func<IRegistrationBuilder<object, object, object>, IRegistrationBuilder<object, object, object>> lifetimeScopeConfigurator) 
    : DiRegistrationModuleBase(lifetimeScopeConfigurator)
{
    protected override IEnumerable<IRegistrationBuilder<object, object, object>> RegisterTypesWithDefaultLifetimeScope(ContainerBuilder builder)
    {
        yield return builder.RegisterAssemblyTypes(typeof(EntityRepositoryBase<,>).Assembly).Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces();

        yield return builder.Register(c =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(c.Resolve<IConfiguration>().GetConnectionString(AppDbContext.ConnectionStringName));

            return optionsBuilder.Options;
        }).As<DbContextOptions>();

        yield return builder.RegisterType<AppDbContext>().AsSelf();
        yield return builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();
    }
}