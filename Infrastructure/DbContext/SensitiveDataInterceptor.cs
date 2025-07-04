using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using TranSPEi_Cifrado.Domain.Attributes;
using Microsoft.Extensions.Logging;
using TranSPEi_Cifrado.Application.Common.Services;
using Microsoft.Extensions.Configuration;
using System.Configuration;
namespace TranSPEi_Cifrado.Infrastructure.DbContext
{
    public class SensitiveDataInterceptor : SaveChangesInterceptor, IMaterializationInterceptor
    {
        private readonly ILogger<SensitiveDataInterceptor> _loggerService;
        private readonly IConfiguration _configuration;
        public bool IsEncryptionEnabled;
        public SensitiveDataInterceptor(ILogger<SensitiveDataInterceptor> loggerService, IConfiguration configuration)
        {
            _loggerService = loggerService;
            // Leer el valor de IsEncryptionEnabled desde appsettings.json, con valor por defecto true si no está configurado
            //_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            IsEncryptionEnabled = bool.Parse(configuration["Encryption:Enabled"] ?? "true");


            _loggerService.LogDebug($"SensitiveDataInterceptor inicializado. Cifrado habilitado: {IsEncryptionEnabled}");
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            try
            {
                ProcessSensitiveDataOnSave(eventData.Context);
                return base.SavingChanges(eventData, result);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error en SavingChanges del interceptor.", ex);
                throw;
            }
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            try
            {
                ProcessSensitiveDataOnSave(eventData.Context);
                
                return base.SavingChangesAsync(eventData, result, cancellationToken);
            }
            catch (Exception ex)
            {
                _loggerService.LogError("Error en SavingChangesAsync del interceptor.", ex);
                throw;
            }
        }

        private void ProcessSensitiveDataOnSave(Microsoft.EntityFrameworkCore.DbContext context)
        {
            if (context == null)
            {
                _loggerService.LogWarning("Contexto de base de datos nulo en ProcessSensitiveDataOnSave.");
                return;
            }

            _loggerService.LogDebug("Iniciando ProcessSensitiveDataOnSave para contexto {0}", context.GetType().Name);
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    _loggerService.LogDebug($"Procesando entidad {entry.Entity.GetType().Name} en estado {entry.State}");
                    foreach (var property in entry.Entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)) && property.CanWrite)
                        {
                            var value = property.GetValue(entry.Entity)?.ToString();
                            if (!string.IsNullOrEmpty(value))
                            {
                                // Verificar si el valor ya está en formato Base64
                                if (value.StartsWith("ENC:"))
                                {
                                    _loggerService.LogDebug($"Propiedad {property.Name} de entidad {entry.Entity.GetType().Name} ya está en formato Base64: {value}, omitiendo encriptación.");
                                    continue;
                                }

                                try
                                {
                                    _loggerService.LogDebug($"Encriptando propiedad {property.Name} de entidad {entry.Entity.GetType().Name} con valor {value}");
                                    var encryptedValue = EncryptionService.Instance.Encrypt(value);
                                    //Se omite la persistencia para no afectar flujos
                                    if (IsEncryptionEnabled)
                                    {
                                        property.SetValue(entry.Entity, encryptedValue);
                                    }
                                    else
                                    {
                                        property.SetValue(entry.Entity, value);
                                    }
                                        
                                    _loggerService.LogDebug($"Propiedad ENCRIPTADA : {property.Name} | Valor: {encryptedValue} | Longitud: {encryptedValue.Length}");
                                    var decryptedValue = EncryptionService.Instance.Decrypt(encryptedValue);
                                    _loggerService.LogDebug($"Propiedad DESENCRIPTADA : {property.Name} | Valor: {decryptedValue} | Longitud: {decryptedValue.Length}");

                                }
                                catch (Exception ex)
                                {
                                    _loggerService.LogError($"Error al encriptar propiedad {property.Name} de entidad {entry.Entity.GetType().Name}", ex);
                                    throw;
                                }
                            }
                            else
                            {
                                _loggerService.LogDebug($"Propiedad {property.Name} de entidad {entry.Entity.GetType().Name} está vacía o nula, no se encripta.");
                            }
                        }
                    }
                }
            }
        }

        public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
        {
            if (instance == null)
            {
                _loggerService.LogWarning("Instancia nula en InitializedInstance.");
                return instance;
            }

            var entityType = instance.GetType();
            _loggerService.LogDebug($"Iniciando materialización de entidad {entityType.Name} para desencriptar");

            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)) && property.CanWrite)
                {
                    var value = property.GetValue(instance)?.ToString();
                    _loggerService.LogDebug($"Propiedad {property.Name} encontrada en entidad {entityType.Name}. Valor: {(string.IsNullOrEmpty(value) ? "nulo o vacío" : value)}");

                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!value.StartsWith("ENC:"))
                        {
                            _loggerService.LogWarning($"El valor de la propiedad {property.Name} en entidad {entityType.Name} no está en formato Base64: {value}");
                            continue;
                        }

                        try
                        {
                            _loggerService.LogDebug($"Desencriptando propiedad {property.Name} de entidad {entityType.Name} con valor {value}");
                            var decryptedValue = EncryptionService.Instance.Decrypt(value);
                            property.SetValue(instance, decryptedValue);
                            
                            
                           _loggerService.LogDebug($"Propiedad {property.Name} desencriptada para entidad {entityType.Name}. Valor desencriptado: {decryptedValue}");
                        }
                        catch (InvalidOperationException ex)
                        {
                            _loggerService.LogError($"Error al desencriptar propiedad {property.Name} de entidad {entityType.Name}. Valor: {value}", ex);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _loggerService.LogError($"Error inesperado al desencriptar propiedad {property.Name} de entidad {entityType.Name}. Valor: {value}", ex);
                            throw;
                        }
                    }
                }
                else if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)) && !property.CanWrite)
                {
                    _loggerService.LogWarning($"Propiedad {property.Name} en entidad {entityType.Name} tiene SensitiveDataAttribute pero no es escribible.");
                }
            }

            _loggerService.LogDebug($"Finalizada materialización de entidad {entityType.Name}");
            return instance;
        }
    }
}
