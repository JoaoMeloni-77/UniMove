using UniMove.Exceptions;

namespace UniMove.Models;

public class Rota
{
    private readonly List<PontoEmbarque> _pontos = new();

    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public int DuracaoMinutos { get; private set; }

    // Somente leitura: ninguém de fora mexe na lista, só os métodos da Rota
    public IReadOnlyList<PontoEmbarque> Pontos => _pontos.AsReadOnly();

    // Usado pelo Dapper ao ler do banco
    private Rota()
    {
    }

    public Rota(string nome, int duracaoMinutos)
    {
        Nome = ValidarNome(nome);
        DuracaoMinutos = ValidarDuracao(duracaoMinutos);
    }

    public void AlterarDados(string nome, int duracaoMinutos)
    {
        Nome = ValidarNome(nome);
        DuracaoMinutos = ValidarDuracao(duracaoMinutos);
    }

    // A ordem é automática: o novo ponto entra no fim da sequência
    public void AdicionarPonto(string nome, string? referencia)
    {
        if (!string.IsNullOrWhiteSpace(nome) &&
            _pontos.Any(p => string.Equals(p.Nome, nome.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new RegraNegocioException("Já existe um ponto com este nome nesta rota.");

        _pontos.Add(new PontoEmbarque(Id, nome, referencia, _pontos.Count + 1));
    }

    // Remove o ponto e renumera os demais (1, 2, 3...), sem deixar buraco
    public void RemoverPonto(int pontoId)
    {
        var ponto = _pontos.FirstOrDefault(p => p.Id == pontoId);
        if (ponto == null)
            throw new RegraNegocioException("Ponto de embarque não encontrado nesta rota.");

        _pontos.Remove(ponto);

        for (var i = 0; i < _pontos.Count; i++)
            _pontos[i].DefinirOrdem(i + 1);
    }

    // Usado pelo repositório ao montar a rota com os pontos lidos do banco
    internal void CarregarPontos(IEnumerable<PontoEmbarque> pontos)
    {
        _pontos.Clear();
        _pontos.AddRange(pontos);
    }

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome da rota é obrigatório.");

        return nome.Trim();
    }

    private static int ValidarDuracao(int duracaoMinutos)
    {
        if (duracaoMinutos <= 0)
            throw new RegraNegocioException("A duração da rota deve ser maior que zero.");

        return duracaoMinutos;
    }
}