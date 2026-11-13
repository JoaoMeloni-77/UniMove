using UniMove.Exceptions;

namespace UniMove.Models;

public class Veiculo
{
    public int Id { get; private set; }
    public string Placa { get; private set; } = string.Empty;
    public string Modelo { get; private set; } = string.Empty;
    public int Capacidade { get; private set; }

    // Usado somente pelo Dapper ao ler do banco. É privado para que
    // nenhum outro código crie um Veiculo vazio (inválido).
    private Veiculo()
    {
    }

    // Usado para criar um veículo novo. O Id é gerado pelo banco.
    public Veiculo(string placa, string modelo, int capacidade)
    {
        Placa = ValidarPlaca(placa);
        Modelo = ValidarModelo(modelo);
        Capacidade = ValidarCapacidade(capacidade);
    }

    // Usado para editar. Passa pelas mesmas validações do construtor.
    public void Atualizar(string placa, string modelo, int capacidade)
    {
        Placa = ValidarPlaca(placa);
        Modelo = ValidarModelo(modelo);
        Capacidade = ValidarCapacidade(capacidade);
    }

    private static string ValidarPlaca(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            throw new RegraNegocioException("A placa do veículo é obrigatória.");

        return placa.Trim();
    }

    private static string ValidarModelo(string modelo)
    {
        if (string.IsNullOrWhiteSpace(modelo))
            throw new RegraNegocioException("O modelo do veículo é obrigatório.");

        return modelo.Trim();
    }

    private static int ValidarCapacidade(int capacidade)
    {
        if (capacidade <= 0)
            throw new RegraNegocioException("A capacidade do veículo deve ser maior que zero.");

        return capacidade;
    }
}