﻿using System.Collections.Generic;
using SME.SGP.Dominio;
using SME.SGP.Infra;
using SME.SGP.TesteIntegracao.Setup;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SME.SGP.Aplicacao;
using SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes;
using Xunit;
using SME.SGP.TesteIntegracao.ServicosFakes;
using ObterTurmaItinerarioEnsinoMedioQueryHandlerFake = SME.SGP.TesteIntegracao.ServicosFakes.ObterTurmaItinerarioEnsinoMedioQueryHandlerFake;
using ObterAlunosAtivosPorTurmaCodigoQueryHandlerFake = SME.SGP.TesteIntegracao.ConselhoDeClasse.ServicosFakes.ObterAlunosAtivosPorTurmaCodigoQueryHandlerFake;

namespace SME.SGP.TesteIntegracao.ConselhoDeClasse
{
    public class Ao_inserir_nota_conceito_pos_conselho_bimestre_final : ConselhoDeClasseTesteBase
    {
        public Ao_inserir_nota_conceito_pos_conselho_bimestre_final(CollectionFixture collectionFixture) : base(collectionFixture)
        {
        }
        
        protected override void RegistrarFakes(IServiceCollection services)
        {
            base.RegistrarFakes(services);
            
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterComponentesCurricularesEOLPorTurmasCodigoQuery, IEnumerable<ComponenteCurricularEol>>), typeof(ObterComponentesCurricularesEOLPorTurmasCodigoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterTurmaItinerarioEnsinoMedioQuery, IEnumerable<TurmaItinerarioEnsinoMedioDto>>), typeof(ObterTurmaItinerarioEnsinoMedioQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunosAtivosPorTurmaCodigoQuery, IEnumerable<AlunoPorTurmaResposta>>), typeof(ObterAlunosAtivosPorTurmaCodigoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ProfessorPodePersistirTurmaQuery, bool>), typeof(ProfessorPodePersistirTurmaQueryHandlerComPermissaoFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterComponentesCurricularesRegenciaPorAnoETurnoQuery, IEnumerable<DisciplinaDto>>), typeof(ObterComponentesCurricularesRegenciaPorAnoETurnoQueryFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterComponentesRegenciaPorAnoEolQuery, IEnumerable<ComponenteCurricularEol>>), typeof(ObterComponentesRegenciaPorAnoQueryHandlerFake), ServiceLifetime.Scoped));
            services.Replace(new ServiceDescriptor(typeof(IRequestHandler<ObterAlunoPorTurmaAlunoCodigoQuery, AlunoPorTurmaResposta>), typeof(ObterAlunoPorTurmaAlunoCodigoQueryHandlerFake), ServiceLifetime.Scoped));
        }

        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Deve_lancar_nota_conceito_pos_conselho_bimestre_final(bool anoAnterior)
        {
            await CriarDados(ObterPerfilProfessor(), 
                             COMPONENTE_CURRICULAR_PORTUGUES_ID_138, 
                             TipoNota.Conceito, 
                             ANO_3, 
                             Modalidade.Fundamental, 
                             ModalidadeTipoCalendario.FundamentalMedio, 
                             anoAnterior, 
                             SituacaoConselhoClasse.EmAndamento, 
                             true);
            
            await CriarConselhoClasseTodosBimestres(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Conceito);
            
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138, TipoNota.Conceito, FECHAMENTO_TURMA_ID_5, BIMESTRE_FINAL);
            
            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Conceito);
            
        }
        
        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Deve_lancar_nota_conceito_pos_conselho_bimestre_regencia_fundamental_final(bool anoAnterior)
        {
            await CriarDados(ObterPerfilProfessor(), 
                COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105, 
                TipoNota.Conceito, 
                ANO_3, 
                Modalidade.Fundamental, 
                ModalidadeTipoCalendario.FundamentalMedio, 
                anoAnterior, 
                SituacaoConselhoClasse.EmAndamento, 
                true);
            
            await CriarConselhoClasseTodosBimestres(COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105,TipoNota.Conceito);
            
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105, TipoNota.Conceito, FECHAMENTO_TURMA_ID_5, BIMESTRE_FINAL);
            
            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Conceito, componentesRegencia: COMPONENTE_REGENCIA_CLASSE_FUND_I_5H_ID_1105);
        }
        
        [Theory]
        [InlineData(false)]
        //[InlineData(true)]
        public async Task Deve_lancar_nota_conceito_pos_conselho_bimestre_regencia_EJA_final(bool anoAnterior)
        {
            await CriarDados(ObterPerfilProfessor(), 
                COMPONENTE_CURRICULAR_PORTUGUES_ID_138, 
                TipoNota.Conceito, 
                ANO_1, 
                Modalidade.EJA, 
                ModalidadeTipoCalendario.EJA, 
                anoAnterior, 
                SituacaoConselhoClasse.EmAndamento, 
                true);
            
            await CriarConselhoClasseTodosBimestres(COMPONENTE_CURRICULAR_PORTUGUES_ID_138,TipoNota.Conceito);
            
            var salvarConselhoClasseAlunoNotaDto = ObterSalvarConselhoClasseAlunoNotaDto(COMPONENTE_CURRICULAR_PORTUGUES_ID_138, TipoNota.Conceito, FECHAMENTO_TURMA_ID_5, BIMESTRE_FINAL);
            
            await ExecutarTeste(salvarConselhoClasseAlunoNotaDto, anoAnterior, TipoNota.Conceito);
        }

        private async Task CriarDados(string perfil, long componente, TipoNota tipo, string anoTurma, Modalidade modalidade, ModalidadeTipoCalendario modalidadeTipoCalendario, bool anoAnterior, SituacaoConselhoClasse situacaoConselhoClasse = SituacaoConselhoClasse.NaoIniciado, bool criarFechamentoDisciplinaAlunoNota = false)
        {
            var dataAula = anoAnterior ? DATA_03_10_INICIO_BIMESTRE_4.AddYears(-1) : DATA_03_10_INICIO_BIMESTRE_4;

            var filtroNota = new FiltroConselhoClasseDto()
            {
                Perfil = perfil,
                Modalidade = modalidade,
                TipoCalendario = modalidadeTipoCalendario,
                Bimestre = BIMESTRE_FINAL,
                ComponenteCurricular = componente.ToString(),
                TipoNota = tipo,
                AnoTurma = anoTurma,
                ConsiderarAnoAnterior = anoAnterior,
                DataAula = dataAula,
                CriarFechamentoDisciplinaAlunoNota = criarFechamentoDisciplinaAlunoNota,
                SituacaoConselhoClasse = situacaoConselhoClasse
            };

            await CriarDadosBase(filtroNota);
        }
    }
}