namespace SistemaProveedores.Domain;

public class Proveedor
{
    public int Id { get; set; }
    public string NitEmpresa { get; set; } = string.Empty;
    public string NombreEmpresa { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public string TipoCarga { get; set; } = string.Empty;
    public string EstadoRegistro { get; set; } = string.Empty;

    /// <summary>
    /// Normaliza los campos de texto eliminando espacios en blanco al inicio/final 
    /// y convirtiendo los valores a mayúsculas para garantizar la consistencia de los datos.
    /// </summary>
    public void NormalizarCampos()
    {
        if (!string.IsNullOrEmpty(TipoCarga)) TipoCarga = TipoCarga.Trim().ToUpper();
        if (!string.IsNullOrEmpty(EstadoRegistro)) EstadoRegistro = EstadoRegistro.Trim().ToUpper();
    }

    /// <summary>
    /// Valida que los campos obligatorios del proveedor cumplan con la estructura mínima requerida.
    /// </summary>
    /// <returns>Una tupla donde 'EsValido' indica el éxito de la validación y 'Mensaje' contiene el detalle del error si existe.</returns>
    public (bool EsValido, string Mensaje) ValidarEstructura()
    {
        if (string.IsNullOrWhiteSpace(NitEmpresa))
            return (false, "El NIT de la empresa es obligatorio.");
        if (string.IsNullOrWhiteSpace(NombreEmpresa))
            return (false, "El nombre de la empresa es obligatorio.");

        return (true, string.Empty);
    }
}