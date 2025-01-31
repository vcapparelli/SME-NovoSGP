using SME.SGP.Infra;
using SME.SGP.Infra.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SME.SGP.Dominio.Interfaces
{
    public interface IRepositorioAulaConsulta : IRepositorioBase<Aula>
    {
        Task<bool> ExisteAulaNaDataAsync(DateTime data, string turmaCodigo, string componenteCurricular);

        Task<bool> ExisteAulaNaDataDataTurmaDisciplinaProfessorRfAsync(DateTime data, string turmaId, string[] disciplinasId, string professorRf, TipoAula tipoAula);

        Task<AulaConsultaDto> ObterAulaDataTurmaDisciplina(DateTime data, string turmaId, string disciplinaId);

        Task<AulaConsultaDto> ObterAulaDataTurmaDisciplinaProfessorRf(DateTime data, string turmaId, string disciplinaId, string professorRf);
        Task<DataAulaDto> ObterAulaPorCodigoTurmaComponenteEData(string turmaId, string componenteCurricularId, DateTime dataCriacao);
        Task<IEnumerable<Aula>> ObterAulasPorIds(IEnumerable<long> aulasIds);
        Task<int> ObterAulasDadasPorTurmaEPeriodoEscolar(long turmaId, long tipoCalendarioId, IEnumerable<long> periodosEscolaresIds);
        Task<PeriodoEscolarInicioFimDto> ObterPeriodoEscolarDaAula(long aulaId);
        Task<AulaConsultaDto> ObterAulaIntervaloTurmaDisciplina(DateTime dataInicio, DateTime dataFim, string turmaId, long atividadeAvaliativaId);
        Task<int> ObterAulasDadasPorTurmaDisciplinaEPeriodoEscolar(string turmaCodigo, long[] componentesCurricularesId, long tipoCalendarioId, IEnumerable<long> periodosEscolaresIds, string professor = null);
        Task<IEnumerable<AulaDto>> ObterAulas(long tipoCalendarioId, string turmaId, string ueId, string codigoRf, int? mes = null, int? semanaAno = null, string disciplinaId = null);

        Task<IEnumerable<AulaConsultaDto>> ObterAulasPorDataTurmaComponenteCurricularCJ(DateTime dataAula, string codigoTurma, string componenteCurricularCodigo, bool aulaCJ);
        Task<IEnumerable<AulaConsultaDto>> ObterAulasPorDataTurmaComponenteCurricular(DateTime dataAula, string codigoTurma, string componenteCurricularCodigo);

        Task<IEnumerable<AulaDto>> ObterAulas(long tipoCalendarioId, string turmaId, string ueId, string codigoRf);

        Task<IEnumerable<AulaDto>> ObterAulas(long tipoCalendarioId, string turmaId, string ueId, string codigoRf, int? mes = null);

        Task<IEnumerable<AulaDto>> ObterAulas(string turmaId, string ueId, string codigoRf, DateTime? data, string disciplinaId);

        Task<IEnumerable<AulaDto>> ObterAulas(string turmaId, string ueId, string codigoRf, DateTime? data, string[] disciplinasId, bool ehCj);

        Task<IEnumerable<AulaCompletaDto>> ObterAulasCompleto(long tipoCalendarioId, string turmaId, string ueId, DateTime data, Guid perfil);

        Task<IEnumerable<AulaConsultaDto>> ObterAulasPorDataTurmaComponenteCurricularProfessorRf(DateTime data, string turmaId, string[] disciplinasIdsConsideradas, string professorRf);

        Task<bool> ObterTurmaInfantilPorAula(long aulaId);

        Task<IEnumerable<Aula>> ObterAulasProfessorCalendarioPorData(string turmaCodigo, string ueCodigo, DateTime dataDaAula);

        Task<IEnumerable<Aula>> ObterAulasProfessorCalendarioPorMes(string turmaCodigo, string ueCodigo, int mes);

        Task<IEnumerable<Aula>> ObterAulasRecorrencia(long aulaPaiId, long? aulaIdInicioRecorrencia = null, DateTime? dataFinal = null);

        IEnumerable<Aula> ObterAulasReposicaoPendentes(string codigoTurma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);

        IEnumerable<Aula> ObterAulasSemFrequenciaRegistrada(string codigoTurma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);

        IEnumerable<Aula> ObterAulasSemPlanoAulaNaDataAtual(string codigoTurma, string disciplinaId, DateTime inicioPeriodo, DateTime fimPeriodo);

        Task<IEnumerable<AulasPorTurmaDisciplinaDto>> ObterAulasTurmaDisciplinaDiaProfessor(string turma, string disciplina, DateTime dataAula, string codigoRf);

        Task<int> ObterQuantidadeAulasTurmaComponenteCurricularDiaProfessor(string turma, string[] componentesCurriculares, DateTime dataAula, string codigoRf, bool ehGestor);

        Task<IEnumerable<AulasPorTurmaDisciplinaDto>> ObterAulasTurmaDisciplinaSemanaProfessor(string turma, string[] componentesCurriculares, int semana, string codigoRf);

        Task<int> ObterQuantidadeAulasTurmaDisciplinaSemanaProfessor(string turma, string[] disciplinas, int semana, string codigoRf, DateTime dataExcecao, bool ehGestor);

        Task<IEnumerable<AulasPorTurmaDisciplinaDto>> ObterAulasTurmaExperienciasPedagogicasDia(string turma, DateTime dataAula);

        Task<int> ObterQuantidadeAulasTurmaExperienciasPedagogicasDia(string turma, DateTime dataAula);

        Task<IEnumerable<AulasPorTurmaDisciplinaDto>> ObterAulasTurmaExperienciasPedagogicasSemana(string turma, int semana);

        Task<int> ObterQuantidadeAulasTurmaExperienciasPedagogicasSemana(string turma, int semana, string[] disciplinas);

        Task<Aula> ObterCompletoPorIdAsync(long id);
        Task<Aula> ObterAulaPorComponenteCurricularIdTurmaIdEData(string componenteCurricularId, string turmaId, DateTime data);
        Task<long?> ObterAulaIdPorComponenteCurricularIdTurmaIdEDataProfessor(string componenteCurricularId, string turmaId, DateTime data,string professorRf);

        Task<DateTime> ObterDataAula(long aulaId);

        Task<IEnumerable<DateTime>> ObterDatasAulasExistentes(List<DateTime> datas, string turmaId, string[] disciplinasId, bool aulaCJ, long? aulaPaiId = null);

        IEnumerable<Aula> ObterDatasDeAulasPorAnoTurmaEDisciplina(IEnumerable<long> periodosEscolaresId, int anoLetivo, string turmaCodigo, string disciplinaId, string usuarioRF, DateTime? aulaInicio, DateTime? aulaFim, bool aulaCj);
        IEnumerable<Aula> ObterDatasDeAulasPorAnoTurmaEDisciplina(long periodoEscolarId, int anoLetivo, string turmaCodigo, string disciplinaId, string usuarioRF, bool aulaCJ = false, bool ehProfessor = false);

        Aula ObterPorWorkflowId(long workflowId);

        Task<int> ObterQuantidadeDeAulasPorTurmaDisciplinaPeriodoAsync(string turmaId, string disciplinaId, DateTime inicio, DateTime fim);

        IEnumerable<DateTime> ObterUltimosDiasLetivos(DateTime dataReferencia, int quantidadeDias, long tipoCalendarioId);

        bool UsuarioPodeCriarAulaNaUeTurmaEModalidade(Aula aula, ModalidadeTipoCalendario modalidade);

        Task<IEnumerable<Aula>> ObterAulasPorTurmaETipoCalendario(long tipoCalendarioId, string turmaId, string criadoPor = null);
        Task<IEnumerable<AulaReduzidaDto>> ObterQuantidadeAulasReduzido(long turmaId, string componenteCurricularId, long tipoCalendarioId, int bimestre, bool professorCJ);

        Task<IEnumerable<AulaReduzidaDto>> ObterAulasReduzidasParaPendenciasAulaDiasNaoLetivos(long tipoCalendarioId, TipoEscola[] tiposEscola);

        Task<bool> VerificarAulaPorWorkflowId(long workflowId);

        Task<IEnumerable<Aula>> ObterAulasExcluidasComDiarioDeBordoAtivos(string codigoTurma, long tipoCalendarioId);

        Task<IEnumerable<Aula>> ObterAulasPorDataPeriodo(DateTime dataInicio, DateTime dataFim, string turmaId, string[] componentesCurricularesId, bool aulaCj, string professor = null);

        Task<IEnumerable<DiarioBordoPorPeriodoDto>> ObterDatasAulaDiarioBordoPorPeriodo(string turmaCodigo, long componenteCurricularId, DateTime dataInicio, DateTime dataFim);
        Task<IEnumerable<DiarioBordoPorPeriodoDto>> ObterAulasDiariosPorPeriodo(string turmaCodigo, string componenteCurricularFilhoCodigo, string componenteCurricularPaiCodigo, DateTime dataFim, DateTime dataInicio);
        Task<IEnumerable<TotalAulasNaoLancamNotaDto>> ObterTotalAulasPorTurmaDisciplinaAluno(string disciplinaId, string codigoTurma, string codigoAluno);

        Task<IEnumerable<AulaPossuiFrequenciaAulaRegistradaDto>> ObterDatasDeAulasPorAnoTurmaEDisciplinaVerificandoSePossuiFrequenciaAulaRegistrada(IEnumerable<long> periodosEscolaresId, int anoLetivo, string turmaCodigo,
                string[] disciplinaId, string usuarioRF, DateTime? aulaInicio, DateTime? aulaFim, bool aulaCj);

        Task<IEnumerable<RegistroFrequenciaAulaParcialDto>> ObterListaDeRegistroFrequenciaAulaPorTurma(string codigoTurma);
        Task<IEnumerable<PeriodoEscolarAulaDto>> ObterPeriodosEscolaresDasAulas(long[] aulasId);
        Task<bool> ExisteAulaNoPeriodoTurmaDisciplinaAsync(DateTime periodoInicio, DateTime periodoFim, string turmaCodigo, string disciplinaId);
    }
}
