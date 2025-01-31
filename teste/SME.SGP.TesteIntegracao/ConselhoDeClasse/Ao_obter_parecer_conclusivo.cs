using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SME.SGP.Aplicacao;
using SME.SGP.Aplicacao.Integracoes.Respostas;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes;
using SME.SGP.TesteIntegracao.Nota.ServicosFakes;
using SME.SGP.TesteIntegracao.Setup;
using Xunit;

namespace SME.SGP.TesteIntegracao.ConselhoDeClasse
{
    public class Ao_obter_parecer_conclusivo : ConselhoDeClasseTesteBase
    {
        public Ao_obter_parecer_conclusivo(CollectionFixture collectionFixture) : base(collectionFixture)
        { }

        [Fact(DisplayName = "Conselho Classe - Deve retornar todos os pareceres conclusivos")]
        public async Task Ao_obter_parecer_conclusivo_deve_retornar_todos()
        {
            var filtroNota = new FiltroConselhoClasseDto()
            {
                Perfil = ObterPerfilProfessorInfantil(),
                Modalidade = Modalidade.EducacaoInfantil,
                TipoCalendario = ModalidadeTipoCalendario.Infantil,
                Bimestre = BIMESTRE_4,
                ComponenteCurricular = COMPONENTE_CURRICULAR_512.ToString(),
                DataAula = DateTimeExtension.HorarioBrasilia().AddDays(-1),
                CriarPeriodoReabertura = false,
            };
            
            await CriarDadosBase(filtroNota);

            var obterPareceresConclusivosUseCase = ServiceProvider.GetService<IObterPareceresConclusivosUseCase>();

            var retorno = await obterPareceresConclusivosUseCase.Executar();
            retorno.ShouldNotBeNull();
            retorno.Count().ShouldBe(6);
        }
    }
}