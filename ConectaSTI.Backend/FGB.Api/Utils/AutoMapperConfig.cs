using AutoMapper;
using System.Reflection;

namespace FGB.API.Utils
{
    public static class AutoMapperConfig
    {
        public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services)
        {
            var profiles = GetApplicationAssemblies()
                .SelectMany(GetLoadableTypes)
                .Where(type => typeof(Profile).IsAssignableFrom(type) && !type.IsAbstract)
                .Select(type => (Profile)Activator.CreateInstance(type)!)
                .ToArray();

            services.AddAutoMapper(cfg =>
            {
                cfg.AllowNullCollections = true;

                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            });

            return services;
        }

        private static IEnumerable<Assembly> GetApplicationAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => !assembly.IsDynamic)
                .Where(assembly =>
                {
                    var name = assembly.GetName().Name;
                    return !string.IsNullOrWhiteSpace(name) &&
                           (name.StartsWith("FGB", StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith("ConectaSTI", StringComparison.OrdinalIgnoreCase));
                });
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type is not null)!;
            }
        }
    }
}
