using Autofac;
using Autofac.Builder;

namespace MusicPlayerBackend.Common.Infr;

public abstract class DiRegistrationModuleBase(Func<IRegistrationBuilder<object, object, object>, IRegistrationBuilder<object, object, object>> lifetimeScopeConfigurator) : Module
{
    protected sealed override void Load(ContainerBuilder builder)
    {
        foreach (var registrationBuilder in RegisterTypesWithDefaultLifetimeScope(builder))
        {
            lifetimeScopeConfigurator(registrationBuilder).PropertiesAutowired();
        }
    }

    protected abstract IEnumerable<IRegistrationBuilder<object, object, object>> RegisterTypesWithDefaultLifetimeScope(ContainerBuilder builder);
}
