using System.Globalization;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TranSPEi_Cifrado.Domain.Interfaces.External;
using TranSPEi_Cifrado.Domain.Interfaces.Repositories;
using TranSPEi_Cifrado.Domain.Attributes;

namespace TranSPEi_Cifrado.Application.Common.Services
{
    public class StoreEntityUseCase : IStoreEntityUseCase
    {
        private readonly IGenericRepository _repository;
        private readonly ILoggerService _loggerService;

        public StoreEntityUseCase(
            IGenericRepository repository,
            ILoggerService loggerService)
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
                    _loggerService.Warn($"No se pudo guardar la entidad {typeof(T).Name}.");
                    return null;
                }

                // Descifrar propiedades sensibles antes de retornar
                DecryptSensitiveProperties(savedEntity);

                return savedEntity;
            }
            catch (Exception ex)
            {
                _loggerService.Error($"Error al ejecutar StoreEntityUseCase para {typeof(T).Name}.", ex);
                return null;
            }
        }

        private void DecryptSensitiveProperties<T>(T entity) where T : class
        {
            if (entity == null)
            {
                _loggerService.Warn("Entidad nula en DecryptSensitiveProperties.");
                return;
            }

            var entityType = typeof(T);
            _loggerService.Debug($"Iniciando descifrado de propiedades sensibles para entidad {entityType.Name}");

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
                            _loggerService.Warn($"El valor de la propiedad {property.Name} en entidad {entityType.Name} no está en formato Base64: {value}");
                            continue;
                        }

                        try
                        {
                            _loggerService.Debug($"Desencriptando propiedad {property.Name} de entidad {entityType.Name} con valor {value}");
                            var decryptedValue = EncryptionService.Instance.Decrypt(value);
                            property.SetValue(entity, decryptedValue);
                            _loggerService.Debug($"Propiedad {property.Name} desencriptada. Valor: {decryptedValue}");
                        }
                        catch (Exception ex)
                        {
                            _loggerService.Error($"Error al desencriptar propiedad {property.Name} de entidad {entityType.Name}. Valor: {value}", ex);
                            throw;
                        }
                    }
                    else
                    {
                        _loggerService.Debug($"Propiedad {property.Name} de entidad {entityType.Name} está vacía o nula, no se desencripta.");
                    }
                }
            }
        }     
    }
}
