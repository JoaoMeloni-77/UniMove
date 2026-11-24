using UniMove.Exceptions;
using UniMove.Models.Enums;

namespace UniMove.Models;

public class Viagem
{
    public int Id { get; private set; }
    public int RotaId { get; private set; }
    public int MotoristaId { get; private set; }
    public int VeiculoId { get; private set; }
    public DateTime DataHoraPartida { get; private set; }
    public TipoViagem Tipo { get; private set; }
    public StatusViagem Status { get; private set; }

    // Usado pelo Dapper ao ler do banco
    private Viagem()
    {
    }

    // Toda viagem nasce Agendada
    public Viagem(int rotaId, int motoristaId, int veiculoId, DateTime dataHoraPartida, TipoViagem tipo)
    {
        Definir(rotaId, motoristaId, veiculoId, dataHoraPartida, tipo);
        Status = StatusViagem.Agendada;
    }

    public void AlterarDados(int rotaId, int motoristaId, int veiculoId, DateTime dataHoraPartida, TipoViagem tipo)
    {
        if (Status != StatusViagem.Agendada)
            throw new RegraNegocioException("Somente viagens agendadas podem ser alteradas.");

        Definir(rotaId, motoristaId, veiculoId, dataHoraPartida, tipo);
    }

    // Agendada -> EmAndamento. Só o motorista da viagem inicia.
    public void Iniciar(int motoristaId)
    {
        if (motoristaId != MotoristaId)
            throw new RegraNegocioException("Somente o motorista da viagem pode iniciá-la.");

        if (Status != StatusViagem.Agendada)
            throw new RegraNegocioException("Somente viagens agendadas podem ser iniciadas.");

        Status = StatusViagem.EmAndamento;
    }

    // EmAndamento -> Finalizada. Só o motorista da viagem finaliza.
    public void Finalizar(int motoristaId)
    {
        if (motoristaId != MotoristaId)
            throw new RegraNegocioException("Somente o motorista da viagem pode finalizá-la.");

        if (Status != StatusViagem.EmAndamento)
            throw new RegraNegocioException("Somente viagens em andamento podem ser finalizadas.");

        Status = StatusViagem.Finalizada;
    }

    // Agendada -> Cancelada
    public void Cancelar()
    {
        if (Status != StatusViagem.Agendada)
            throw new RegraNegocioException("Somente viagens agendadas podem ser canceladas.");

        Status = StatusViagem.Cancelada;
    }

    // Regra usada pela Reserva: só em viagem Agendada e antes da partida.
    // O horário atual vem de fora para a regra poder ser testada.
    public bool PodeReceberReservas(DateTime agora)
    {
        return Status == StatusViagem.Agendada && agora < DataHoraPartida;
    }

    // Fim previsto = partida + duração da rota
    public DateTime CalcularChegadaPrevista(int duracaoMinutos)
    {
        return DataHoraPartida.AddMinutes(duracaoMinutos);
    }

    // Valida tudo primeiro e só depois atribui, para nunca ficar pela metade
    private void Definir(int rotaId, int motoristaId, int veiculoId, DateTime dataHoraPartida, TipoViagem tipo)
    {
        var rota = ValidarId(rotaId, "A rota");
        var motorista = ValidarId(motoristaId, "O motorista");
        var veiculo = ValidarId(veiculoId, "O veículo");
        var partida = ValidarPartida(dataHoraPartida);
        var tipoValido = ValidarTipo(tipo);

        RotaId = rota;
        MotoristaId = motorista;
        VeiculoId = veiculo;
        DataHoraPartida = partida;
        Tipo = tipoValido;
    }

    private static int ValidarId(int id, string campo)
    {
        if (id <= 0)
            throw new RegraNegocioException($"{campo} é obrigatório.");
        return id;
    }

    private static DateTime ValidarPartida(DateTime partida)
    {
        if (partida <= DateTime.Now)
            throw new RegraNegocioException("A data e a hora de partida devem estar no futuro.");
        return partida;
    }

    private static TipoViagem ValidarTipo(TipoViagem tipo)
    {
        if (!Enum.IsDefined(tipo))
            throw new RegraNegocioException("O tipo da viagem é inválido.");
        return tipo;
    }
}