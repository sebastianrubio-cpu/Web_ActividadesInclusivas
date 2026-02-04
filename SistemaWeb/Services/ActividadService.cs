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

        public Actividad ObtenerPorId(string id)
        {
            return _repository.ObtenerPorId(id);
        }

        // Modificado para pedir el ID de auditoría
        public void Agregar(Actividad actividad, string idUsuarioAuditoria)
        {
            if (string.IsNullOrEmpty(actividad.Estado)) actividad.Estado = "Activo";
            if (actividad.Cupo <= 0) actividad.Estado = "Lleno";

            _repository.Agregar(actividad, idUsuarioAuditoria);
            _logger.Log($"Nueva actividad creada: {actividad.Nombre} por usuario {idUsuarioAuditoria}");
        }

        public void Actualizar(Actividad actividad, string idUsuarioAuditoria)
        {
            if (actividad.Cupo <= 0) actividad.Estado = "Lleno";
            _repository.Actualizar(actividad, idUsuarioAuditoria);
        }

        public void Eliminar(string codigo, string idUsuarioAuditoria)
        {
            _repository.Eliminar(codigo, idUsuarioAuditoria);
            _logger.Log($"Actividad eliminada: {codigo} por usuario {idUsuarioAuditoria}");
        }
    }
}