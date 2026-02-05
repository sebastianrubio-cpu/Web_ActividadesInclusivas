using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace SistemaWeb.Models
{
    public class UsuarioRepository
    {
        private readonly string _folderPath;
        private readonly string _filePath;

        public UsuarioRepository(IConfiguration configuration)
        {
            // Define la ruta del archivo JSON
            _folderPath = Path.Combine(Directory.GetCurrentDirectory(), configuration["DataSettings:FilePath"] ?? "Data");
            _filePath = Path.Combine(_folderPath, "usuarios.json");

            // Crear carpeta si no existe
            if (!Directory.Exists(_folderPath)) Directory.CreateDirectory(_folderPath);

            // Crear archivo con usuarios por defecto (Admin y Profesor) si no existe
            if (!File.Exists(_filePath))
            {
                var seed = new List<Usuario> {
                    new Usuario { IdUsuario = "1700000000", Nombre = "Admin", Correo = "admin@uisek.edu.ec", Clave = "12345", IdRol = 1, Rol = "Administrador", IdGenero = 1 },
                    new Usuario { IdUsuario = "1799999999", Nombre = "Profesor", Correo = "profe@uisek.edu.ec", Clave = "12345", IdRol = 2, Rol = "Profesor", IdGenero = 2 }
                };
                GuardarDatos(seed);
            }
        }

        // --- MÉTODOS PRIVADOS DE LECTURA/ESCRITURA ---
        private List<Usuario> LeerDatos()
        {
            if (!File.Exists(_filePath)) return new List<Usuario>();
            try
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<Usuario>>(json) ?? new List<Usuario>();
            }
            catch
            {
                return new List<Usuario>();
            }
        }

        private void GuardarDatos(List<Usuario> datos)
        {
            var json = JsonSerializer.Serialize(datos, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        // --- MÉTODOS PÚBLICOS (Requeridos por los Controladores) ---

        public Usuario ValidarUsuario(string correo, string clave)
        {
            var usuarios = LeerDatos();
            return usuarios.FirstOrDefault(u => u.Correo == correo && u.Clave == clave);
        }

        public bool Registrar(Usuario user)
        {
            var lista = LeerDatos();
            if (lista.Any(u => u.IdUsuario == user.IdUsuario || u.Correo == user.Correo))
                return false;

            // Lógica por defecto para estudiante
            user.IdRol = 3;
            user.Rol = "Estudiante";

            lista.Add(user);
            GuardarDatos(lista);
            return true;
        }

        // Simula la tabla Cat_Roles
        public List<dynamic> ObtenerListaRoles()
        {
            return new List<dynamic>
            {
                new { IdRol = 1, NombreRol = "Administrador" },
                new { IdRol = 2, NombreRol = "Profesor" },
                new { IdRol = 3, NombreRol = "Estudiante" }
            };
        }

        // Simula la tabla Cat_Generos
        public Dictionary<int, string> ObtenerGeneros()
        {
            return new Dictionary<int, string>
            {
                { 1, "Masculino" },
                { 2, "Femenino" },
                { 3, "Otro" }
            };
        }

        public bool CrearUsuarioConRol(Usuario user)
        {
            var lista = LeerDatos();
            if (lista.Any(u => u.IdUsuario == user.IdUsuario || u.Correo == user.Correo))
                return false;

            // Asignar nombre de rol basado en el ID (Simulación de JOIN)
            var roles = ObtenerListaRoles();
            // Truco sucio para obtener el nombre del rol desde la lista dinámica
            string nombreRol = "Estudiante";
            if (user.IdRol == 1) nombreRol = "Administrador";
            if (user.IdRol == 2) nombreRol = "Profesor";

            user.Rol = nombreRol;

            lista.Add(user);
            GuardarDatos(lista);
            return true;
        }

        public Usuario ObtenerPorId(string idUsuario)
        {
            return LeerDatos().FirstOrDefault(u => u.IdUsuario == idUsuario);
        }

        public bool Actualizar(Usuario user)
        {
            var lista = LeerDatos();
            var existente = lista.FirstOrDefault(u => u.IdUsuario == user.IdUsuario);

            if (existente == null) return false;

            // Actualizamos campos
            existente.Nombre = user.Nombre;
            existente.Correo = user.Correo;
            // Solo actualizamos clave si viene con datos, sino mantenemos la anterior
            if (!string.IsNullOrEmpty(user.Clave)) existente.Clave = user.Clave;

            existente.IdRol = user.IdRol;
            existente.IdGenero = user.IdGenero;

            // Actualizar nombre de rol si cambió el ID
            if (existente.IdRol == 1) existente.Rol = "Administrador";
            else if (existente.IdRol == 2) existente.Rol = "Profesor";
            else existente.Rol = "Estudiante";

            GuardarDatos(lista);
            return true;
        }

        public bool Eliminar(string idUsuario)
        {
            var lista = LeerDatos();
            var item = lista.FirstOrDefault(u => u.IdUsuario == idUsuario);
            if (item != null)
            {
                lista.Remove(item);
                GuardarDatos(lista);
                return true;
            }
            return false;
        }

        public List<Usuario> ObtenerTodos()
        {
            return LeerDatos();
        }

        public int ContarUsuarios()
        {
            return LeerDatos().Count;
        }

        public List<Usuario> ObtenerProfesores()
        {
            return LeerDatos().Where(u => u.IdRol == 2).ToList();
        }

        public Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            return LeerDatos().FirstOrDefault(u => u.Correo == correo);
        }

        // Método usado en ActualizarPerfil
        public bool ActualizarPerfilEstudiante(Usuario user)
        {
            // Reutilizamos la lógica de actualizar, pero asegurando que sea estudiante
            // (La validación de dominio @uisek ya estaba en el controller o repo anterior, la mantenemos si gustas)
            if (!user.Correo.ToLower().EndsWith("@uisek.edu.ec")) return false;

            return Actualizar(user);
        }
    }
}