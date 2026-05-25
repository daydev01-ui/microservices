namespace BCP.Notifications.API.Domain.Entities;

public class Notificacion
{
    public Guid IdNotificacion { get; set; } = Guid.NewGuid();
    public Guid? IdUsuario { get; set; }
    public Guid? IdTransaccion { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string Estado { get; set; } = "Pendiente";
    public bool Leida { get; set; } = false;
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}
