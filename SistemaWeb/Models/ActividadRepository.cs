using System.Text.Json;

namespace SistemaWeb.Models
{
    public class ActividadRepository
    {
        private readonly string _filePath;

        public ActividadRepository()
        {
            // El archivo se guardará en la carpeta raíz de la app
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "actividades.json");

            // Si no existe, creamos uno vacío para que no de error
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        // --- MÉTODOS AUXILIARES ---
        private List<Actividad> LeerDatos()
        {
            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<Actividad>();
            return JsonSerializer.Deserialize<List<Actividad>>(json) ?? new List<Actividad>();
        }

        private void GuardarDatos(List<Actividad> actividades)
        {
            var json = JsonSerializer.Serialize(actividades, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        // --- CRUD COMPLETO ---

        public List<Actividad> ObtenerTodas()
        {
            return LeerDatos();
        }

        public Actividad ObtenerPorId(string id)
        {
            var lista = LeerDatos();
            return lista.FirstOrDefault(a => a.Codigo == id);
        }

        public void Agregar(Actividad actividad)
        {
            var lista = LeerDatos();
            lista.Add(actividad);
            GuardarDatos(lista);
        }

        public void Actualizar(Actividad actividadActualizada)
        {
            var lista = LeerDatos();
            var index = lista.FindIndex(a => a.Codigo == actividadActualizada.Codigo);

            if (index != -1)
            {
                lista[index] = actividadActualizada;
                GuardarDatos(lista);
            }
        }
    }
}