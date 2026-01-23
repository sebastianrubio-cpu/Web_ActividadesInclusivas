using SistemaWeb.Models;

namespace SistemaWeb.Services
{
    public class ActividadService
    {
        private readonly ActividadRepository _repository;
        private readonly ILogService _logger;

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
            if (string.IsNullOrEmpty(actividad.Estado))
            {
                actividad.Estado = "Activo";
            }

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
            if (actividad.Cupo <= 0)
            {
                actividad.Estado = "Lleno";
            }
            _repository.Actualizar(actividad);
        }

        // 👇 ESTE ES EL MÉTODO NUEVO QUE NECESITAS PARA EL ERROR CS1061 👇
        public void Eliminar(string codigo)
        {
            _repository.Eliminar(codigo);
            _logger.Log($"Actividad eliminada: {codigo}");
        }
    }
}