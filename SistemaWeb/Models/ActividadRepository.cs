using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SistemaWeb.Models
{
    public class ActividadRepository
    {
        private readonly string _folderPath;
        private readonly string _filePath;

        public ActividadRepository(IConfiguration configuration)
        {
            // Definimos la ruta: /app/Data/actividades.json
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), configuration["DataSettings:FilePath"] ?? "Data");
            _filePath = Path.Combine(_folderPath, "actividades.json");

            if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);
        }

        // Método auxiliar para leer y escribir
        private List<Actividad> LeerDatos()
        {
            if (!File.Exists(_filePath)) return new List<Actividad>();
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Actividad>>(json) ?? new List<Actividad>();
        }

        private void GuardarDatos(List<Actividad> datos)
        {
            var json = JsonSerializer.Serialize(datos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public List<Actividad> ObtenerTodas()
        {
            return LeerDatos();
        }

        public Actividad ObtenerPorId(string codigo)
        {
            return LeerDatos().FirstOrDefault(a => a.Codigo == codigo);
        }

        public void Agregar(Actividad actividad, string idUsuarioAuditoria)
        {
            var lista = LeerDatos();
            // Lógica simple para manejar nulos
            actividad.Estado = actividad.Estado ?? "Activo";
            actividad.TipoDiscapacidad = actividad.TipoDiscapacidad ?? "Ninguna";

            lista.Add(actividad);
            GuardarDatos(lista);
        }

        public void Actualizar(Actividad actividad, string idUsuarioAuditoria)
        {
            var lista = LeerDatos();
            var item = lista.FirstOrDefault(a => a.Codigo == actividad.Codigo);
            if (item != null)
            {
                item.Nombre = actividad.Nombre;
                item.FechaRealizacion = actividad.FechaRealizacion;
                item.Cupo = actividad.Cupo;
                item.IdResponsable = actividad.IdResponsable; // Asegúrate de que el modelo tenga esta propiedad
                item.Latitud = actividad.Latitud;
                item.Longitud = actividad.Longitud;
                item.Estado = actividad.Estado;
                item.TipoDiscapacidad = actividad.TipoDiscapacidad;
                // idUsuarioAuditoria se podría loguear aparte si fuera necesario
                GuardarDatos(lista);
            }
        }

        public void Eliminar(string codigo, string idUsuarioAuditoria)
        {
            var lista = LeerDatos();
            var item = lista.FirstOrDefault(a => a.Codigo == codigo);
            if (item != null)
            {
                lista.Remove(item);
                GuardarDatos(lista);
            }
        }

        public dynamic ObtenerEstadisticasGlobales()
        {
            var actividades = LeerDatos().Count;
            // Nota: Para usuarios tendrías que leer el otro JSON, aquí simplifico
            return new { Usuarios = 0, Actividades = actividades };
        }
    }
}