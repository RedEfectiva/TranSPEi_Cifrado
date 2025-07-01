using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Reflection;
using TranSPEi_Cifrado.Domain.Interfaces.External;
using TranSPEi_Cifrado.Application.Common.Services;
using TranSPEi_Cifrado.Domain.Attributes;
namespace TranSPEi_Cifrado.Infrastructure.DbContext
{
    public class SensitiveDataInterceptor : SaveChangesInterceptor, IMaterializationInterceptor
    {
        private readonly ILoggerService _loggerService;

        public SensitiveDataInterceptor( ILoggerService loggerService)
        {
            _loggerService = loggerService;
            _loggerService.Debug("SensitiveDataInterceptor inicializado.");
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
                _loggerService.Error("Error en SavingChanges del interceptor.", ex);
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
                _loggerService.Error("Error en SavingChangesAsync del interceptor.", ex);
                throw;
            }
        }

        private void ProcessSensitiveDataOnSave(Microsoft.EntityFrameworkCore.DbContext context)
        {
            if (context == null)
            {
                _loggerService.Warn("Contexto de base de datos nulo en ProcessSensitiveDataOnSave.");
                return;
            }

            _loggerService.Debug("Iniciando ProcessSensitiveDataOnSave para contexto {0}", context.GetType().Name);
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                {
                    _loggerService.Debug($"Procesando entidad {entry.Entity.GetType().Name} en estado {entry.State}");
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
                                    _loggerService.Debug($"Propiedad {property.Name} de entidad {entry.Entity.GetType().Name} ya está en formato Base64: {value}, omitiendo encriptación.");
                                    continue;
                                }

                                try
                                {
                                    _loggerService.Debug($"Encriptando propiedad {property.Name} de entidad {entry.Entity.GetType().Name} con valor {value}");
                                    var encryptedValue = EncryptionService.Instance.Encrypt(value);
                                    //Se omite la persistencia para no afectar flujos
                                    //property.SetValue(entry.Entity, encryptedValue);
                                    property.SetValue(entry.Entity, value);
                                    _loggerService.Debug($"Propiedad ENCRIPTADA : {property.Name} | Valor: {encryptedValue} | Longitud: {encryptedValue.Length}");
                                    var decryptedValue = EncryptionService.Instance.Decrypt(encryptedValue);
                                    _loggerService.Debug($"Propiedad DESENCRIPTADA : {property.Name} | Valor: {decryptedValue} | Longitud: {decryptedValue.Length}");

                                }
                                catch (Exception ex)
                                {
                                    _loggerService.Error($"Error al encriptar propiedad {property.Name} de entidad {entry.Entity.GetType().Name}", ex);
                                    throw;
                                }
                            }
                            else
                            {
                                _loggerService.Debug($"Propiedad {property.Name} de entidad {entry.Entity.GetType().Name} está vacía o nula, no se encripta.");
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
                _loggerService.Warn("Instancia nula en InitializedInstance.");
                return instance;
            }

            var entityType = instance.GetType();
            _loggerService.Debug($"Iniciando materialización de entidad {entityType.Name} para desencriptar");

            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)) && property.CanWrite)
                {
                    var value = property.GetValue(instance)?.ToString();
                    _loggerService.Debug($"Propiedad {property.Name} encontrada en entidad {entityType.Name}. Valor: {(string.IsNullOrEmpty(value) ? "nulo o vacío" : value)}");

                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!value.StartsWith("ENC:"))
                        {
                            _loggerService.Warn($"El valor de la propiedad {property.Name} en entidad {entityType.Name} no está en formato Base64: {value}");
                            continue;
                        }

                        try
                        {
                            _loggerService.Debug($"Desencriptando propiedad {property.Name} de entidad {entityType.Name} con valor {value}");
                            var decryptedValue = EncryptionService.Instance.Decrypt(value);
                            property.SetValue(instance, decryptedValue);
                            _loggerService.Debug($"Propiedad {property.Name} desencriptada para entidad {entityType.Name}. Valor desencriptado: {decryptedValue}");
                        }
                        catch (InvalidOperationException ex)
                        {
                            _loggerService.Error($"Error al desencriptar propiedad {property.Name} de entidad {entityType.Name}. Valor: {value}", ex);
                            throw;
                        }
                        catch (Exception ex)
                        {
                            _loggerService.Error($"Error inesperado al desencriptar propiedad {property.Name} de entidad {entityType.Name}. Valor: {value}", ex);
                            throw;
                        }
                    }
                }
                else if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)) && !property.CanWrite)
                {
                    _loggerService.Warn($"Propiedad {property.Name} en entidad {entityType.Name} tiene SensitiveDataAttribute pero no es escribible.");
                }
            }

            _loggerService.Debug($"Finalizada materialización de entidad {entityType.Name}");
            return instance;
        }
    }
}
