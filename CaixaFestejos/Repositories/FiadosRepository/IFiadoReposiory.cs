using CaixaFestejos.Models;

namespace CaixaFestejos.Repositories;

public interface IFiadoRepository
{
    void Registrar(Fiado fiado);

    Fiado? BuscarPorId(int id);

    List<Fiado> Listar();

    List<Fiado> ListarPendentes();

    List<Fiado> ListarQuitados();

    void Quitar(int id, FormaPagamento formaPagamento);

    void Excluir(int id);

    void Zerar();
}