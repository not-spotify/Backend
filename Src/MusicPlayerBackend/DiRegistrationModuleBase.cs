using Autofac;
using Autofac.Builder;

namespace MusicPlayerBackend;

public abstract class DiRegistrationModuleBase(Func<IRegistrationBuilder<object, object, object>, IRegistrationBuilder<object, object, object>> lifetimeScopeConfigurator) : Module
{
    protected sealed override void Load(ContainerBuilder builder)
    {
        foreach (var registrationBuilder in RegisterTypesWithDefaultLifetimeScope(builder))
        {
            lifetimeScopeConfigurator(registrationBuilder).PropertiesAutowired();
        }
    }

    protected virtual IEnumerable<IRegistrationBuilder<object, object, object>> RegisterTypesWithDefaultLifetimeScope(ContainerBuilder builder)
    {
        return Enumerable.Empty<IRegistrationBuilder<object, object, object>>();
    }
}