using TillasDesktop.Entities.Clientes;
using TillasDesktop.Entities.Usuarios;


namespace TillasDesktop.UI.Modelos
{
    // La clase Cliente representa la estructura de los datos de un cliente. 
    // Hereda de INotifyPropertyChanged para que la interfaz gráfica (WPF) se entere cuando sus valores cambian.
    public class ClienteViewModel : ViewModelBase
    {

        // La entidad de datos original proveniente de la base de datos (modelo puro sin lógica de interfaz).
        private readonly Cliente _clientePuro;

        // Constructor principal: exige una entidad de cliente. Si llega como nula (por ejemplo, para dar de alta uno nuevo), inicializa una instancia vacía.
        public ClienteViewModel(Cliente cliente)
        {
            _clientePuro = cliente ?? new Cliente();
        }


        

        // Propiedad pública para el ID del cliente.
        public int ID
        {
            get => _clientePuro.ID;
            set { _clientePuro.ID = value; OnPropertyChanged(); } // Guarda el nuevo valor y avisa a la interfaz que cambió.
        }

        // Propiedad pública para el Nombre Completo del cliente.
        public string Apellido
        {
            get => _clientePuro.Apellido;
            set { _clientePuro.Apellido = value; OnPropertyChanged(); } // Actualiza el apellido y avisa a la interfaz.
        }

        // Propiedad pública para el Nombre del cliente.
        public string Nombre
        {
            get => _clientePuro.Nombre;
            set { _clientePuro.Nombre = value; OnPropertyChanged(); } // Actualiza el nombre y avisa a la interfaz.
        }

       

        // Propiedad pública para el CUIT del cliente.
        public string CUIT
        {
            get => _clientePuro.CUIT; // Retorna el CUIT actual.
            set { _clientePuro.CUIT = value; OnPropertyChanged(); } // Actualiza el CUIT y avisa a la interfaz.
        }   

        // Propiedad pública para el Teléfono del cliente.
        public string Telefono
        {
            get => _clientePuro.Telefono; // Retorna el teléfono actual.
            set { _clientePuro.Telefono = value; OnPropertyChanged(); } // Actualiza el teléfono y avisa a la interfaz.
        }

        // Propiedad pública para el Email del cliente.
        public string Email
        {
            get => _clientePuro.Email; // Retorna el email actual.
            set { _clientePuro.Email = value; OnPropertyChanged(); } // Actualiza el email y avisa a la interfaz.
        }


        // Devuelve la entidad "limpia" para que pueda ser enviada con seguridad a la capa de negocios (BLL) y luego a la base de datos.
        public Cliente ObtenerEntidadPura()
        {
            return _clientePuro;
        }
    }
}