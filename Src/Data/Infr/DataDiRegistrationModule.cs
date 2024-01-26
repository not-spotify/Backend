using Autofac;
using Autofac.Builder;
using MusicPlayerBackend.Common.Infr;
using MusicPlayerBackend.Data.Repositories;

namespace MusicPlayerBackend.Data.Infr;

public sealed class DataDiRegistrationModule(Func<IRegistrationBuilder<object, object, object>, IRegistrationBuilder<object, object, object>> lifetimeScopeConfigurator)
    : DiRegistrationModuleBase(lifetimeScopeConfigurator)
{
    protected override IEnumerable<IRegistrationBuilder<object, object, object>> RegisterTypesWithDefaultLifetimeScope(ContainerBuilder builder)
    {
        yield return builder.RegisterAssemblyTypes(typeof(EntityRepositoryBase<,>).Assembly).Where(t => t.Name.EndsWith("Repository"))
            .AsImplementedInterfaces();
    }
}
