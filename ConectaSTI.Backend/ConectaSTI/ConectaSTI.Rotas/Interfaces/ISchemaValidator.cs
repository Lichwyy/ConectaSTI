namespace ConectaSTI.Rotas.Interfaces
{
    public interface ISchemaValidator
    {
        List<string> ValidateJsonAgainstSchema(string json, string schema);
    }
}
