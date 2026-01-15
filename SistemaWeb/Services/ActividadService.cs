using SistemaWeb.Models;

namespace SistemaWeb.Services
{
    public class ActividadService
    {
        private readonly ActividadRepository _repository;
        private readonly ILogService _logger;

        // Dependency Injection: The Service gets the Repository
        public ActividadService(ActividadRepository repository, ILogService logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public List<Actividad> ObtenerTodas()
        {
            return _repository.ObtenerTodas();
        }

        public void Agregar(Actividad actividad)
        {
            // BUSINES LOGIC EXAMPLE (Justifies the Service Layer):
            // If the user forgets to set a status, default to 'Activo'
            if (string.IsNullOrEmpty(actividad.Estado))
            {
                actividad.Estado = "Activo";
            }

            // Logic: If Cupo is 0, force status to 'Lleno'
            if (actividad.Cupo <= 0)
            {
                actividad.Estado = "Lleno";
            }

            _repository.Agregar(actividad);
            _logger.Log($"Nueva actividad creada: {actividad.Nombre}");
        }

        public Actividad ObtenerPorId(string id)
        {
            return _repository.ObtenerPorId(id);
        }

        public void Actualizar(Actividad actividad)
        {
            // Logic: Check Cupo again on update
            if (actividad.Cupo <= 0)
            {
                actividad.Estado = "Lleno";
            }
            _repository.Actualizar(actividad);
        }
    }
}