using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Infrastructure.Routing
{
    /// <summary>
    /// Clase responsable de configurar las rutas de la aplicación
    /// </summary>
    public static class RouteConfiguration
    {
        /// <summary>
        /// Configura todas las rutas de la aplicación web
        /// </summary>
        public static void ConfigureRoutes(this WebApplication app)
        {
            // Map Razor Pages
            app.MapRazorPages();
            
            // Fallback to index.html for SPA routes if needed
            // app.MapFallbackToFile("index.html");
        }
    }
}
