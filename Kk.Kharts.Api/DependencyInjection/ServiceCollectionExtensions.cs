using System.Reflection;

namespace Kk.Kharts.Api.DependencyInjection;

/// <summary>
/// Extension methods for convention-based service registration.
/// Scans the assembly for interface/implementation pairs following the I{Name} → {Name} convention.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all application services and repositories by convention.
    /// Classes implementing an interface named I{ClassName} are auto-registered as Scoped.
    /// Override lifetime with <see cref="SingletonServiceAttribute"/> or <see cref="TransientServiceAttribute"/>.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var concreteTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
            .ToList();

        foreach (var implementation in concreteTypes)
        {
            var matchingInterface = implementation.GetInterfaces()
                .FirstOrDefault(i => i.Name == $"I{implementation.Name}");

            if (matchingInterface is null)
                continue;

            // Determine lifetime from attribute, default to Scoped
            if (implementation.GetCustomAttribute<SingletonServiceAttribute>() is not null)
                services.AddSingleton(matchingInterface, implementation);
            else if (implementation.GetCustomAttribute<TransientServiceAttribute>() is not null)
                services.AddTransient(matchingInterface, implementation);
            else
                services.AddScoped(matchingInterface, implementation);
        }

        return services;
    }
}

/// <summary>
/// Marks a service implementation for Singleton registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SingletonServiceAttribute : Attribute;

/// <summary>
/// Marks a service implementation for Transient registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TransientServiceAttribute : Attribute;
