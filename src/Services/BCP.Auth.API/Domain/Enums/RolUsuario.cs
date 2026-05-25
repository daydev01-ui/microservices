namespace BCP.Auth.API.Domain.Enums;

public static class RolUsuario
{
    public const string AdministradorSistema = "AdministradorSistema";
    public const string GerenteEmpresarial = "GerenteEmpresarial";
    public const string SupervisorSucursal = "SupervisorSucursal";
    public const string OperadorCaja = "OperadorCaja";

    public static readonly string[] Todos = [
        AdministradorSistema, GerenteEmpresarial, SupervisorSucursal, OperadorCaja
    ];
}
