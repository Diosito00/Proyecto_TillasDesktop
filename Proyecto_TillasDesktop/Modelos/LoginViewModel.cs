using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using TillasDesktop.UI.Vistas;

namespace TillasDesktop.UI.Modelos
{
    public class LoginViewModel : ViewModelBase
    {
        private string _usuario;
        public string Usuario
        {
            get => _usuario;
            set { _usuario = value; OnPropertyChanged(); }
        }

        // Retorna el rol del usuario si el login es exitoso, o null si falla
        public string Autenticar(string password)
        {
            string usr = Usuario?.ToLower().Trim();

            if (usr == "vendedor" && password == "123") return "Vendedor";
            if (usr == "gerente" && password == "123") return "Gerente";
            if (usr == "admin" && password == "123") return "Admin";

            return null; // Credenciales incorrectas
        }
    }
}
