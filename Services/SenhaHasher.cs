using System.Security.Cryptography;

namespace UniMove.Services;

public static class SenhaHasher
{
    public const string HashProvisorio = "HASH_PROVISORIO";
    public const string SenhaPadrao = "unimove123";

    private const int Iteracoes = 100_000;

    public static string Gerar(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senha, salt, Iteracoes, HashAlgorithmName.SHA256, 32);
        return $"PBKDF2${Iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string senha, string armazenado)
    {
        // O seed e os cadastros novos usam um hash provisório: aceita a senha padrão
        if (armazenado == HashProvisorio)
            return senha == SenhaPadrao;

        var partes = armazenado.Split('$');
        if (partes.Length != 4 || partes[0] != "PBKDF2" || !int.TryParse(partes[1], out var iteracoes))
            return false;

        var salt = Convert.FromBase64String(partes[2]);
        var esperado = Convert.FromBase64String(partes[3]);
        var calculado = Rfc2898DeriveBytes.Pbkdf2(senha, salt, iteracoes, HashAlgorithmName.SHA256, esperado.Length);
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }

    public static bool PrecisaAtualizar(string armazenado) => armazenado == HashProvisorio;
}