using System.Globalization;
using System;
using System.Linq;
using System.Reflection;
using TranSPEi_ApiModGes_DbContext.Domain.Attributes;
using TranSPEi_Cifrado.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;


namespace TranSPEi_Cifrado.Application.Common.Services
{
    public class StoreEntityUseCase : IStoreEntityUseCase
    {
        private readonly ILogger<StoreEntityUseCase> _loggerService;
        private readonly IGenericRepository _repository;

        public StoreEntityUseCase(
            IGenericRepository repository,
            ILogger<StoreEntityUseCase> loggerService)
        {
            _repository = repository;
            _loggerService = loggerService;
        }

        public async Task<T?> ExecuteAsync<T>(T entity) where T : class
        {
            try
            {
                // Guardar la entidad usando el repositorio
                var savedEntity = await _repository.StoreAsync(entity);
                if (savedEntity == null)
                {
                    _loggerService.LogWarning($"No se pudo guardar la entidad {typeof(T).Name}.");
                    return null;
                }

                // Descifrar propiedades sensibles antes de retornar
                DecryptSensitiveProperties(savedEntity);

                return savedEntity;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Error al ejecutar StoreEntityUseCase para {typeof(T).Name}.", ex);
                return null;
            }
        }

        private void DecryptSensitiveProperties<T>(T entity) where T : class
        {
            if (entity == null)
            {
                _loggerService.LogWarning("Entidad nula en DecryptSensitiveProperties.");
                return;
            }

            var entityType = typeof(T);
            _loggerService.LogDebug($"Iniciando descifrado de propiedades sensibles para entidad {entityType.Name}");

            foreach (var property in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(property, typeof(SensitiveDataAttribute)) && property.CanWrite)
                {
                    var value = property.GetValue(entity)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Verificar si el valor está en formato Base64
                        if (!value.StartsWith("ENC:"))
                        {
                            _loggerService.LogWarning($"El valor de la propiedad {property.Name} en entidad {entityType.Name} no está en formato Base64: {value}");
                            continue;
                        }

                        try
                        {
                            _loggerService.LogDebug($"Desencriptando propiedad {property.Name} de entidad {entityType.Name} con valor {value}");
                            var decryptedValue = EncryptionService.Instance.Decrypt(value);
                            property.SetValue(entity, decryptedValue);
                            _loggerService.LogDebug($"Propiedad {property.Name} desencriptada. Valor: {decryptedValue}");
                        }
                        catch (Exception ex)
                        {
                            _loggerService.LogError($"Error al desencriptar propiedad {property.Name} de entidad {entityType.Name}. Valor: {value}", ex);
                            throw;
                        }
                    }
                    else
                    {
                        _loggerService.LogDebug($"Propiedad {property.Name} de entidad {entityType.Name} está vacía o nula, no se desencripta.");
                    }
                }
            }
        }     
    }
}
